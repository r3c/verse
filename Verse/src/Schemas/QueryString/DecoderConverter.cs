using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.DecoderDescriptors.Abstract;

namespace Verse.Schemas.QueryString
{
	internal class DecoderConverter : IDecoderConverter<string>
	{
		private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
		{
			{ typeof (string), new Converter<string, string> (v => v) },
			{ typeof (bool), new Converter<string, bool>(bool.Parse) },
			{ typeof (char), new Converter<string, char>(char.Parse) },
			{ typeof (decimal), new Converter<string, decimal>(v => decimal.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (float), new Converter<string, float>((v) => float.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (double), new Converter<string, double>(v => double.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (sbyte), new Converter<string, sbyte>(v => sbyte.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (byte), new Converter<string, byte>(v => byte.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (short), new Converter<string, short>(v => short.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (ushort), new Converter<string, ushort>(v => ushort.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (int), new Converter<string, int>(v => int.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (uint), new Converter<string, uint>(v => uint.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (long), new Converter<string, long>(v => long.Parse(v, CultureInfo.InvariantCulture)) },
			{ typeof (ulong), new Converter<string, ulong>(v => ulong.Parse(v, CultureInfo.InvariantCulture)) }
		};

		public Converter<string, T> Get<T>()
		{
			return (Converter<string, T>)this.converters[typeof(T)];
		}

		public void Set<T>(Converter<string, T> converter)
		{
			this.converters[typeof(T)] = converter;
		}
	}
}
