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

		private Writer								selfWriter;

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
			this.selfWriter = null;
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
				type = typeof (int);

			if (type == typeof (bool))
				this.selfWriter = this.WrapWriter<bool> ((writer, value) => writer.WriteBoolean (value));
			else if (type == typeof (char))
				this.selfWriter = this.WrapWriter<char> ((writer, value) => writer.WriteString (new string (value, 1)));
			else if (type == typeof (float))
				this.selfWriter = this.WrapWriter<float> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (double))
				this.selfWriter = this.WrapWriter<double> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (sbyte))
				this.selfWriter = this.WrapWriter<sbyte> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (byte))
				this.selfWriter = this.WrapWriter<byte> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (short))
				this.selfWriter = this.WrapWriter<short> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (ushort))
				this.selfWriter = this.WrapWriter<ushort> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (int))
				this.selfWriter = this.WrapWriter<int> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (uint))
				this.selfWriter = this.WrapWriter<uint> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (long))
				this.selfWriter = this.WrapWriter<long> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (ulong))
				this.selfWriter = this.WrapWriter<ulong> ((writer, value) => writer.WriteNumber (value));
			else if (type == typeof (string))
				this.selfWriter = this.WrapWriter<string> ((writer, value) => writer.WriteString (value));
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

		private JSONEncoder<U>	DefineAttribute<U> (string name, EncoderValueGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.fieldWriters[name] = (writer, container) => encoder.Write (writer, getter (container));

			return encoder;
		}

		private JSONEncoder<U>	DefineElements<U> (EncoderArrayGetter<T, U> getter, JSONEncoder<U> encoder)
		{
			this.selfWriter = (writer, container) =>
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
			this.selfWriter = (writer, container) =>
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

		private Writer	WrapWriter<U> (WriterInjector<U> injector)
		{
			ILGenerator			generator;
			DynamicMethod		method;
			WriterWrapper<U>	wrapper;

			method = new DynamicMethod (string.Empty, typeof (void), new Type[] {typeof (JSONPrinter), typeof (WriterInjector<U>), typeof (T)}, typeof (JSONEncoder<T>).Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_2);
			generator.Emit (OpCodes.Call, Resolver<WriterInjector<U>>.Method<JSONPrinter, U> ((i, w, v) => i.Invoke (w, v)));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterWrapper<U>)method.CreateDelegate (typeof (WriterWrapper<U>));

			return (writer, value) =>
			{
				wrapper (writer, injector, value);

				return true;
			};
		}

		private bool	Write (JSONPrinter printer, T value)
		{
			bool	empty;

			if (value == null)
			{
				printer.WriteNull ();

				return true;
			}

			if (this.selfWriter != null)
				return this.selfWriter (printer, value);

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

		private delegate bool	Writer (JSONPrinter printer, T value);

		private delegate void	WriterInjector<U> (JSONPrinter printer, U value);

		private delegate void	WriterWrapper<U> (JSONPrinter printer, WriterInjector<U> injector, T value);
		
		#endregion
	}
}
