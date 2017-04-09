using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.EncoderDescriptors.Abstract;

namespace Verse.Schemas.Protobuf
{
    class EncoderConverter : IEncoderConverter<Value>
    {
        #region Attributes

        private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
        {
            { typeof (bool), new Converter<bool, Value>(v => new Value(v ? 1 : 0)) },
            { typeof (char), new Converter<char, Value>((v) => new Value(new string(v, 1))) },
            { typeof (decimal), new Converter<decimal, Value>(v => new Value((double)v)) },
            { typeof (float), new Converter<float, Value>(v => new Value(v)) },
            { typeof (double), new Converter<double, Value>(v => new Value(v)) },
            { typeof (sbyte), new Converter<sbyte, Value>((v) => new Value((long)v)) },
            { typeof (byte), new Converter<byte, Value>((v) => new Value((long)v)) },
            { typeof (short), new Converter<short, Value>((v) => new Value((long)v)) },
            { typeof (ushort), new Converter<ushort, Value>((v) => new Value((long)v)) },
            { typeof (int), new Converter<int, Value>((v) => new Value((long)v)) },
            { typeof (uint), new Converter<uint, Value>((v) => new Value((long)v)) },
            { typeof (long), new Converter<long, Value>((v) => new Value(v)) },
            { typeof (ulong), new Converter<ulong, Value>((v) => new Value((long)v)) },
            { typeof (string), new Converter<string, Value>((v) => new Value(v)) },
            { typeof (Value), new Converter<Value, Value>((v) => v) }
        };

        #endregion

        #region Methods

        public Converter<TFrom, Value> Get<TFrom>()
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

            return (Converter<TFrom, Value>)box;
        }

        public void Set<TFrom>(Converter<TFrom, Value> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            this.converters[typeof (TFrom)] = converter;
        }

        #endregion
    }
}
