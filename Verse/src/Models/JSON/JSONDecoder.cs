using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Verse.Models.JSON
{
    class JSONDecoder<T> : ConvertDecoder<string, T>
    {
		#region Attributes
		
		private Browser						arrayBrowser;

		private Func<T>						constructor;

		private Encoding					encoding;

		private Dictionary<string, Browser>	fieldBrowsers;
        
		private Browser						objectBrowser;

		private Reader						selfReader;

        #endregion

        #region Constructors

        public	JSONDecoder (Dictionary<Type, object> converters, Encoding encoding, Func<T> constructor) :
        	base (converters)
        {
        	this.arrayBrowser = null;
			this.constructor = constructor;
			this.encoding = encoding;
			this.fieldBrowsers = new Dictionary<string, Browser> ();
 			this.objectBrowser = null;
			this.selfReader = null;
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

        public override IDecoder<U>	HasField<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (generator);

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

        public override IDecoder<U>	HasItems<U> (Func<U> generator, DecoderArraySetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (generator);

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

        public override IDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = this.BuildDecoder (generator);

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

		#region Methods / Protected

        protected override bool	TryLinkConvert (ConvertSchema<string>.DecoderConverter<T> converter)
        {
        	this.selfReader = (JSONLexer lexer, out T value) =>
        	{
        		switch (lexer.Lexem)
        		{
        			case JSONLexem.Null:
    					value = default (T);

    					return this.Ignore (lexer);

        			case JSONLexem.String:
        				if (converter (lexer.AsString, out value))
        					return this.Ignore (lexer);

        				goto default;

					default:
						value = default (T);

						return false;
        		}
        	};

			return true;
        }
        
		protected override bool	TryLinkNative ()
		{
        	Type	type;

        	type = typeof (T);

        	if (type.IsEnum)
        		this.selfReader = this.BuildReader<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (bool))
        		this.selfReader = this.BuildReader<bool> (JSONConverter.ToBoolean);
        	else if (type == typeof (char))
        		this.selfReader = this.BuildReader<char> (JSONConverter.ToChar);
        	else if (type == typeof (float))
        		this.selfReader = this.BuildReader<float> (JSONConverter.ToFloat4);
        	else if (type == typeof (double))
        		this.selfReader = this.BuildReader<double> (JSONConverter.ToFloat8);
        	else if (type == typeof (sbyte))
        		this.selfReader = this.BuildReader<sbyte> (JSONConverter.ToInt1s);
        	else if (type == typeof (byte))
        		this.selfReader = this.BuildReader<byte> (JSONConverter.ToInt1u);
        	else if (type == typeof (short))
        		this.selfReader = this.BuildReader<short> (JSONConverter.ToInt2s);
        	else if (type == typeof (ushort))
        		this.selfReader = this.BuildReader<ushort> (JSONConverter.ToInt2u);
        	else if (type == typeof (int))
        		this.selfReader = this.BuildReader<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (uint))
        		this.selfReader = this.BuildReader<uint> (JSONConverter.ToInt4u);
        	else if (type == typeof (long))
        		this.selfReader = this.BuildReader<long> (JSONConverter.ToInt8s);
        	else if (type == typeof (ulong))
        		this.selfReader = this.BuildReader<ulong> (JSONConverter.ToInt8u);
        	else if (type == typeof (string))
        		this.selfReader = this.BuildReader<string> (JSONConverter.ToString);
        	else
        		return false;

			return true;
        }

		#endregion

        #region Methods / Private

        private JSONDecoder<U>	BuildDecoder<U> (Func<U> builder)
        {
        	JSONDecoder<U>	decoder;

        	decoder = new JSONDecoder<U> (this.converters, this.encoding, builder);
        	decoder.OnStreamError += this.EventStreamError;
        	decoder.OnTypeError += this.EventTypeError;

        	return decoder;
        }

        private Reader	BuildReader<U> (ReaderExtractor<U> extractor)
        {
        	ILGenerator			generator;
			DynamicMethod		method;
        	Label				success;
        	ReaderWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), typeof (ReaderExtractor<U>), typeof (T).MakeByRefType ()}, typeof (JSONDecoder<T>).Module, true);
			generator = method.GetILGenerator ();
			success = generator.DefineLabel ();

			generator.DeclareLocal (typeof (U));
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldloca_S, 0);
			generator.Emit (OpCodes.Call, typeof (ReaderExtractor<U>).GetMethod ("Invoke"));
			generator.Emit (OpCodes.Brtrue_S, success);
			generator.Emit (OpCodes.Ldc_I4_0);
			generator.Emit (OpCodes.Ret);

			generator.MarkLabel (success);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Ldloc_0);

			if (typeof (U).IsValueType)
				generator.Emit (OpCodes.Stobj, typeof (U));
			else
				generator.Emit (OpCodes.Stind_Ref);

			generator.Emit (OpCodes.Ldc_I4_1);
			generator.Emit (OpCodes.Ret);

			wrapper = (ReaderWrapper<U>)method.CreateDelegate (typeof (ReaderWrapper<U>));

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

        	if (this.selfReader != null)
        	{
        		if (!this.selfReader (lexer, out value))
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
        
        private delegate bool	ReaderExtractor<U> (JSONLexer lexer, out U value);

        private delegate bool	ReaderWrapper<U> (JSONLexer lexer, ReaderExtractor<U> extractor, out T value);
        
        #endregion
    }
}
