using System;

namespace Verse.Schemas.Json;

internal class JsonEncoderAdapter : IEncoderAdapter<JsonValue>
{
    public Func<bool, JsonValue> FromBoolean => JsonValue.FromBoolean;

    public Func<char, JsonValue> FromCharacter => v => JsonValue.FromString(new string(v, 1));

    public Func<decimal, JsonValue> FromDecimal => v => JsonValue.FromNumber((double) v);

    public Func<float, JsonValue> FromFloat32 => v => JsonValue.FromNumber(v);

    public Func<double, JsonValue> FromFloat64 => JsonValue.FromNumber;

    public Func<sbyte, JsonValue> FromInteger8S => v => JsonValue.FromNumber(v);

    public Func<byte, JsonValue> FromInteger8U => v => JsonValue.FromNumber(v);

    public Func<short, JsonValue> FromInteger16S => v => JsonValue.FromNumber(v);

    public Func<ushort, JsonValue> FromInteger16U => v => JsonValue.FromNumber(v);

    public Func<int, JsonValue> FromInteger32S => v => JsonValue.FromNumber(v);

    public Func<uint, JsonValue> FromInteger32U => v => JsonValue.FromNumber(v);

    public Func<long, JsonValue> FromInteger64S => v => JsonValue.FromNumber(v);

    public Func<ulong, JsonValue> FromInteger64U => v => JsonValue.FromNumber(v);

    public Func<string, JsonValue> FromString => JsonValue.FromString;
}