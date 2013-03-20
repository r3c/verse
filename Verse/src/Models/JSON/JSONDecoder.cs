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
    class JSONDecoder<T> : Decoder<T>
    {
		#region Attributes
		
		private Func<T>							constructor;

		private ValueReader						elementReader;

        private Encoding						encoding;

        private Dictionary<string, ValueReader>	fieldReaders;

        private KeyValueReader					keyValueReader;

        private Linker							linker;
        
        #endregion
        
        #region Constructors

        public JSONDecoder (Func<T> constructor, Encoding encoding)
        {
			this.constructor = constructor;
        	this.elementReader = null;
			this.encoding = encoding;
            this.fieldReaders = new Dictionary<string, ValueReader> ();
			this.keyValueReader = null;
            this.linker = null;
        }
        
        #endregion
        
        #region Methods / Public

        public override bool	Decode (Stream stream, out T instance)
        {
        	T	result;

            using (JSONLexer lexer = new JSONLexer (stream, this.encoding))
            {
            	if (!lexer.Next () || !this.Convert (lexer, out result))
            	{
            		instance = default (T);
            		
            		return false;
            	}

				instance = result;
            }

			return true;
        }

        public override IDecoder<U>	Define<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	reader;

        	reader = new JSONDecoder<U> (builder, this.encoding);

        	this.fieldReaders[name] = this.GetValueReader (reader, setter);

            return reader;
        }

        public override IDecoder<U>	Define<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	reader;

        	reader = new JSONDecoder<U> (builder, this.encoding);

			this.keyValueReader = this.GetKeyValueReader (reader, setter);

			return reader;
        }

        public override IDecoder<U>	Define<U> (Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	reader;

        	reader = new JSONDecoder<U> (builder, this.encoding);

        	this.elementReader = this.GetValueReader (reader, setter);

            return reader;
        }

        public override void	Link (Func<T> builder)
        {
        	this.linker = (JSONLexer lexer, out T value) =>
        	{
        		value = builder ();

        		return this.Ignore (lexer);
        	};
        }

        public override void	Link ()
        {
        	Type	type;

        	type = typeof (T);

        	if (type == typeof (char))
        		this.linker = this.GetLinker<char> (JSONConverter.ToChar);
        	else if (type == typeof (float))
        		this.linker = this.GetLinker<float> (JSONConverter.ToFloat4);
        	else if (type == typeof (double))
        		this.linker = this.GetLinker<double> (JSONConverter.ToFloat8);
        	else if (type == typeof (sbyte))
        		this.linker = this.GetLinker<sbyte> (JSONConverter.ToInt1s);
        	else if (type == typeof (byte))
        		this.linker = this.GetLinker<byte> (JSONConverter.ToInt1u);
        	else if (type == typeof (short))
        		this.linker = this.GetLinker<short> (JSONConverter.ToInt2s);
        	else if (type == typeof (ushort))
        		this.linker = this.GetLinker<ushort> (JSONConverter.ToInt2u);
        	else if (type == typeof (int))
        		this.linker = this.GetLinker<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (uint))
        		this.linker = this.GetLinker<uint> (JSONConverter.ToInt4u);
        	else if (type == typeof (long))
        		this.linker = this.GetLinker<long> (JSONConverter.ToInt8s);
        	else if (type == typeof (ulong))
        		this.linker = this.GetLinker<ulong> (JSONConverter.ToInt8u);
        	else if (type == typeof (string))
        		this.linker = this.GetLinker<string> (JSONConverter.ToString);
        	else
        		throw new LinkTypeException (type, "JSON model does not support binding for this type");
        }
        
        #endregion
        
        #region Methods / Protected

        protected bool	Convert (JSONLexer lexer, out T value)
        {
        	string		key;
        	int			offset;
        	ValueReader	reader;

        	if (this.linker != null)
        		return this.linker (lexer, out value);

        	value = this.constructor ();

            switch (lexer.Lexem)
            {
            	case JSONLexem.ArrayBegin:
            		for (offset = 0; true; ++offset)
        			{
        				if (!lexer.Next ())
        					return false;

        				if (this.fieldReaders.TryGetValue (offset.ToString (CultureInfo.InvariantCulture), out reader))
        				{
        					if (!reader (lexer, ref value))
        						return false;
        				}
        				else if (this.elementReader != null)
        				{
        					if (!this.elementReader (lexer, ref value))
        						return false;
        				}
        				else if (!this.Ignore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ArrayEnd)
        					break;

        				if (lexer.Lexem != JSONLexem.Comma)
        					return false;
        			}

        			lexer.Next ();

            		return true;

            	case JSONLexem.ObjectBegin:
            		while (true)
        			{
        				if (!lexer.Next () || lexer.Lexem != JSONLexem.String)
        					return false;

						key = lexer.AsString;

						if (!lexer.Next () || lexer.Lexem != JSONLexem.Colon || !lexer.Next ())
							return false;

        				if (this.fieldReaders.TryGetValue (key, out reader))
        				{
        					if (!reader (lexer, ref value))
        						return false;
        				}
        				else if (this.keyValueReader != null)
        				{
        					if (!this.keyValueReader (lexer, ref value, key))
        						return false;
        				}
        				else if (this.elementReader != null)
        				{
        					if (!this.elementReader (lexer, ref value))
        						return false;
        				}
        				else if (!this.Ignore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ObjectEnd)
        					break;

        				if (lexer.Lexem != JSONLexem.Comma)
        					return false;
        			}
        			
        			lexer.Next ();

            		return true;

            	default:
            		return this.Ignore (lexer);
            }
        }

        protected bool	Ignore (JSONLexer lexer)
        {
        	switch (lexer.Lexem)
        	{
        		case JSONLexem.ArrayBegin:
        			while (true)
        			{
        				if (!lexer.Next () || !this.Ignore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ArrayEnd)
        					break;

        				if (lexer.Lexem != JSONLexem.Comma)
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
        			while (true)
        			{
        				if (!lexer.Next () || lexer.Lexem != JSONLexem.String || !lexer.Next () || lexer.Lexem != JSONLexem.Colon || !lexer.Next () || !this.Ignore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ObjectEnd)
        					break;

        				if (lexer.Lexem != JSONLexem.Comma)
        					return false;
        			}

        			lexer.Next ();

        			return true;

        		default:
        			return false;
        	}
        }
        
        #endregion
        
        #region Methods / Private
        
        private KeyValueReader	GetKeyValueReader<U> (JSONDecoder<U> reader, DecoderKeyValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container, string key) =>
			{
				U	value;

				if (!reader.Convert (lexer, out value))
					return false;

				setter (ref container, key, value);

				return true;
			};
        }

        private Linker	GetLinker<U> (LinkerConverter<U> extractor)
        {
        	Label				assign;
        	ILGenerator			generator;
			DynamicMethod		method;
        	LocalBuilder		output;
        	LinkerWrapper<U>	wrapper;

			method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), extractor.GetType (), typeof (T).MakeByRefType ()}, this.GetType ());
			generator = method.GetILGenerator ();

			assign = generator.DefineLabel ();
			output = generator.DeclareLocal (typeof (U));

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldloca_S, output);
			generator.Emit (OpCodes.Callvirt, extractor.GetType ().GetMethod ("Invoke"));
			generator.Emit (OpCodes.Brtrue_S, assign);
			generator.Emit (OpCodes.Ldc_I4_0);
			generator.Emit (OpCodes.Ret);

			generator.MarkLabel (assign);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Ldloc, output);
			generator.Emit (OpCodes.Stind_Ref);
			generator.Emit (OpCodes.Ldc_I4_1);
			generator.Emit (OpCodes.Ret);

			wrapper = (LinkerWrapper<U>)method.CreateDelegate (typeof (LinkerWrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, extractor, out target) && this.Ignore (lexer);
        }
        
        private ValueReader	GetValueReader<U> (JSONDecoder<U> reader, DecoderValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container) =>
    		{
    			U	value;

    			if (!reader.Convert (lexer, out value))
    				return false;

    			setter (ref container, value);

    			return true;
    		};
        }

        #endregion
        
        #region Types

        private delegate bool	KeyValueReader (JSONLexer lexer, ref T container, string key);

        private delegate bool	Linker (JSONLexer lexer, out T value);
        
        private delegate bool	LinkerConverter<U> (JSONLexer lexer, out U value);

        private delegate bool	LinkerWrapper<U> (JSONLexer lexer, LinkerConverter<U> extractor, out T value);

        private delegate bool	ValueReader (JSONLexer lexer, ref T container);
        
        #endregion
    }
}
