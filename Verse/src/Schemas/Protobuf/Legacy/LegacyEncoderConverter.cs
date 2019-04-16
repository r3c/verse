using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.Protobuf.Legacy
{
	internal class LegacyEncoderConverter : IEncoderConverter<ProtobufValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<bool, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (char), new Converter<char, ProtobufValue>(v => new ProtobufValue(new string(v, 1))) },
			{ typeof (decimal), new Converter<decimal, ProtobufValue>(v => new ProtobufValue((double)v)) },
			{ typeof (float), new Converter<float, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (double), new Converter<double, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (sbyte), new Converter<sbyte, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (byte), new Converter<byte, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (short), new Converter<short, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (ushort), new Converter<ushort, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (int), new Converter<int, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (uint), new Converter<uint, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (long), new Converter<long, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (ulong), new Converter<ulong, ProtobufValue>(v => new ProtobufValue((long)v)) },
			{ typeof (string), new Converter<string, ProtobufValue>(v => new ProtobufValue(v)) },
			{ typeof (ProtobufValue), new Converter<ProtobufValue, ProtobufValue>(v => v) }
		};

		public Converter<TFrom, ProtobufValue> Get<TFrom>()
		{
			if (!this.converters.TryGetValue(typeof (TFrom), out var box))
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
		    this.converters[typeof (TFrom)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}
	}
}
