using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.BSON
{
	class BSONEncoder<T> : ConvertEncoder<byte[], T>
	{
		#region Attributes

		private Encoding	encoding;

		#endregion

		#region Constructors

		public	BSONEncoder (Dictionary<Type, object> converters, Encoding encoding) :
			base (converters)
		{
			this.encoding = encoding;
		}

		#endregion
		
		#region Methods / Public

		public override bool	Encode (Stream stream, T instance)
		{
			throw new NotImplementedException ();
		}

		public override void	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is BSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a BSON encoder", "encoder");

			this.HasAttribute (name, getter, (BSONEncoder<U>)encoder);
		}

		public override void	HasElements<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is BSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a BSON encoder", "encoder");

			this.HasElements (getter, (BSONEncoder<U>)encoder);
		}

		public override void	HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder)
		{
			if (!(encoder is BSONEncoder<U>))
				throw new ArgumentException ("nested encoder must be a BSON encoder", "encoder");

			this.HasPairs (getter, (BSONEncoder<U>)encoder);
		}

		#endregion
		
		#region Methods / Protected

		protected override AbstractEncoder<U>	DefineAttribute<U> (string name, EncoderValueGetter<T, U> getter)
		{
			return this.HasAttribute (name, getter, this.BuildEncoder<U> ());
		}

		protected override AbstractEncoder<U>	DefineElements<U> (EncoderArrayGetter<T, U> getter)
		{
			return this.HasElements (getter, this.BuildEncoder<U> ());
		}

		protected override AbstractEncoder<U>	DefinePairs<U> (EncoderMapGetter<T, U> getter)
		{
			return this.HasPairs (getter, this.BuildEncoder<U> ());
		}

		protected override bool	TryLinkNative ()
		{
			throw new NotImplementedException ();
		}
		
		protected override bool	TryLinkConvert (ConvertSchema<byte[]>.EncoderConverter<T> converter)
		{
			throw new NotImplementedException ();
		}

		#endregion

		#region Methods / Private

		private BSONEncoder<U>	BuildEncoder<U> ()
		{
			BSONEncoder<U>	encoder;

			encoder = new BSONEncoder<U> (this.converters, this.encoding);
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
		}
/*
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
			generator.Emit (OpCodes.Call, Resolver<WriterInjector<U>>.Method<JSONWriter, U> ((i, w, v) => i.Invoke (w, v)));
			generator.Emit (OpCodes.Ret);

			wrapper = (WriterWrapper<U>)method.CreateDelegate (typeof (WriterWrapper<U>));

			return (writer, value) =>
			{
				wrapper (writer, injector, value);

				return true;
			};
		}
*/
		private BSONEncoder<U>	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, BSONEncoder<U> encoder)
		{
			throw new NotImplementedException ();
/*			this.fieldWriters[name] = (writer, container) => encoder.Write (writer, getter (container));

			return encoder;*/
		}

		private BSONEncoder<U>	HasElements<U> (EncoderArrayGetter<T, U> getter, BSONEncoder<U> encoder)
		{
			throw new NotImplementedException ();
/*			this.selfWriter = (writer, container) =>
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

			return encoder;*/
		}

		private BSONEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter, BSONEncoder<U> encoder)
		{
			throw new NotImplementedException ();
/*			this.selfWriter = (writer, container) =>
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

			return encoder;*/
		}

		private bool	Write (object writer, T value)
		{
			throw new NotImplementedException ();
/*			bool	empty;

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
					writer.WriteObjectBegin (false);

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
				writer.WriteObjectBegin (true);

			writer.WriteObjectEnd (empty);

			return true;*/
		}

		#endregion

		#region Types

		private delegate bool	Writer (object writer, T value);
		
		#endregion
	}
}
