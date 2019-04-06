using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.EncoderDescriptors.Base;

namespace Verse.Schemas.JSON
{
	class EncoderConverter : IEncoderConverter<JSONValue>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (bool), new Converter<bool, JSONValue>(JSONValue.FromBoolean) },
			{ typeof (char), new Converter<char, JSONValue>(v => JSONValue.FromString(new string(v, 1))) },
			{ typeof (decimal), new Converter<decimal, JSONValue>(v => JSONValue.FromNumber((double)v)) },
			{ typeof (float), new Converter<float, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (double), new Converter<double, JSONValue>(JSONValue.FromNumber) },
			{ typeof (sbyte), new Converter<sbyte, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (byte), new Converter<byte, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (short), new Converter<short, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (ushort), new Converter<ushort, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (int), new Converter<int, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (uint), new Converter<uint, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (long), new Converter<long, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (ulong), new Converter<ulong, JSONValue>(v => JSONValue.FromNumber(v)) },
			{ typeof (string), new Converter<string, JSONValue>(JSONValue.FromString) },
			{ typeof (JSONValue), new Converter<JSONValue, JSONValue>(v => v) }
		};

		public Converter<TFrom, JSONValue> Get<TFrom>()
		{
		    if (!this.converters.TryGetValue(typeof (TFrom), out var box))
				throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "cannot convert '{0}' into JSON value, please register a converter using schema's SetEncoderConverter method", typeof (TFrom)));

			return (Converter<TFrom, JSONValue>)box;
		}

		public void Set<TFrom>(Converter<TFrom, JSONValue> converter)
		{
            this.converters[typeof (TFrom)] = converter ?? throw new ArgumentNullException(nameof(converter));
		}
	}
}