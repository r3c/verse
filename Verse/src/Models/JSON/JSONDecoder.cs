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
		#region Attributes / Instance
		
		private ValueReader						arrayReader;
		
		private Func<T>							constructor;
		
		private Dictionary<Type, object>		converters;

        private Encoding						encoding;

        private Dictionary<string, ValueReader>	fieldReaders;

        private Linker							linker;
        
        private KeyValueReader					objectReader;
        
        #endregion

        #region Constructors

        public	JSONDecoder (Func<T> constructor, Encoding encoding, Dictionary<Type, object> converters)
        {
        	this.arrayReader = null;
			this.constructor = constructor;
			this.converters = converters;
			this.encoding = encoding;
            this.fieldReaders = new Dictionary<string, ValueReader> ();
            this.linker = null;
            this.objectReader = null;
        }
        
        #endregion
        
        #region Methods / Public

        public override bool	Decode (Stream stream, out T instance)
        {
        	T	result;

            using (JSONLexer lexer = new JSONLexer (stream, this.encoding))
            {
            	if (!lexer.Next () || !this.Extract (lexer, out result))
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
        	JSONDecoder<U>	decoder;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);

        	this.fieldReaders[name] = this.GetValueReader (decoder, setter);

            return decoder;
        }

        public override IDecoder<U>	Define<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;
        	KeyValueReader	reader;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	reader = this.GetKeyValueReader (decoder, setter);

			this.objectReader = reader;
			this.arrayReader = null;

			return decoder;
        }

        public override IDecoder<U>	Define<U> (Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;
        	ValueReader		reader;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	reader = this.GetValueReader (decoder, setter);

        	this.arrayReader = reader;
        	this.objectReader = (JSONLexer lexer, ref T container, string key) => reader (lexer, ref container);

            return decoder;
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
        	object	converter;
        	Type	type;

        	type = typeof (T);

        	if (this.converters.TryGetValue (type, out converter))
        		this.linker = this.GetConverterLinker (type, converter);
        	else if (type == typeof (char))
        		this.linker = this.GetExtractorLinker<char> (JSONConverter.ToChar);
        	else if (type == typeof (float))
        		this.linker = this.GetExtractorLinker<float> (JSONConverter.ToFloat4);
        	else if (type == typeof (double))
        		this.linker = this.GetExtractorLinker<double> (JSONConverter.ToFloat8);
        	else if (type == typeof (sbyte))
        		this.linker = this.GetExtractorLinker<sbyte> (JSONConverter.ToInt1s);
        	else if (type == typeof (byte))
        		this.linker = this.GetExtractorLinker<byte> (JSONConverter.ToInt1u);
        	else if (type == typeof (short))
        		this.linker = this.GetExtractorLinker<short> (JSONConverter.ToInt2s);
        	else if (type == typeof (ushort))
        		this.linker = this.GetExtractorLinker<ushort> (JSONConverter.ToInt2u);
        	else if (type == typeof (int))
        		this.linker = this.GetExtractorLinker<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (uint))
        		this.linker = this.GetExtractorLinker<uint> (JSONConverter.ToInt4u);
        	else if (type == typeof (long))
        		this.linker = this.GetExtractorLinker<long> (JSONConverter.ToInt8s);
        	else if (type == typeof (ulong))
        		this.linker = this.GetExtractorLinker<ulong> (JSONConverter.ToInt8u);
        	else if (type == typeof (string))
        		this.linker = this.GetExtractorLinker<string> (JSONConverter.ToString);
        	else
        		throw new LinkTypeException (type, "JSON model does not support binding for this type");
        }

        #endregion
        
        #region Methods / Protected

        protected bool	Extract (JSONLexer lexer, out T value)
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
        				else if (this.arrayReader != null)
        				{
        					if (!this.arrayReader (lexer, ref value))
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
        				else if (this.objectReader != null)
        				{
        					if (!this.objectReader (lexer, ref value, key))
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
        
        private void	GenerateLinker (DynamicMethod method, Type type, MethodInfo converter)
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
			generator.Emit (OpCodes.Callvirt, converter);
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

        private Linker	GetConverterLinker (Type type, object converter)
        {
			DynamicMethod			method;
        	LinkerConverterWrapper	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (string), typeof (object), typeof (T).MakeByRefType ()}, this.GetType ());

        	this.GenerateLinker (method, type, typeof (StringSchema.DecoderConverter<>).MakeGenericType (type).GetMethod ("Invoke"));

			wrapper = (LinkerConverterWrapper)method.CreateDelegate (typeof (LinkerConverterWrapper));

			return (JSONLexer lexer, out T target) => wrapper (lexer.AsString, converter, out target) && this.Ignore (lexer);
        }

        private Linker	GetExtractorLinker<U> (LinkerExtractor<U> extractor)
        {
			DynamicMethod				method;
        	LinkerExtractorWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), typeof (LinkerExtractor<U>), typeof (T).MakeByRefType ()}, this.GetType ());
        	
        	this.GenerateLinker (method, typeof (U), extractor.GetType ().GetMethod ("Invoke"));

			wrapper = (LinkerExtractorWrapper<U>)method.CreateDelegate (typeof (LinkerExtractorWrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, extractor, out target) && this.Ignore (lexer);
        }
        
        private KeyValueReader	GetKeyValueReader<U> (JSONDecoder<U> reader, DecoderKeyValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container, string key) =>
			{
				U	value;

				if (!reader.Extract (lexer, out value))
					return false;

				setter (ref container, key, value);

				return true;
			};
        }
        
        private ValueReader	GetValueReader<U> (JSONDecoder<U> reader, DecoderValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container) =>
    		{
    			U	value;

    			if (!reader.Extract (lexer, out value))
    				return false;

    			setter (ref container, value);

    			return true;
    		};
        }

        #endregion
        
        #region Types

        private delegate bool	KeyValueReader (JSONLexer lexer, ref T container, string key);

        private delegate bool	Linker (JSONLexer lexer, out T value);

        private delegate bool	LinkerConverterWrapper (string input, object convertor, out T value);
        
        private delegate bool	LinkerExtractor<U> (JSONLexer lexer, out U value);

        private delegate bool	LinkerExtractorWrapper<U> (JSONLexer lexer, LinkerExtractor<U> extractor, out T value);

        private delegate bool	ValueReader (JSONLexer lexer, ref T container);
        
        #endregion
    }
}
