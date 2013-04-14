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

		private Func<Stream, Encoding, JSONWriter>	generator;

		private Writer								selfWriter;

		private JSONSettings						settings;

        #endregion

        #region Constructors

        public	JSONEncoder (Dictionary<Type, object> converters, JSONSettings settings, Encoding encoding, Func<Stream, Encoding, JSONWriter> generator) :
        	base (converters)
		{
			this.encoding = encoding;
			this.fieldWriters = new Dictionary<string, Writer> ();
			this.generator = generator;
			this.selfWriter = null;
			this.settings = settings;
		}

		#endregion

    	#region Methods / Public

        public override bool	Encode (Stream stream, T instance)
        {
            using (JSONWriter writer = this.generator (stream, this.encoding))
            {
            	return this.Write (writer, instance);
            }
        }

		public override IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter)
		{
			return this.HasField (name, getter, this.BuildEncoder<U> ());
		}

		public override void	HasField<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.HasField (name, getter, (JSONEncoder<U>)encoder);
		}

		public override IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter)
		{
			return this.HasItems (getter, this.BuildEncoder<U> ());
		}

		public override void	HasItems<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.HasItems (getter, (JSONEncoder<U>)encoder);
		}

		public override IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter)
		{
			return this.HasPairs (getter, this.BuildEncoder<U> ());
		}

		public override void	HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is JSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a JSON encoder", "encoder");

			this.HasPairs (getter, (JSONEncoder<U>)encoder);
		}

        #endregion

		#region Methods / Protected

		protected override bool	TryLinkConvert (ConvertSchema<string>.EncoderConverter<T> converter)
    	{
			this.selfWriter = (writer, input) =>
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
        	Type	type;

        	type = typeof (T);

        	if (type.IsEnum)
        		this.selfWriter = this.BuildWriter<int> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (bool))
        		this.selfWriter = this.BuildWriter<bool> ((writer, value) => writer.WriteBoolean (value));
        	else if (type == typeof (char))
        		this.selfWriter = this.BuildWriter<char> ((writer, value) => writer.WriteString (new string (value, 1)));
        	else if (type == typeof (float))
        		this.selfWriter = this.BuildWriter<float> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (double))
        		this.selfWriter = this.BuildWriter<double> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (sbyte))
        		this.selfWriter = this.BuildWriter<sbyte> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (byte))
        		this.selfWriter = this.BuildWriter<byte> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (short))
        		this.selfWriter = this.BuildWriter<short> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (ushort))
        		this.selfWriter = this.BuildWriter<ushort> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (int))
        		this.selfWriter = this.BuildWriter<int> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (uint))
        		this.selfWriter = this.BuildWriter<uint> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (long))
        		this.selfWriter = this.BuildWriter<long> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (ulong))
        		this.selfWriter = this.BuildWriter<ulong> ((writer, value) => writer.WriteNumber (value));
        	else if (type == typeof (string))
        		this.selfWriter = this.BuildWriter<string> ((writer, value) => writer.WriteString (value));
        	else
        		return false;

			return true;
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

		private Writer	BuildWriter<U> (WriterInjector<U> injector)
		{
			ILGenerator			generator;
			DynamicMethod		method;
        	WriterWrapper<U>	wrapper;

        	method = new DynamicMethod (string.Empty, typeof (void), new Type[] {typeof (JSONWriter), typeof (WriterInjector<U>), typeof (T)}, typeof (JSONEncoder<T>).Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Call, Resolver.Method<WriterInjector<U>, JSONWriter, U> ((i, writer, value) => i.Invoke (writer, value)));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterWrapper<U>)method.CreateDelegate (typeof (WriterWrapper<U>));

			return (writer, value) =>
			{
				wrapper (writer, injector, value);

				return true;
			};
		}

		private JSONEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.fieldWriters[name] = (writer, container) => encoder.Write (writer, getter (container));

			return encoder;
		}

		private JSONEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.selfWriter = (writer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (U value in getter (container))
				{
					if (value == null && (this.settings & JSONSettings.OmitNullItems) == JSONSettings.OmitNullItems)
						continue;

					if (empty)
					{
						writer.WriteArrayBegin ();

						empty = false;
					}
					else
						writer.WriteComma ();

					if (!encoder.Write (writer, value))
						return false;
				}

				if (empty)
					writer.WriteArrayBegin ();

				writer.WriteArrayEnd ();

				return true;
			};

			return encoder;
		}

		private JSONEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.selfWriter = (writer, container) =>
			{
				bool	empty;

				empty = true;

				foreach (KeyValuePair<string, U> pair in getter (container))
				{
					if (pair.Value == null && (this.settings & JSONSettings.OmitNullItems) == JSONSettings.OmitNullItems)
						continue;

					if (empty)
					{
						writer.WriteObjectBegin ();

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
					writer.WriteObjectBegin ();

				writer.WriteObjectEnd ();

				return true;
			};

			return encoder;
		}

		private bool	Write (JSONWriter writer, T value)
		{
			bool	empty;

			if (value == null)
			{
				writer.WriteNull ();

				return true;
			}

			if (this.selfWriter != null)
				return this.selfWriter (writer, value);

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

				if (!field.Value (writer, value))
					return false;
			}

			if (empty)
				writer.WriteObjectBegin ();

			writer.WriteObjectEnd ();

			return true;
		}

		#endregion

        #region Types

        private delegate bool	Writer (JSONWriter writer, T value);

        private delegate void	WriterInjector<U> (JSONWriter writer, U value);

        private delegate void	WriterWrapper<U> (JSONWriter writer, WriterInjector<U> importer, T value);
        
        #endregion
	}
}
