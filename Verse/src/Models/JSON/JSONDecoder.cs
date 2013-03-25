using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Verse.Exceptions;

namespace Verse.Models.JSON
{
    class JSONDecoder<T> : AbstractDecoder<T>
    {
		#region Attributes
		
		private Browser						arrayBrowser;

		private Func<T>						constructor;
		
		private Dictionary<Type, object>	converters;

		private Encoding					encoding;

		private Dictionary<string, Browser>	fieldBrowsers;
        
		private Browser						objectBrowser;

		private Reader						reader;

        #endregion

        #region Constructors

        public	JSONDecoder (Func<T> constructor, Encoding encoding, Dictionary<Type, object> converters)
        {
        	this.arrayBrowser = null;
			this.constructor = constructor;
			this.converters = converters;
			this.encoding = encoding;
			this.fieldBrowsers = new Dictionary<string, Browser> ();
 			this.objectBrowser = null;
			this.reader = null;
        }
        
        #endregion
        
        #region Methods / Public

        public override void	Bind (Func<T> builder)
        {
        	this.reader = (JSONLexer lexer, out T value) =>
        	{
        		value = builder ();

        		return this.Ignore (lexer);
        	};
        }

        public override void	Bind ()
        {
        	object	converter;
        	Type	type;

        	type = typeof (T);

        	if (this.converters.TryGetValue (type, out converter))
        		this.reader = this.BuildReader (converter);
        	else if (type == typeof (bool))
        		this.reader = this.BuildReader<bool> (JSONConverter.ToBoolean);
        	else if (type == typeof (char))
        		this.reader = this.BuildReader<char> (JSONConverter.ToChar);
        	else if (type == typeof (float))
        		this.reader = this.BuildReader<float> (JSONConverter.ToFloat4);
        	else if (type == typeof (double))
        		this.reader = this.BuildReader<double> (JSONConverter.ToFloat8);
        	else if (type == typeof (sbyte))
        		this.reader = this.BuildReader<sbyte> (JSONConverter.ToInt1s);
        	else if (type == typeof (byte))
        		this.reader = this.BuildReader<byte> (JSONConverter.ToInt1u);
        	else if (type == typeof (short))
        		this.reader = this.BuildReader<short> (JSONConverter.ToInt2s);
        	else if (type == typeof (ushort))
        		this.reader = this.BuildReader<ushort> (JSONConverter.ToInt2u);
        	else if (type == typeof (int))
        		this.reader = this.BuildReader<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (uint))
        		this.reader = this.BuildReader<uint> (JSONConverter.ToInt4u);
        	else if (type == typeof (long))
        		this.reader = this.BuildReader<long> (JSONConverter.ToInt8s);
        	else if (type == typeof (ulong))
        		this.reader = this.BuildReader<ulong> (JSONConverter.ToInt8u);
        	else if (type == typeof (string))
        		this.reader = this.BuildReader<string> (JSONConverter.ToString);
        	else
        		throw new BindTypeException (type, "no converter for this type has been defined");
        }

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

        public override IDecoder<U>	HasField<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (builder);

        	this.fieldBrowsers[name] = (JSONLexer lexer, ref T container) =>
    		{
    			U	value;

    			if (!decoder.Read (lexer, out value))
    				return false;

    			setter (ref container, value);

    			return true;
    		};

            return decoder;
        }

        public override IDecoder<U>	HasItems<U> (Func<U> builder, DecoderArraySetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (builder);

        	this.arrayBrowser = (JSONLexer lexer, ref T container) =>
        	{
        		List<U>	array;
        		U		value;

        		array = new List<U> ();

        		for (lexer.Next (); lexer.Lexem != JSONLexem.ArrayEnd; )
    			{
    				if (array.Count > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of array"))
    					return false;

    				if (!decoder.Read (lexer, out value))
    					return false;

    				array.Add (value);
    			}

    			lexer.Next ();

    			setter (ref container, array);

    			return true;
        	};

            return decoder;
        }

        public override IDecoder<U>	HasPairs<U> (Func<U> builder, DecoderMapSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (builder);

        	this.objectBrowser = (JSONLexer lexer, ref T container) =>
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

					if (!this.Expect (lexer, JSONLexem.Colon, "colon") ||
					    !decoder.Read (lexer, out value))
						return false;

					map.Add (new KeyValuePair<string, U> (key, value));
    			}

    			lexer.Next ();

    			setter (ref container, map);

    			return true;
			};

            return decoder;
        }

        #endregion

        #region Methods / Private

        private JSONDecoder<U>	BuildDecoder<U> (Func<U> builder)
        {
        	JSONDecoder<U>	decoder;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	decoder.OnStreamError += this.EventStreamError;
        	decoder.OnTypeError += this.EventTypeError;

        	return decoder;
        }

        private Reader	BuildReader (object converter)
        {
			DynamicMethod			method;
        	ReaderConverterWrapper	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (string), typeof (object), typeof (T).MakeByRefType ()}, this.GetType ());

        	#warning FIXME: this method should not be common to both BuildReader signatures
        	this.GenerateReader (method, typeof (T), typeof (StringSchema.DecoderConverter<>).MakeGenericType (typeof (T)).GetMethod ("Invoke"));

			wrapper = (ReaderConverterWrapper)method.CreateDelegate (typeof (ReaderConverterWrapper));

			return (JSONLexer lexer, out T target) => wrapper (lexer.AsString, converter, out target) && this.Ignore (lexer);
        }

        private Reader	BuildReader<U> (ReaderExtractor<U> extractor)
        {
			DynamicMethod				method;
        	ReaderExtractorWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), typeof (ReaderExtractor<U>), typeof (T).MakeByRefType ()}, this.GetType ());

        	#warning FIXME: this method should not be common to both BuildReader signatures
        	this.GenerateReader (method, typeof (U), extractor.GetType ().GetMethod ("Invoke"));

			wrapper = (ReaderExtractorWrapper<U>)method.CreateDelegate (typeof (ReaderExtractorWrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, extractor, out target) && this.Ignore (lexer);
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

        private void	GenerateReader (DynamicMethod method, Type type, MethodInfo extractor)
        {
        	Label			assign;
        	ILGenerator		generator;
        	LocalBuilder	output;

			generator = method.GetILGenerator ();

			assign = generator.DefineLabel ();
			output = generator.DeclareLocal (type);

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldloca_S, output);
			generator.Emit (OpCodes.Callvirt, extractor);
			generator.Emit (OpCodes.Brtrue_S, assign);
			generator.Emit (OpCodes.Ldc_I4_0);
			generator.Emit (OpCodes.Ret);

			generator.MarkLabel (assign);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Ldloc, output);

			if (type.IsValueType)
				generator.Emit (OpCodes.Stobj, type);
			else
				generator.Emit (OpCodes.Stind_Ref);

			generator.Emit (OpCodes.Ldc_I4_1);
			generator.Emit (OpCodes.Ret);
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
        	Browser	browser;
        	int		index;
        	string	key;

        	if (this.reader != null)
        	{
        		if (!this.reader (lexer, out value))
        		{
        			this.EventTypeError (typeof (T), lexer.ToString ());

        			return false;
        		}

				return true;
        	}

        	value = this.constructor ();

            switch (lexer.Lexem)
            {
            	case JSONLexem.ArrayBegin:
            		if (this.arrayBrowser != null)
            			return this.arrayBrowser (lexer, ref value);

            		lexer.Next ();

            		for (index = 0; lexer.Lexem != JSONLexem.ArrayEnd; ++index)
        			{
        				if (index > 0 && !this.Expect (lexer, JSONLexem.Comma, "comma or end of array"))
        					return false;

        				if (this.fieldBrowsers.TryGetValue (index.ToString (CultureInfo.InvariantCulture), out browser))
        				{
        					if (!browser (lexer, ref value))
        						return false;
        				}
        				else if (!this.Ignore (lexer))
        					return false;
        			}

        			lexer.Next ();

            		return true;

            	case JSONLexem.ObjectBegin:
            		if (this.objectBrowser != null)
            			return this.objectBrowser (lexer, ref value);

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

        				if (this.fieldBrowsers.TryGetValue (key, out browser))
        				{
        					if (!browser (lexer, ref value))
        						return false;
        				}
        				else if (!this.Ignore (lexer))
        					return false;
        			}
        			
        			lexer.Next ();

            		return true;

            	default:
            		return this.Ignore (lexer);
            }
        }

        #endregion
        
        #region Types

        private delegate bool	Browser (JSONLexer lexer, ref T container);

        private delegate bool	Reader (JSONLexer lexer, out T value);

        private delegate bool	ReaderConverterWrapper (string input, object converter, out T value);
        
        private delegate bool	ReaderExtractor<U> (JSONLexer lexer, out U value);

        private delegate bool	ReaderExtractorWrapper<U> (JSONLexer lexer, ReaderExtractor<U> extractor, out T value);
        
        #endregion
    }
}
