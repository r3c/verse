using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Verse.Dynamics;

namespace Verse.Models.JSON
{
	class JSONEncoder<T> : ConvertEncoder<string, T>
	{
		#region Attributes

		private Encoding							encoding;

		private Dictionary<string, Writer>			fieldWriters;

		private Func<Stream, Encoding, JSONPrinter>	generator;

		private JSONInjector<T>						injector;

		private bool								settingOmitNull;

		private JSONSettings						settings;

		#endregion

		#region Constructors

		public	JSONEncoder (Dictionary<Type, object> converters, JSONSettings settings, Encoding encoding, Func<Stream, Encoding, JSONPrinter> generator) :
			base (converters)
		{
			this.encoding = encoding;
			this.fieldWriters = new Dictionary<string, Writer> ();
			this.generator = generator;
			this.injector = null;
			this.settingOmitNull = (settings & JSONSettings.OmitNull) == JSONSettings.OmitNull;
			this.settings = settings;
		}

		#endregion

		#region Methods / Public

		public override void	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.DefineAttribute (name, getter, (JSONEncoder<U>)encoder);
		}

		public override void	HasElements<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.DefineElements (getter, (JSONEncoder<U>)encoder);
		}

		public override void	HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.DefinePairs (getter, (JSONEncoder<U>)encoder);
		}

		public override bool	Encode (Stream stream, T instance)
		{
			using (JSONPrinter writer = this.generator (stream, this.encoding))
			{
				return this.Write (writer, instance);
			}
		}

		#endregion

		#region Methods / Protected

		protected override AbstractEncoder<U>	DefineAttribute<U> (string name, EncoderValueGetter<T, U> getter)
		{
			return this.DefineAttribute (name, getter, this.BuildEncoder<U> ());
		}

		protected override AbstractEncoder<U>	DefineElements<U> (EncoderArrayGetter<T, U> getter)
		{
			return this.DefineElements (getter, this.BuildEncoder<U> ());
		}

		protected override AbstractEncoder<U>	DefinePairs<U> (EncoderMapGetter<T, U> getter)
		{
			return this.DefinePairs (getter, this.BuildEncoder<U> ());
		}

		protected override bool	TryLinkConvert (ConvertSchema<string>.EncoderConverter<T> converter)
		{
			this.injector = (writer, input) =>
			{
				string	value;

				if (!converter (input, out value))
					return false;

				if (value != null)
					writer.WriteString (value);
				else
					writer.WriteNull ();

				return true;
			};

			return true;
		}

		protected override bool	TryLinkNative ()
		{
			Type[]	arguments;
			object	injector;
			Type	type;

			type = typeof (T);

			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (Nullable<>))
			{
				arguments = type.GetGenericArguments ();

				if (arguments.Length == 1 && JSONConverter.TryGetInjector (arguments[0], out injector))
				{
					this.injector = (JSONInjector<T>)Resolver<JSONEncoder<T>>
						.Method<int, JSONInjector<T>> ((e, i) => e.WrapNullable<int> (i), null, new Type[] {arguments[0]})
						.Invoke (this, new object[] {injector});

					return true;
				}
			}

			if (type.IsEnum && JSONConverter.TryGetInjector (typeof (int), out injector))
			{
				this.injector = this.WrapCompatible<int> (injector);

				return true;
			}

			if (JSONConverter.TryGetInjector (type, out injector))
			{
				this.injector = (JSONInjector<T>)injector;

				return true;
			}

			return false;
		}

		#endregion

		#region Methods / Private

		private JSONEncoder<U>	BuildEncoder<U> ()
		{
			JSONEncoder<U>	encoder;

			encoder = new JSONEncoder<U> (this.converters, this.settings, this.encoding, this.generator);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}

		private JSONEncoder<U>	DefineAttribute<U> (string name, EncoderValueGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.fieldWriters[name] = (writer, container) => encoder.Write (writer, getter (container));

			return encoder;
		}

		private JSONEncoder<U>	DefineElements<U> (EncoderArrayGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.injector = (writer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (U value in getter (container))
				{
					if (value == null && this.settingOmitNull)
						continue;

					if (empty)
					{
						writer.WriteArrayBegin (false);

						empty = false;
					}
					else
						writer.WriteComma ();

					if (!encoder.Write (writer, value))
						return false;
				}

				if (empty)
					writer.WriteArrayBegin (true);

				writer.WriteArrayEnd (empty);

				return true;
			};

			return encoder;
		}

		private JSONEncoder<U>	DefinePairs<U> (EncoderMapGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.injector = (writer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (KeyValuePair<string, U> pair in getter (container))
				{
					if (pair.Value == null && this.settingOmitNull)
						continue;

					if (empty)
					{
						writer.WriteObjectBegin (false);

						empty = false;
					}
					else
						writer.WriteComma ();

					writer.WriteString (pair.Key);
					writer.WriteColon ();

					if (!encoder.Write (writer, pair.Value))
						return false;
				}

				if (empty)
					writer.WriteObjectBegin (true);

				writer.WriteObjectEnd (empty);

				return true;
			};

			return encoder;
		}

		private JSONInjector<T>	WrapCompatible<U> (object injector)
		{
			JSONInjector<U>	closure;
			ILGenerator		generator;
			DynamicMethod	method;
			Wrapper<U>		wrapper;

			method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONPrinter), typeof (JSONInjector<U>), typeof (T)}, typeof (JSONEncoder<T>).Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Call, Resolver<JSONInjector<U>>.Method<JSONPrinter, U> ((i, p, v) => i.Invoke (p, v)));
			generator.Emit (OpCodes.Ret);

			closure = (JSONInjector<U>)injector;
			wrapper = (Wrapper<U>)method.CreateDelegate (typeof (Wrapper<U>));

			return (writer, value) => wrapper (writer, closure, value);
		}

		private JSONInjector<T>	WrapNullable<U> (object injector) where
			U : struct
		{
			JSONInjector<U>	closure;

			closure = (JSONInjector<U>)injector;

			return this.WrapCompatible<U?> ((JSONInjector<U?>)((JSONPrinter printer, U? nullable) =>
			{
				if (nullable.HasValue)
					return closure (printer, nullable.GetValueOrDefault ());

				printer.WriteNull ();

				return true;
			}));
		}

		private bool	Write (JSONPrinter printer, T value)
		{
			bool	empty;

			if (value == null)
			{
				printer.WriteNull ();

				return true;
			}

			if (this.injector != null)
				return this.injector (printer, value);

			empty = true;

			foreach (KeyValuePair<string, Writer> field in this.fieldWriters)
			{
				if (empty)
				{
					printer.WriteObjectBegin (false);

					empty = false;
				}
				else
					printer.WriteComma ();

				printer.WriteString (field.Key);
				printer.WriteColon ();

				if (!field.Value (printer, value))
					return false;
			}

			if (empty)
				printer.WriteObjectBegin (true);

			printer.WriteObjectEnd (empty);

			return true;
		}

		#endregion

		#region Types

		private delegate bool	Wrapper<U> (JSONPrinter printer, JSONInjector<U> injector, T value);

		private delegate bool	Writer (JSONPrinter printer, T value);
		
		#endregion
	}
}
