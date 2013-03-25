using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

using Verse.Exceptions;

namespace Verse.Models.JSON
{
	class JSONEncoder<T> : AbstractEncoder<T>
	{
		#region Attributes
		
		private Dictionary<Type, object>	converters;

        private Encoding					encoding;

        private Dictionary<string, Writer>	fieldWriters;

        private IFormatter					formatter;

		private Writer						writer;

        #endregion

        #region Constructors

        public	JSONEncoder (Encoding encoding, Dictionary<Type, object> converters, IFormatter formatter)
		{
			this.converters = converters;
			this.encoding = encoding;
			this.fieldWriters = new Dictionary<string, Writer> ();
			this.formatter = formatter;
			this.writer = null;
		}

		#endregion

    	#region Methods / Public

		public override void	Bind ()
		{
        	object	converter;
        	Type	type;

        	type = typeof (T);

        	if (this.converters.TryGetValue (type, out converter))
        		this.writer = this.BuildWriter (converter);
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
        		throw new BindTypeException (type, "no converter for this type has been defined");
		}

        public override bool	Encode (Stream stream, T instance)
        {
            using (JSONWriter writer = new JSONWriter (stream, this.encoding))
            {
            	return this.Write (writer, instance);
            }
        }

		public override IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

        	this.fieldWriters[name] = (writer, container) => encoder.Write (writer, getter (container));

        	return encoder;
		}

		public override IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

			this.writer = (writer, container) =>
			{
				bool			empty;
				IEnumerable<U>	items;

				empty = true;
				items = getter (container);

				foreach (U value in items)
				{
					if (empty)
					{
						writer.WriteArrayBegin ();

						empty = false;
					}
					else
						writer.WriteComma ();

					encoder.Write (writer, value);
				}

				if (!empty/* || force_empty_array*/)
				{
					if (empty)
						writer.WriteArrayBegin ();

					writer.WriteArrayEnd ();
				}
				else
					writer.WriteNull ();

				return true;
			};

			return encoder;
		}

		public override IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter)
		{
        	JSONEncoder<U>	encoder;

        	encoder = this.BuildEncoder<U> ();

			this.writer = (writer, container) =>
			{
				bool									empty;
				IEnumerable<KeyValuePair<string, U>>	pairs;

				empty = true;
				pairs = getter (container);

				foreach (KeyValuePair<string, U> pair in pairs)
				{
					if (empty)
					{
						writer.WriteObjectBegin ();

						empty = false;
					}
					else
						writer.WriteComma ();

					writer.WriteString (pair.Key);
					writer.WriteColon ();

					encoder.Write (writer, pair.Value);
				}

				if (!empty/* || force_empty_object*/)
				{
					if (empty)
						writer.WriteObjectBegin ();

					writer.WriteObjectEnd ();
				}
				else
					writer.WriteNull ();

				return true;
			};

			return encoder;
		}

        #endregion

		#region Methods / Private

        private JSONEncoder<U>	BuildEncoder<U> ()
        {
        	JSONEncoder<U>	encoder;

        	encoder = new JSONEncoder<U> (this.encoding, this.converters, this.formatter);
        	encoder.OnStreamError += this.EventStreamError;
        	encoder.OnTypeError += this.EventTypeError;

        	return encoder;
        }

		private Writer	BuildWriter (object converter)
		{
			ILGenerator				generator;
			DynamicMethod			method;
			WriterConverterWrapper	wrapper;

			method = new DynamicMethod (string.Empty, typeof (string), new Type[] {typeof (object), typeof (T)}, this.GetType ());

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Callvirt, typeof (StringSchema.EncoderConverter<>).MakeGenericType (typeof (T)).GetMethod ("Invoke"));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterConverterWrapper)method.CreateDelegate (typeof (WriterConverterWrapper));

			return (writer, value) => writer.WriteString (wrapper (converter, value));
		}

		private Writer	BuildWriter<U> (WriterInjector<U> injector)
		{
			ILGenerator					generator;
			DynamicMethod				method;
        	WriterInjectorWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (bool), new Type[] {typeof (JSONWriter), typeof (WriterInjector<U>), typeof (T)}, this.GetType ());

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Callvirt, injector.GetType ().GetMethod ("Invoke"));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterInjectorWrapper<U>)method.CreateDelegate (typeof (WriterInjectorWrapper<U>));

			return (writer, value) => wrapper (writer, injector, value);
		}

		private bool	Write (JSONWriter writer, T value)
		{
			bool	empty;

			if (this.writer != null)
				return this.writer (writer, value);

			empty = true;

			foreach (KeyValuePair<string, Writer> field in this.fieldWriters)
			{
				if (empty)
				{
					writer.WriteObjectBegin ();

					empty = false;
				}
				else
					writer.WriteComma ();

				writer.WriteString (field.Key);
				writer.WriteColon ();

				field.Value (writer, value);
			}

			if (!empty/* || force_empty_object*/)
			{
				if (empty)
					writer.WriteObjectBegin ();

				writer.WriteObjectEnd ();
			}
			else
				writer.WriteNull ();

			return true;
		}

		#endregion

        #region Types

        private delegate bool	Writer (JSONWriter writer, T value);

		private delegate string	WriterConverterWrapper (object converter, T value);

        private delegate bool	WriterInjector<U> (JSONWriter writer, U value);

        private delegate bool	WriterInjectorWrapper<U> (JSONWriter writer, WriterInjector<U> importer, T value);
        
        #endregion
	}
}
