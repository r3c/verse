using System;
using System.Globalization;

namespace Verse.Schemas.Protobuf;

internal class ProtobufDecoderAdapter : IDecoderAdapter<ProtobufValue>
{
    public Setter<bool, ProtobufValue> Boolean => (ref bool target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean;

                break;

            case ProtobufType.Float32:
                target = Math.Abs(source.Float32) >= float.Epsilon;

                break;

            case ProtobufType.Float64:
                target = Math.Abs(source.Float64) >= float.Epsilon;

                break;

            case ProtobufType.Signed:
                target = source.Signed != 0;

                break;

            case ProtobufType.String:
                target = !string.IsNullOrEmpty(source.String);

                break;

            case ProtobufType.Unsigned:
                target = source.Unsigned != 0;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<char, ProtobufValue> Character => (ref char target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? '1' : '0';

                break;

            case ProtobufType.Float32:
                target = (char) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (char) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (char) source.Signed;

                break;

            case ProtobufType.String:
                if (source.String.Length > 0)
                {
                    target = source.String[0];

                    break;
                }

                target = default;

                break;

            case ProtobufType.Unsigned:
                target = (char) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<decimal, ProtobufValue> Decimal => (ref decimal target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case ProtobufType.Float32:
                target = (decimal) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (decimal) source.Float64;

                break;

            case ProtobufType.Signed:
                target = source.Signed;

                break;

            case ProtobufType.String:
                decimal.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (char) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<float, ProtobufValue> Float32 => (ref float target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case ProtobufType.Float32:
                target = source.Float32;

                break;

            case ProtobufType.Float64:
                target = (float) source.Float64;

                break;

            case ProtobufType.Signed:
                target = source.Signed;

                break;

            case ProtobufType.String:
                float.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<double, ProtobufValue> Float64 => (ref double target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case ProtobufType.Float32:
                target = source.Float32;

                break;

            case ProtobufType.Float64:
                target = source.Float64;

                break;

            case ProtobufType.Signed:
                target = source.Signed;

                break;

            case ProtobufType.String:
                double.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<sbyte, ProtobufValue> Integer8S => (ref sbyte target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? (sbyte) 1 : (sbyte) 0;

                break;

            case ProtobufType.Float32:
                target = (sbyte) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (sbyte) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (sbyte) source.Signed;

                break;

            case ProtobufType.String:
                sbyte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (sbyte) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<byte, ProtobufValue> Integer8U => (ref byte target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? (byte) 1 : (byte) 0;

                break;

            case ProtobufType.Float32:
                target = (byte) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (byte) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (byte) source.Signed;

                break;

            case ProtobufType.String:
                byte.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (byte) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<short, ProtobufValue> Integer16S => (ref short target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? (short) 1 : (short) 0;

                break;

            case ProtobufType.Float32:
                target = (short) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (short) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (short) source.Signed;

                break;

            case ProtobufType.String:
                short.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (short) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ushort, ProtobufValue> Integer16U => (ref ushort target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? (ushort) 1 : (ushort) 0;

                break;

            case ProtobufType.Float32:
                target = (ushort) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (ushort) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (ushort) source.Signed;

                break;

            case ProtobufType.String:
                ushort.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (ushort) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<int, ProtobufValue> Integer32S => (ref int target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case ProtobufType.Float32:
                target = (int) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (int) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (int) source.Signed;

                break;

            case ProtobufType.String:
                int.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (int) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<uint, ProtobufValue> Integer32U => (ref uint target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1u : 0u;

                break;

            case ProtobufType.Float32:
                target = (uint) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (uint) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (uint) source.Signed;

                break;

            case ProtobufType.String:
                uint.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (uint) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<long, ProtobufValue> Integer64S => (ref long target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1 : 0;

                break;

            case ProtobufType.Float32:
                target = (long) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (long) source.Float64;

                break;

            case ProtobufType.Signed:
                target = source.Signed;

                break;

            case ProtobufType.String:
                long.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = (long) source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<ulong, ProtobufValue> Integer64U => (ref ulong target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? 1u : 0u;

                break;

            case ProtobufType.Float32:
                target = (ulong) source.Float32;

                break;

            case ProtobufType.Float64:
                target = (ulong) source.Float64;

                break;

            case ProtobufType.Signed:
                target = (ulong) source.Signed;

                break;

            case ProtobufType.String:
                ulong.TryParse(source.String, NumberStyles.Integer, CultureInfo.InvariantCulture, out target);

                break;

            case ProtobufType.Unsigned:
                target = source.Unsigned;

                break;

            default:
                target = default;

                break;
        }
    };

    public Setter<string, ProtobufValue> String => (ref string target, ProtobufValue source) =>
    {
        switch (source.Type)
        {
            case ProtobufType.Boolean:
                target = source.Boolean ? "1" : string.Empty;

                break;

            case ProtobufType.Float32:
                target = source.Float32.ToString(CultureInfo.InvariantCulture);

                break;

            case ProtobufType.Float64:
                target = source.Float64.ToString(CultureInfo.InvariantCulture);

                break;

            case ProtobufType.Signed:
                target = source.Signed.ToString(CultureInfo.InvariantCulture);

                break;

            case ProtobufType.String:
                target = source.String;

                break;

            case ProtobufType.Unsigned:
                target = source.Unsigned.ToString(CultureInfo.InvariantCulture);

                break;

            default:
                target = string.Empty;

                break;
        }
    };
}