using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reflection.Emit;
using System.Text;

using Verse.Exceptions;

namespace Verse.Models.JSON
{
    class JSONDecoder<T> : Decoder<T>
    {
		#region Attributes

		private Func<T>							constructor;

        private Encoding						encoding;

        private Dictionary<string, FieldReader>	fields;

        private Linker							linker;
        
        private SubReader						subscript;
        
        #endregion
        
        #region Constructors

        public JSONDecoder (Func<T> constructor, Encoding encoding)
        {
            this.linker = null;
			this.constructor = constructor;
			this.encoding = encoding;
            this.fields = new Dictionary<string, FieldReader> ();
            this.subscript = null;
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

        public override IDecoder<U>	Define<U> (string name, Func<U> builder, DecoderFieldSetter<T, U> setter)
        {
        	JSONDecoder<U>	reader;

        	reader = new JSONDecoder<U> (builder, this.encoding);

        	this.fields[name] = (JSONLexer lexer, ref T container) =>
        	{
        		U	value;

        		if (!reader.Convert (lexer, out value))
        			return false;

        		setter (ref container, value);

				return true;
        	};

            return reader;
        }

        public override IDecoder<U>	Define<U> (Func<U> builder, DecoderSubSetter<T, U> setter)
        {
        	JSONDecoder<U>	reader;

        	reader = new JSONDecoder<U> (builder, this.encoding);

        	this.subscript = (JSONLexer lexer, ref T container, Subscript subscript) =>
        	{
        		U	value;

        		if (!reader.Convert (lexer, out value))
        			return false;

        		setter (ref container, subscript, value);

				return true;
        	};

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
        	if (typeof (T) == typeof (char))
        		this.linker = this.GetLinker<char> (JSONConverter.ToChar);
        	else if (typeof (T) == typeof (float))
        		this.linker = this.GetLinker<float> (JSONConverter.ToFloat4);
        	else if (typeof (T) == typeof (double))
        		this.linker = this.GetLinker<double> (JSONConverter.ToFloat8);
        	else if (typeof (T) == typeof (sbyte))
        		this.linker = this.GetLinker<sbyte> (JSONConverter.ToInt1s);
        	else if (typeof (T) == typeof (byte))
        		this.linker = this.GetLinker<byte> (JSONConverter.ToInt1u);
        	else if (typeof (T) == typeof (short))
        		this.linker = this.GetLinker<short> (JSONConverter.ToInt2s);
        	else if (typeof (T) == typeof (ushort))
        		this.linker = this.GetLinker<ushort> (JSONConverter.ToInt2u);
        	else if (typeof (T) == typeof (int))
        		this.linker = this.GetLinker<int> (JSONConverter.ToInt4s);
        	else if (typeof (T) == typeof (uint))
        		this.linker = this.GetLinker<uint> (JSONConverter.ToInt4u);
        	else if (typeof (T) == typeof (long))
        		this.linker = this.GetLinker<long> (JSONConverter.ToInt8s);
        	else if (typeof (T) == typeof (ulong))
        		this.linker = this.GetLinker<ulong> (JSONConverter.ToInt8u);
        	else if (typeof (T) == typeof (string))
        		this.linker = this.GetLinker<string> (JSONConverter.ToString);
        	else
        		throw new BindTypeException (typeof (T), string.Format (CultureInfo.InvariantCulture, "JSON model does not support mapping for type \"{0}\"", typeof (T).Name));
        }
        
        #endregion
        
        #region Methods / Protected

        protected bool	Convert (JSONLexer lexer, out T source)
        {
        	string		key;
        	int			offset;
        	FieldReader	reader;

        	if (this.linker != null)
        		return this.linker (lexer, out source);

        	source = this.constructor ();

            switch (lexer.Lexem)
            {
            	case JSONLexem.ArrayBegin:
        			for (offset = 0; true; ++offset)
        			{
        				if (!lexer.Next ())
        					return false;

        				if (this.fields.TryGetValue (offset.ToString (CultureInfo.InvariantCulture), out reader))
        				{
        					if (!reader (lexer, ref source))
        						return false;
        				}
        				else if (this.subscript != null)
        				{
        					if (!this.subscript (lexer, ref source, new Subscript (offset)))
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

        				if (this.fields.TryGetValue (key, out reader))
        				{
        					if (!reader (lexer, ref source))
        						return false;
        				}
        				else if (this.subscript != null)
        				{
        					if (!this.subscript (lexer, ref source, new Subscript (key)))
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

        private Linker	GetLinker<U> (LinkerConverter<U> converter)
        {
        	Label				assign;
        	ILGenerator			generator;
			DynamicMethod		method;
        	LocalBuilder		value;
        	LinkerWrapper<U>	wrapper;

			method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONLexer), converter.GetType (), typeof (T).MakeByRefType ()}, this.GetType ());
			generator = method.GetILGenerator ();

			assign = generator.DefineLabel ();
			value = generator.DeclareLocal (typeof (U));

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldloca_S, value);
			generator.Emit (OpCodes.Callvirt, converter.GetType ().GetMethod ("Invoke"));
			generator.Emit (OpCodes.Brtrue_S, assign);
			generator.Emit (OpCodes.Ldc_I4_0);
			generator.Emit (OpCodes.Ret);

			generator.MarkLabel (assign);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Ldloc, value);
			generator.Emit (OpCodes.Stind_Ref);
			generator.Emit (OpCodes.Ldc_I4_1);
			generator.Emit (OpCodes.Ret);

			wrapper = (LinkerWrapper<U>)method.CreateDelegate (typeof (LinkerWrapper<U>));

			return (JSONLexer lexer, out T target) => wrapper (lexer, converter, out target) && this.Ignore (lexer);
        }

        #endregion
        
        #region Types

        private delegate bool	Linker (JSONLexer lexer, out T value);

        private delegate bool	LinkerConverter<U> (JSONLexer lexer, out U value);
        
        private delegate bool	LinkerWrapper<U> (JSONLexer lexer, LinkerConverter<U> converter, out T value);

        private delegate bool	FieldReader (JSONLexer lexer, ref T container);

        private delegate bool	SubReader (JSONLexer lexer, ref T container, Subscript subscript);
        
        #endregion
    }
}
