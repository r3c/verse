using System;
using System.Collections.Generic;
using System.Globalization;
using Verse.ParserDescriptors.Abstract;

namespace Verse.Schemas.JSON
{
    internal class ValueDecoder : IDecoder<Value>
    {
        #region Attributes

        private readonly Dictionary<Type, object> converters = new Dictionary<Type, object>
        {
            { typeof (bool), new Converter<Value, bool>(ValueDecoder.ToBoolean) },
            { typeof (char), new Converter<Value, char>(ValueDecoder.ToCharacter) },
            { typeof (decimal), new Converter<Value, decimal>(ValueDecoder.ToDecimal) },
            { typeof (float), new Converter<Value, float>(ValueDecoder.ToFloat32) },
            { typeof (double), new Converter<Value, double>(ValueDecoder.ToFloat64) },
            { typeof (sbyte), new Converter<Value, sbyte>(ValueDecoder.ToInteger8s) },
            { typeof (byte), new Converter<Value, byte>(ValueDecoder.ToInteger8u) },
            { typeof (short), new Converter<Value, short>(ValueDecoder.ToInteger16s) },
            { typeof (ushort), new Converter<Value, ushort>(ValueDecoder.ToInteger16u) },
            { typeof (int), new Converter<Value, int>(ValueDecoder.ToInteger32s) },
            { typeof (uint), new Converter<Value, uint>(ValueDecoder.ToInteger32u) },
            { typeof (long), new Converter<Value, long>(ValueDecoder.ToInteger64s) },
            { typeof (ulong), new Converter<Value, ulong>(ValueDecoder.ToInteger64u) },
            { typeof (string), new Converter<Value, string>(ValueDecoder.ToString) },
            { typeof (Value), new Converter<Value, Value>((v) => v) }
        };

        #endregion

        #region Methods / Public

        public Converter<Value, TValue> Get<TValue>()
        {
            object box;

            if (!this.converters.TryGetValue(typeof (TValue), out box))
                throw new InvalidCastException(string.Format(CultureInfo.InvariantCulture, "no available converter from JSON value to type '{0}'", typeof (TValue)));

            return (Converter<Value, TValue>)box;
        }

        public void Set<TValue>(Converter<Value, TValue> converter)
        {
            if (converter == null)
                throw new ArgumentNullException("converter");

            this.converters[typeof (TValue)] = converter;
        }

        #endregion

        #region Methods / Private

        private static bool ToBoolean(Value value)
        {
            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean;

                case ContentType.Number:
                    return value.Number != 0;

                case ContentType.String:
                    return !string.IsNullOrEmpty(value.String);

                default:
                    return false;
            }
        }

        private static char ToCharacter(Value value)
        {
            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? '1' : '\0';

                case ContentType.Number:
                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    return value.Number != 0 ? '1' : '\0';

                case ContentType.String:
                    return value.String.Length > 0 ? value.String[0] : '\0';

                default:
                    return '\0';
            }
        }

        private static decimal ToDecimal(Value value)
        {
            decimal number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1 : 0;

                case ContentType.Number:
                    return value.Number;

                case ContentType.String:
                    if (decimal.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static float ToFloat32(Value value)
        {
            float number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1 : 0;

                case ContentType.Number:
                    return (float)value.Number;

                case ContentType.String:
                    if (float.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static double ToFloat64(Value value)
        {
            double number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1 : 0;

                case ContentType.Number:
                    return (double) value.Number;

                case ContentType.String:
                    if (double.TryParse(value.String, NumberStyles.Float, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static sbyte ToInteger8s(Value value)
        {
            sbyte number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? (sbyte)1 : (sbyte)0;

                case ContentType.Number:
                    return (sbyte)value.Number;

                case ContentType.String:
                    if (sbyte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static byte ToInteger8u(Value value)
        {
            byte number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? (byte)1 : (byte)0;

                case ContentType.Number:
                    return (byte)value.Number;

                case ContentType.String:
                    if (byte.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static short ToInteger16s(Value value)
        {
            short number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? (short)1 : (short)0;

                case ContentType.Number:
                    return (short)value.Number;

                case ContentType.String:
                    if (short.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static ushort ToInteger16u(Value value)
        {
            ushort number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? (ushort)1 : (ushort)0;

                case ContentType.Number:
                    return (ushort)value.Number;

                case ContentType.String:
                    if (ushort.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static int ToInteger32s(Value value)
        {
            int number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1 : 0;

                case ContentType.Number:
                    return (int)value.Number;

                case ContentType.String:
                    if (int.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static uint ToInteger32u(Value value)
        {
            uint number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1u : 0;

                case ContentType.Number:
                    return (uint)value.Number;

                case ContentType.String:
                    if (uint.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static long ToInteger64s(Value value)
        {
            long number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1 : 0;

                case ContentType.Number:
                    return (long)value.Number;

                case ContentType.String:
                    if (long.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static ulong ToInteger64u(Value value)
        {
            ulong number;

            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? 1u : 0;

                case ContentType.Number:
                    return (ulong)value.Number;

                case ContentType.String:
                    if (ulong.TryParse(value.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out number))
                        return number;

                    return 0;

                default:
                    return 0;
            }
        }

        private static string ToString(Value value)
        {
            switch (value.Type)
            {
                case ContentType.Boolean:
                    return value.Boolean ? "1" : string.Empty;

                case ContentType.Number:
                    return value.Number.ToString(CultureInfo.InvariantCulture);

                case ContentType.String:
                    return value.String;

                default:
                    return string.Empty;
            }
        }

        #endregion
    }
}