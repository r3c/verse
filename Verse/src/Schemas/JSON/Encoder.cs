using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.BuilderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Encoder : IEncoder<Value>
	{
		#region Attributes

		private readonly Dictionary<Type, object>	converters = new Dictionary<Type, object>
		{
			{typeof (bool),		new Converter<bool, Value> (Encoder.FromBoolean)},
			{typeof (char),		new Converter<char, Value> (Encoder.FromCharacter)},
			{typeof (decimal),	new Converter<decimal, Value> (Encoder.FromDecimal)},
			{typeof (float),	new Converter<float, Value> (Encoder.FromFloat32)},
			{typeof (double),	new Converter<double, Value> (Encoder.FromFloat64)},
			{typeof (sbyte),	new Converter<sbyte, Value> (Encoder.FromInteger8s)},
			{typeof (byte),		new Converter<byte, Value> (Encoder.FromInteger8u)},
			{typeof (short),	new Converter<short, Value> (Encoder.FromInteger16s)},
			{typeof (ushort),	new Converter<ushort, Value> (Encoder.FromInteger16u)},
			{typeof (int),		new Converter<int, Value> (Encoder.FromInteger32s)},
			{typeof (uint),		new Converter<uint, Value> (Encoder.FromInteger32u)},
			{typeof (long),		new Converter<long, Value> (Encoder.FromInteger64s)},
			{typeof (ulong),	new Converter<ulong, Value> (Encoder.FromInteger64u)},
			{typeof (string),	new Converter<string, Value> (Encoder.FromString)},
			{typeof (Value),	new Converter<Value, Value> ((v) => v)}
		};

		#endregion

		#region Methods / Public

		public Converter<T, Value> Get<T> ()
		{
			object	box;

			if (!this.converters.TryGetValue (typeof (T), out box))
				throw new InvalidCastException (string.Format (CultureInfo.InvariantCulture, "no available converter from type '{0}', JSON value", typeof (T)));

			return (Converter<T, Value>)box;
		}

		public void Set<T> (Converter<T, Value> converter)
		{
			if (converter == null)
				throw new ArgumentNullException ("converter");

			this.converters[typeof (T)] = converter;
		}

		#endregion

		#region Methods / Private

		private static Value FromBoolean (bool value)
		{
			return new Value {Boolean = value, Type = Content.Boolean};
		}

		private static Value FromCharacter (char value)
		{
			return new Value {String = new string (value, 1), Type = Content.String};
		}

		private static Value FromDecimal (decimal value)
		{
			return new Value {Number = (double)value, Type = Content.Number};
		}

		private static Value FromFloat32 (float value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromFloat64 (double value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger8s (sbyte value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger8u (byte value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger16s (short value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger16u (ushort value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger32s (int value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger32u (uint value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger64s (long value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromInteger64u (ulong value)
		{
			return new Value {Number = value, Type = Content.Number};
		}

		private static Value FromString (string value)
		{
			return new Value {String = value, Type = Content.String};
		}

		#endregion
	}
}
