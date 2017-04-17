using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.EncoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf
{
	class EncoderConverter : IEncoderConverter<ProtobufValue>
	{
		#region Attributes

		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<bool, ProtobufValue>(v => new ProtobufValue(v ? 1 : 0)) },
			{ typeof (char), new Converter<char, ProtobufValue>((v) => new ProtobufValue(new string(v, 1))) },
			{ typeof (decimal), new Converter<decimal, ProtobufValue>(v => new ProtobufValue((double)v)) },
			{ typeof (float), new Converter<float, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (double), new Converter<double, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (sbyte), new Converter<sbyte, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (byte), new Converter<byte, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (short), new Converter<short, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (ushort), new Converter<ushort, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (int), new Converter<int, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (uint), new Converter<uint, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (long), new Converter<long, ProtobufValue>((v) => new ProtobufValue(v)) },
			{ typeof (ulong), new Converter<ulong, ProtobufValue>((v) => new ProtobufValue((long)v)) },
			{ typeof (string), new Converter<string, ProtobufValue>((v) => new ProtobufValue(v)) },
			{ typeof (ProtobufValue), new Converter<ProtobufValue, ProtobufValue>((v) => v) }
		};

		#endregion

		#region Methods

		public Converter<TFrom, ProtobufValue> Get<TFrom>()
		{
			object box;

			if (!this.converters.TryGetValue(typeof (TFrom), out box))
			{
				throw new InvalidCastException(
					string.Format(
						CultureInfo.InvariantCulture,
						"no available converter from type '{0}', protobuf value",
						typeof (TFrom)));
			}

			return (Converter<TFrom, ProtobufValue>)box;
		}

		public void Set<TFrom>(Converter<TFrom, ProtobufValue> converter)
		{
			if (converter == null)
				throw new ArgumentNullException("converter");

			this.converters[typeof (TFrom)] = converter;
		}

		#endregion
	}
}
