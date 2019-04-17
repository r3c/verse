using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.EncoderDescriptors.Tree;

namespace Verse.Schemas.RawProtobuf
{
	internal class RawProtobufEncoderConverter : IEncoderConverter<RawProtobufValue>
	{
		private readonly unsafe Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{
				typeof(bool),
				new Converter<bool, RawProtobufValue>(v => new RawProtobufValue(v ? 1 : 0, RawProtobufStorage.Variant))
			},
			{
				typeof(char),
				new Converter<char, RawProtobufValue>(v =>
					new RawProtobufValue(new string(v, 1), RawProtobufStorage.Variant))
			},
			{
				typeof(decimal), new Converter<decimal, RawProtobufValue>(v =>
				{
					var number = (double) v;

					return new RawProtobufValue(*(long*) &number, RawProtobufStorage.Fixed64);
				})
			},
			{
				typeof(float),
				new Converter<float, RawProtobufValue>(
					v => new RawProtobufValue(*(int*) &v, RawProtobufStorage.Fixed32))
			},
			{
				typeof(double),
				new Converter<double, RawProtobufValue>(v =>
					new RawProtobufValue(*(long*) &v, RawProtobufStorage.Fixed64))
			},
			{
				typeof(sbyte),
				new Converter<sbyte, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(byte),
				new Converter<byte, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(short),
				new Converter<short, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(ushort),
				new Converter<ushort, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(int),
				new Converter<int, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(uint),
				new Converter<uint, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(long),
				new Converter<long, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.Variant))
			},
			{
				typeof(ulong),
				new Converter<ulong, RawProtobufValue>(
					v => new RawProtobufValue(*(long*) v, RawProtobufStorage.Variant))
			},
			{
				typeof(string),
				new Converter<string, RawProtobufValue>(v => new RawProtobufValue(v, RawProtobufStorage.String))
			},
			{typeof(RawProtobufValue), new Converter<RawProtobufValue, RawProtobufValue>(v => v)}
		};

		public Converter<TFrom, RawProtobufValue> Get<TFrom>()
		{
			if (!this.converters.TryGetValue(typeof(TFrom), out var box))
			{
				throw new InvalidCastException(
					string.Format(
						CultureInfo.InvariantCulture,
						"no available converter from type '{0}', protobuf value",
						typeof(TFrom)));
			}

			return (Converter<TFrom, RawProtobufValue>) box;
		}

		public void Set<TFrom>(Converter<TFrom, RawProtobufValue> converter)
		{
			this.converters[typeof(TFrom)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}
	}
}
