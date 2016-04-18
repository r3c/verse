using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.PrinterDescriptors.Abstract;

namespace Verse.Schemas.JSON
{
    internal class ValueEncoder : IEncoder<Value>
    {
        #region Attributes

        private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
        {
            { typeof (bool), new Converter<bool, Value>(Value.FromBoolean) },
            { typeof (char), new Converter<char, Value>((v) => Value.FromString(new string(v, 1))) },
            { typeof (decimal), new Converter<decimal, Value>(Value.FromNumber) },
            { typeof (float), new Converter<float, Value>(v => Value.FromNumber((decimal)v)) },
            { typeof (double), new Converter<double, Value>(v => Value.FromNumber((decimal)v)) },
            { typeof (sbyte), new Converter<sbyte, Value>((v) => Value.FromNumber(v)) },
            { typeof (byte), new Converter<byte, Value>((v) => Value.FromNumber(v)) },
            { typeof (short), new Converter<short, Value>((v) => Value.FromNumber(v)) },
            { typeof (ushort), new Converter<ushort, Value>((v) => Value.FromNumber(v)) },
            { typeof (int), new Converter<int, Value>((v) => Value.FromNumber(v)) },
            { typeof (uint), new Converter<uint, Value>((v) => Value.FromNumber(v)) },
            { typeof (long), new Converter<long, Value>((v) => Value.FromNumber(v)) },
            { typeof (ulong), new Converter<ulong, Value>((v) => Value.FromNumber(v)) },
            { typeof (string), new Converter<string, Value>(Value.FromString) },
            { typeof (Value), new Converter<Value, Value>((v) => v) }
        };

        #endregion

        #region Methods

        public Converter<TValue, Value> Get<TValue>()
        {
            object box;

            if (!this.converters.TryGetValue(typeof (TValue), out box))
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "no available converter from type '{0}', JSON value", typeof (TValue)));

            return (Converter<TValue, Value>)box;
        }

        public void Set<TValue>(Converter<TValue, Value> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            this.converters[typeof (TValue)] = converter;
        }

        #endregion
    }
}