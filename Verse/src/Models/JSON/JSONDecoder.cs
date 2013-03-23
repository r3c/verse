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
		#region Attributes / Instance
		
		private ValueReader						arrayReader;

        private Binder							binder;
		
		private Func<T>							constructor;
		
		private Dictionary<Type, object>		converters;

        private Encoding						encoding;

        private Dictionary<string, ValueReader>	fieldReaders;
        
        private KeyValueReader					objectReader;
        
        #endregion

        #region Constructors

        public	JSONDecoder (Func<T> constructor, Encoding encoding, Dictionary<Type, object> converters)
        {
        	this.arrayReader = null;
            this.binder = null;
			this.constructor = constructor;
			this.converters = converters;
			this.encoding = encoding;
            this.fieldReaders = new Dictionary<string, ValueReader> ();
            this.objectReader = null;
        }
        
        #endregion
        
        #region Methods / Public

        public override void	Bind (Func<T> builder)
        {
        	this.binder = (JSONLexer lexer, out T value) =>
        	{
        		value = builder ();

        		return this.LexerIgnore (lexer);
        	};
        }

        public override void	Bind ()
        {
        	object	converter;
        	Type	type;

        	type = typeof (T);

        	if (this.converters.TryGetValue (type, out converter))
        		this.binder = this.GetConverterBinder (type, converter);
        	else if (type == typeof (char))
        		this.binder = this.GetExtractorBinder<char> (JSONConverter.ToChar);
        	else if (type == typeof (float))
        		this.binder = this.GetExtractorBinder<float> (JSONConverter.ToFloat4);
        	else if (type == typeof (double))
        		this.binder = this.GetExtractorBinder<double> (JSONConverter.ToFloat8);
        	else if (type == typeof (sbyte))
        		this.binder = this.GetExtractorBinder<sbyte> (JSONConverter.ToInt1s);
        	else if (type == typeof (byte))
        		this.binder = this.GetExtractorBinder<byte> (JSONConverter.ToInt1u);
        	else if (type == typeof (short))
        		this.binder = this.GetExtractorBinder<short> (JSONConverter.ToInt2s);
        	else if (type == typeof (ushort))
        		this.binder = this.GetExtractorBinder<ushort> (JSONConverter.ToInt2u);
        	else if (type == typeof (int))
        		this.binder = this.GetExtractorBinder<int> (JSONConverter.ToInt4s);
        	else if (type == typeof (uint))
        		this.binder = this.GetExtractorBinder<uint> (JSONConverter.ToInt4u);
        	else if (type == typeof (long))
        		this.binder = this.GetExtractorBinder<long> (JSONConverter.ToInt8s);
        	else if (type == typeof (ulong))
        		this.binder = this.GetExtractorBinder<ulong> (JSONConverter.ToInt8u);
        	else if (type == typeof (string))
        		this.binder = this.GetExtractorBinder<string> (JSONConverter.ToString);
        	else
        		throw new BindTypeException (type, "JSON model does not support binding for this type");
        }

        public override bool	Decode (Stream stream, out T instance)
        {
        	T	result;

            using (JSONLexer lexer = new JSONLexer (stream, this.encoding))
            {
            	lexer.Next ();

            	if (!this.LexerRead (lexer, out result))
            	{
            		instance = default (T);

            		return false;
            	}

				instance = result;
            }

			return true;
        }

        public override IDecoder<U>	Field<U> (string name, Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	decoder.OnStreamError += this.EventStreamError;
        	decoder.OnTypeError += this.EventTypeError;

        	this.fieldReaders[name] = this.GetValueReader (decoder, setter);

            return decoder;
        }

        public override IDecoder<U>	Field<U> (Func<U> builder, DecoderKeyValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;
        	KeyValueReader	reader;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	decoder.OnStreamError += this.EventStreamError;
        	decoder.OnTypeError += this.EventTypeError;

        	reader = this.GetKeyValueReader (decoder, setter);

			this.objectReader = reader;
			this.arrayReader = null;

			return decoder;
        }

        public override IDecoder<U>	Field<U> (Func<U> builder, DecoderValueSetter<T, U> setter)
        {
        	JSONDecoder<U>	decoder;
        	ValueReader		reader;

        	decoder = new JSONDecoder<U> (builder, this.encoding, this.converters);
        	decoder.OnStreamError += this.EventStreamError;
        	decoder.OnTypeError += this.EventTypeError;

        	reader = this.GetValueReader (decoder, setter);

        	this.arrayReader = reader;
        	this.objectReader = (JSONLexer lexer, ref T container, string key) => reader (lexer, ref container);

            return decoder;
        }

        #endregion

        #region Methods / Private
        
        private void	GenerateBinder (DynamicMethod method, Type type, MethodInfo converter)
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

        private Binder	GetConverterBinder (Type type, object converter)
        {
			DynamicMethod			method;
        	BinderConverterWrapper	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (string), typeof (object), typeof (T).MakeByRefType ()}, this.GetType ());

        	this.GenerateBinder (method, type, typeof (StringSchema.DecoderConverter<>).MakeGenericType (type).GetMethod ("Invoke"));

			wrapper = (BinderConverterWrapper)method.CreateDelegate (typeof (BinderConverterWrapper));

			return (JSONLexer lexer, out T target) => wrapper (lexer.AsString, converter, out target) && this.LexerIgnore (lexer);
        }

        private Binder	GetExtractorBinder<U> (BinderExtractor<U> extractor)
        {
			DynamicMethod				method;
        	BinderExtractorWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), typeof (BinderExtractor<U>), typeof (T).MakeByRefType ()}, this.GetType ());
        	
        	this.GenerateBinder (method, typeof (U), extractor.GetType ().GetMethod ("Invoke"));

			wrapper = (BinderExtractorWrapper<U>)method.CreateDelegate (typeof (BinderExtractorWrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, extractor, out target) && this.LexerIgnore (lexer);
        }
        
        private KeyValueReader	GetKeyValueReader<U> (JSONDecoder<U> decoder, DecoderKeyValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container, string key) =>
			{
				U	value;

				if (!decoder.LexerRead (lexer, out value))
					return false;

				setter (ref container, key, value);

				return true;
			};
        }
        
        private ValueReader	GetValueReader<U> (JSONDecoder<U> decoder, DecoderValueSetter<T, U> setter)
        {
			return (JSONLexer lexer, ref T container) =>
    		{
    			U	value;

    			if (!decoder.LexerRead (lexer, out value))
    				return false;

    			setter (ref container, value);

    			return true;
    		};
        }

        private bool	LexerExpect (JSONLexer lexer, JSONLexem expected, string description)
        {
        	if (lexer.Lexem != expected)
        	{
        		this.EventStreamError (lexer.Position, "expected " + description);

				return false;
        	}

        	lexer.Next ();

        	return true;
        }

        private bool	LexerIgnore (JSONLexer lexer)
        {
        	switch (lexer.Lexem)
        	{
        		case JSONLexem.ArrayBegin:
        			lexer.Next ();

        			while (true)
        			{
        				if (!this.LexerIgnore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ArrayEnd)
        					break;

        				if (!this.LexerExpect (lexer, JSONLexem.Comma, "comma or end of array"))
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

        			while (true)
        			{
        				if (!this.LexerExpect (lexer, JSONLexem.String, "object property name") ||
        				    !this.LexerExpect (lexer, JSONLexem.Colon, "colon") ||
        				    !this.LexerIgnore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ObjectEnd)
        					break;

        				if (!this.LexerExpect (lexer, JSONLexem.Comma, "comma or end of object"))
        					return false;
        			}

        			lexer.Next ();

        			return true;

        		default:
        			this.EventStreamError (lexer.Position, "expected value");

        			return false;
        	}
        }

        private bool	LexerRead (JSONLexer lexer, out T value)
        {
        	string		key;
        	int			offset;
        	ValueReader	reader;

        	if (this.binder != null)
        	{
        		if (!this.binder (lexer, out value))
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
            		lexer.Next ();

            		for (offset = 0; true; ++offset)
        			{
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
        				else if (!this.LexerIgnore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ArrayEnd)
        					break;

						if (!this.LexerExpect (lexer, JSONLexem.Comma, "comma or end of array"))
        					return false;
        			}

        			lexer.Next ();

            		return true;

            	case JSONLexem.ObjectBegin:
            		lexer.Next ();

            		while (true)
        			{
        				if (lexer.Lexem != JSONLexem.String)
        				{
        					this.EventStreamError (lexer.Position, "expected object property name");

        					return false;
        				}

						key = lexer.AsString;

						lexer.Next ();

						if (!this.LexerExpect (lexer, JSONLexem.Colon, "colon"))
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
        				else if (!this.LexerIgnore (lexer))
        					return false;

        				if (lexer.Lexem == JSONLexem.ObjectEnd)
        					break;

        				if (!this.LexerExpect (lexer, JSONLexem.Comma, "comma or end of object"))
        					return false;
        			}
        			
        			lexer.Next ();

            		return true;

            	default:
            		return this.LexerIgnore (lexer);
            }
        }

        #endregion
        
        #region Types

        private delegate bool	Binder (JSONLexer lexer, out T value);

        private delegate bool	BinderConverterWrapper (string input, object converter, out T value);
        
        private delegate bool	BinderExtractor<U> (JSONLexer lexer, out U value);

        private delegate bool	BinderExtractorWrapper<U> (JSONLexer lexer, BinderExtractor<U> extractor, out T value);

        private delegate bool	KeyValueReader (JSONLexer lexer, ref T container, string key);

        private delegate bool	ValueReader (JSONLexer lexer, ref T container);
        
        #endregion
    }
}
