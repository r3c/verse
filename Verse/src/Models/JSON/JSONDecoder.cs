using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Verse.Dynamics;

namespace Verse.Models.JSON
{
	class JSONDecoder<T> : ConvertDecoder<string, T>
	{
		#region Attributes
		
		private Reader						arrayReader;

		private Func<T>						constructor;

		private Encoding					encoding;

		private JSONExtractor<T>			extractor;

		private Dictionary<string, Reader>	fieldReaders;
		
		private Reader						objectReader;

		private JSONSettings				settings;

		#endregion

		#region Constructors

		public	JSONDecoder (Dictionary<Type, object> converters, JSONSettings settings, Encoding encoding, Func<T> constructor) :
			base (converters)
		{
			this.arrayReader = null;
			this.constructor = constructor;
			this.encoding = encoding;
			this.extractor = null;
			this.fieldReaders = new Dictionary<string, Reader> ();
 			this.objectReader = null;
			this.settings = settings;
		}
		
		#endregion
		
		#region Methods / Public

		public override bool	Decode (Stream stream, out T instance)
		{
			T	result;

			using (JSONLexer lexer = new JSONLexer (stream, this.encoding))
			{
				lexer.Next ();

				if (!this.Read (lexer, out result))
				{
					instance = default (T);

					return false;
				}

				instance = result;
			}

			return true;
		}

		public override void	HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter, IDecoder<U> decoder)
		{
			if (!(decoder is JSONDecoder<U>))
				throw new ArgumentException ("nested decoder must be a JSON decoder", "decoder");

			this.HasAttribute (name, generator, setter, (JSONDecoder<U>)decoder);
		}

		public override void	HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter, IDecoder<U> decoder)
		{
			if (!(decoder is JSONDecoder<U>))
				throw new ArgumentException ("nested decoder must be a JSON decoder", "decoder");

			this.HasElements (generator, setter, (JSONDecoder<U>)decoder);
		}

		public override void	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter, IDecoder<U> decoder)
		{
			if (!(decoder is JSONDecoder<U>))
				throw new ArgumentException ("nested decoder must be a JSON decoder", "decoder");

			this.HasPairs (generator, setter, (JSONDecoder<U>)decoder);
		}

		#endregion

		#region Methods / Protected

		protected override AbstractDecoder<U>	DefineAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter)
		{
			return this.HasAttribute (name, generator, setter, this.BuildDecoder (generator));
		}

		protected override AbstractDecoder<U>	DefineElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter)
		{
			return this.HasElements (generator, setter, this.BuildDecoder (generator));
		}

		protected override AbstractDecoder<U>	DefinePairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter)
		{
			return this.HasPairs (generator, setter, this.BuildDecoder (generator));
		}

		protected override bool	TryLinkConvert (ConvertSchema<string>.DecoderConverter<T> converter)
		{
			this.extractor = (JSONLexer lexer, out T value) =>
			{
				switch (lexer.Lexem)
				{
					case JSONLexem.Null:
						value = default (T);

						return true;

					case JSONLexem.String:
						return converter (lexer.AsString, out value);

					default:
						value = default (T);

						return false;
				}
			};

			return true;
		}

		protected override bool	TryLinkNative ()
		{
			Type[]	arguments;
			object	extractor;
			Type	type;

			type = typeof (T);

			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>))
			{
				arguments = type.GetGenericArguments ();

				if (arguments.Length == 1 && JSONLexer.TryGetExtractor (arguments[0], out extractor))
				{
					this.extractor = (JSONExtractor<T>)Resolver<JSONDecoder<T>>
						.Method<int, JSONExtractor<T>> ((d, e) => d.WrapNullable<int> (e), null, new Type[] {arguments[0]})
						.Invoke (this, new object[] {extractor});

					return true;
				}
			}

			if (type.IsEnum && JSONLexer.TryGetExtractor (typeof (int), out extractor))
			{
				this.extractor = this.WrapCompatible<int> (extractor);

				return true;
			}

			if (JSONLexer.TryGetExtractor (type, out extractor))
			{
				this.extractor = (JSONExtractor<T>)extractor;

				return true;
			}

			return false;
		}

		#endregion

		#region Methods / Private

		private JSONDecoder<U>	BuildDecoder<U> (Func<U> builder)
		{
			JSONDecoder<U>	decoder;

			decoder = new JSONDecoder<U> (this.converters, this.settings, this.encoding, builder);
			decoder.OnStreamError += this.EventStreamError;
			decoder.OnTypeError += this.EventTypeError;

			return decoder;
		}

		private bool	Expect (JSONLexer lexer, JSONLexem expected, string description)
		{
			if (lexer.Lexem != expected)
			{
				this.EventStreamError (lexer.Position, "expected " + description);

				return false;
			}

			lexer.Next ();

			return true;
		}

		private JSONDecoder<U>	HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter, JSONDecoder<U> decoder)
		{
			this.fieldReaders[name] = (JSONLexer lexer, ref T container) =>
			{
				U	value;

				if (!decoder.Read (lexer, out value))
					return false;

				setter (ref container, value);

				return true;
			};

			return decoder;
		}

		private JSONDecoder<U>	HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter, JSONDecoder<U> decoder)
		{
			this.arrayReader = (JSONLexer lexer, ref T container) =>
			{
				List<U>	array;
				U		value;

				array = new List<U> ();

				for (lexer.Next (); lexer.Lexem != JSONLexem.ArrayEnd; )
				{
					if (array.Count > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of array"))
						return false;
					else if (!decoder.Read (lexer, out value))
						return false;

					array.Add (value);
				}

				lexer.Next ();

				setter (ref container, array);

				return true;
			};

			return decoder;
		}

		private JSONDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter, JSONDecoder<U> decoder)
		{
			this.objectReader = (JSONLexer lexer, ref T container) =>
			{
				string							key;
				List<KeyValuePair<string, U>>	map;
				U								value;

				map = new List<KeyValuePair<string, U>> ();

				for (lexer.Next (); lexer.Lexem != JSONLexem.ObjectEnd; )
				{
					if (map.Count > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of object"))
						return false;

					if (lexer.Lexem != JSONLexem.String)
					{
						this.EventStreamError (lexer.Position, "expected object property name");

						return false;
					}

					key = lexer.AsString;

					lexer.Next ();

					if (!this.Expect (lexer, JSONLexem.Colon, "colon"))
						return false;
					else if (!decoder.Read (lexer, out value))
						return false;

					map.Add (new KeyValuePair<string, U> (key, value));
				}

				lexer.Next ();

				setter (ref container, map);

				return true;
			};

			return decoder;
		}

		private bool	Ignore (JSONLexer lexer)
		{
			bool	separator;

			switch (lexer.Lexem)
			{
				case JSONLexem.ArrayBegin:
					lexer.Next ();

					for (separator = false; lexer.Lexem != JSONLexem.ArrayEnd; separator = true)
					{
						if (separator && !this.Expect (lexer, JSONLexem.Comma, "comma or end of array"))
							return false;

						if (!this.Ignore (lexer))
							return false;
					}

					lexer.Next ();

					return true;

				case JSONLexem.False:
				case JSONLexem.Null:
				case JSONLexem.Number:
				case JSONLexem.String:
				case JSONLexem.True:
					lexer.Next ();

					return true;

				case JSONLexem.ObjectBegin:
					lexer.Next ();

					for (separator = false; lexer.Lexem != JSONLexem.ObjectEnd; separator = true)
					{
						if (separator && !this.Expect (lexer, JSONLexem.Comma, "comma or end of object"))
							return false;

						if (!this.Expect (lexer, JSONLexem.String, "object property name") ||
						    !this.Expect (lexer, JSONLexem.Colon, "colon") ||
						    !this.Ignore (lexer))
							return false;
					}

					lexer.Next ();

					return true;

				default:
					this.EventStreamError (lexer.Position, "expected value");

					return false;
			}
		}

		private bool	Read (JSONLexer lexer, out T value)
		{
			int		index;
			string	key;
			Reader	reader;

			if (this.extractor != null)
			{
				if (!this.extractor (lexer, out value))
				{
					this.EventTypeError (typeof (T), lexer.ToString ());

					return false;
				}

				return this.Ignore (lexer);
			}

			switch (lexer.Lexem)
			{
				case JSONLexem.ArrayBegin:
					value = this.constructor ();

					if (this.arrayReader != null)
						return this.arrayReader (lexer, ref value);

					lexer.Next ();

					for (index = 0; lexer.Lexem != JSONLexem.ArrayEnd; ++index)
					{
						if (index > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of array"))
							return false;

						if (this.fieldReaders.TryGetValue (index.ToString (CultureInfo.InvariantCulture), out reader))
						{
							if (!reader (lexer, ref value))
								return false;
						}
						else if (!this.Ignore (lexer))
							return false;
					}

					lexer.Next ();

					return true;

				case JSONLexem.ObjectBegin:
					value = this.constructor ();

					if (this.objectReader != null)
						return this.objectReader (lexer, ref value);

					lexer.Next ();

					for (index = 0; lexer.Lexem != JSONLexem.ObjectEnd; ++index)
					{
						if (index > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of object"))
							return false;

						if (lexer.Lexem != JSONLexem.String)
						{
							this.EventStreamError (lexer.Position, "expected object property name");

							return false;
						}

						key = lexer.AsString;

						lexer.Next ();

						if (!this.Expect (lexer, JSONLexem.Colon, "colon"))
							return false;

						if (this.fieldReaders.TryGetValue (key, out reader))
						{
							if (!reader (lexer, ref value))
								return false;
						}
						else if (!this.Ignore (lexer))
							return false;
					}

					lexer.Next ();

					return true;

				default:
					value = default (T);

					return this.Ignore (lexer);
			}
		}

		private JSONExtractor<T>	WrapCompatible<U> (object extractor)
		{
			JSONExtractor<U>	closure;
			Label				failure;
			ILGenerator			generator;
			DynamicMethod		method;
			Type				type;
			Wrapper<U>			wrapper;

			method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), typeof (JSONExtractor<U>), typeof (T).MakeByRefType ()}, typeof (JSONDecoder<T>).Module, true);
			generator = method.GetILGenerator ();
			failure = generator.DefineLabel ();
			type = typeof (U);

			generator.DeclareLocal (type);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldloca_S, 0);
			generator.Emit (OpCodes.Call, typeof (JSONExtractor<U>).GetMethod ("Invoke")); // Can't use static reflection here
			generator.Emit (OpCodes.Brfalse_S, failure);

			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Ldloc_0);

			if (type.IsValueType)
				generator.Emit (OpCodes.Stobj, type);
			else
				generator.Emit (OpCodes.Stind_Ref);

			generator.Emit (OpCodes.Ldc_I4_1);
			generator.Emit (OpCodes.Ret);

			generator.MarkLabel (failure);
			generator.Emit (OpCodes.Ldc_I4_0);
			generator.Emit (OpCodes.Ret);

			closure = (JSONExtractor<U>)extractor;
			wrapper = (Wrapper<U>)method.CreateDelegate (typeof (Wrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, closure, out target);
		}

		private JSONExtractor<T>	WrapNullable<U> (object extractor) where
			U : struct
		{
			JSONExtractor<U>	closure;

			closure = (JSONExtractor<U>)extractor;

			return this.WrapCompatible<U?> ((JSONExtractor<U?>)((JSONLexer lexer, out U? nullable) =>
			{
				U	value;

				if (lexer.Lexem == JSONLexem.Null)
				{
					nullable = null;

					return true;
				}

				if (closure (lexer, out value))
				{
					nullable = value;

					return true;
			    }

				nullable = null;

				return false;
			}));
		}

		#endregion
		
		#region Types

		private delegate bool	Reader (JSONLexer lexer, ref T container);

		private delegate bool	Wrapper<U> (JSONLexer lexer, JSONExtractor<U> extractor, out T value);

		#endregion
	}
}
