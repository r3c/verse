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

		private Func<Stream, Encoding, JSONPrinter>	printer;

		private Writer								writer;

        #endregion

        #region Constructors

        public	JSONEncoder (Dictionary<Type, object> converters, Encoding encoding, Func<Stream, Encoding, JSONPrinter> printer) :
        	base (converters)
		{
			this.printer = printer;
			this.encoding = encoding;
			this.fieldWriters = new Dictionary<string, Writer> ();
			this.writer = null;
		}

		#endregion

    	#region Methods / Public

        public override bool	Encode (Stream stream, T instance)
        {
            using (JSONPrinter printer = this.printer (stream, this.encoding))
            {
            	return this.Write (printer, instance);
            }
        }

		public override IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

        	this.fieldWriters[name] = (printer, container) => encoder.Write (printer, getter (container));

        	return encoder;
		}

		public override IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

			this.writer = (printer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (U value in getter (container))
				{
					if (empty)
					{
						printer.PrintArrayBegin ();

						empty = false;
					}
					else
						printer.PrintComma ();

					if (!encoder.Write (printer, value))
						return false;
				}

				if (empty)
					printer.PrintArrayBegin ();

				printer.PrintArrayEnd ();

				return true;
			};

			return encoder;
		}

		public override IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

			this.writer = (printer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (KeyValuePair<string, U> pair in getter (container))
				{
					if (empty)
					{
						printer.PrintObjectBegin ();

						empty = false;
					}
					else
						printer.PrintComma ();

					printer.PrintString (pair.Key);
					printer.PrintColon ();

					if (!encoder.Write (printer, pair.Value))
						return false;
				}

				if (empty)
					printer.PrintObjectBegin ();

				printer.PrintObjectEnd ();

				return true;
			};

			return encoder;
		}

        #endregion

		#region Methods / Protected

		protected override bool	TryLinkConvert (ConvertSchema<string>.EncoderConverter<T> converter)
    	{
			this.writer = (printer, input) =>
			{
				string	value;

				if (!converter (input, out value))
					return false;

				if (value != null)
					printer.PrintString (value);
				else
					printer.PrintNull ();

				return true;
			};

			return true;
    	}

		protected override bool	TryLinkNative ()
		{
        	Type	type;

        	type = typeof (T);

        	if (type.IsEnum)
        		this.writer = this.BuildWriter<int> (JSONConverter.FromInt4s);
        	else if (type == typeof (bool))
        		this.writer = this.BuildWriter<bool> (JSONConverter.FromBoolean);
        	else if (type == typeof (char))
        		this.writer = this.BuildWriter<char> (JSONConverter.FromChar);
        	else if (type == typeof (float))
        		this.writer = this.BuildWriter<float> (JSONConverter.FromFloat4);
        	else if (type == typeof (double))
        		this.writer = this.BuildWriter<double> (JSONConverter.FromFloat8);
        	else if (type == typeof (sbyte))
        		this.writer = this.BuildWriter<sbyte> (JSONConverter.FromInt1s);
        	else if (type == typeof (byte))
        		this.writer = this.BuildWriter<byte> (JSONConverter.FromInt1u);
        	else if (type == typeof (short))
        		this.writer = this.BuildWriter<short> (JSONConverter.FromInt2s);
        	else if (type == typeof (ushort))
        		this.writer = this.BuildWriter<ushort> (JSONConverter.FromInt2u);
        	else if (type == typeof (int))
        		this.writer = this.BuildWriter<int> (JSONConverter.FromInt4s);
        	else if (type == typeof (uint))
        		this.writer = this.BuildWriter<uint> (JSONConverter.FromInt4u);
        	else if (type == typeof (long))
        		this.writer = this.BuildWriter<long> (JSONConverter.FromInt8s);
        	else if (type == typeof (ulong))
        		this.writer = this.BuildWriter<ulong> (JSONConverter.FromInt8u);
        	else if (type == typeof (string))
        		this.writer = this.BuildWriter<string> (JSONConverter.FromString);
        	else
        		return false;

			return true;
		}

		#endregion

		#region Methods / Private

        private JSONEncoder<U>	BuildEncoder<U> ()
        {
        	JSONEncoder<U>	encoder;

        	encoder = new JSONEncoder<U> (this.converters, this.encoding, this.printer);
        	encoder.OnStreamError += this.EventStreamError;
        	encoder.OnTypeError += this.EventTypeError;

        	return encoder;
        }

		private Writer	BuildWriter<U> (WriterInjector<U> injector)
		{
			ILGenerator			generator;
			DynamicMethod		method;
        	WriterWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (void), new Type[] {typeof (JSONPrinter), typeof (WriterInjector<U>), typeof (T)}, typeof (JSONEncoder<T>).Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Call, Resolver.Method<WriterInjector<U>, JSONPrinter, U> ((i, printer, value) => i.Invoke (printer, value)));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterWrapper<U>)method.CreateDelegate (typeof (WriterWrapper<U>));

			return (printer, value) =>
			{
				wrapper (printer, injector, value);

				return true;
			};
		}

		private bool	Write (JSONPrinter printer, T value)
		{
			bool	empty;

			if (value == null)
			{
				printer.PrintNull ();

				return true;
			}

			if (this.writer != null)
				return this.writer (printer, value);

			empty = true;

			foreach (KeyValuePair<string, Writer> field in this.fieldWriters)
			{
				if (empty)
				{
					printer.PrintObjectBegin ();

					empty = false;
				}
				else
					printer.PrintComma ();

				printer.PrintString (field.Key);
				printer.PrintColon ();

				if (!field.Value (printer, value))
					return false;
			}

			if (empty)
				printer.PrintObjectBegin ();

			printer.PrintObjectEnd ();

			return true;
		}

		#endregion

        #region Types

        private delegate bool	Writer (JSONPrinter printer, T value);

        private delegate void	WriterInjector<U> (JSONPrinter printer, U value);

        private delegate void	WriterWrapper<U> (JSONPrinter printer, WriterInjector<U> importer, T value);
        
        #endregion
	}
}
