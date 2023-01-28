using System;

namespace Verse.Schemas.Json;

internal class JsonEncoderAdapter : IEncoderAdapter<JsonValue>
{
    public Func<bool, JsonValue> Boolean => JsonValue.FromBoolean;

    public Func<char, JsonValue> Character => v => JsonValue.FromString(new string(v, 1));

    public Func<decimal, JsonValue> Decimal => v => JsonValue.FromNumber((double) v);

    public Func<float, JsonValue> Float32 => v => JsonValue.FromNumber(v);

    public Func<double, JsonValue> Float64 => JsonValue.FromNumber;

    public Func<sbyte, JsonValue> Integer8S => v => JsonValue.FromNumber(v);

    public Func<byte, JsonValue> Integer8U => v => JsonValue.FromNumber(v);

    public Func<short, JsonValue> Integer16S => v => JsonValue.FromNumber(v);

    public Func<ushort, JsonValue> Integer16U => v => JsonValue.FromNumber(v);

    public Func<int, JsonValue> Integer32S => v => JsonValue.FromNumber(v);

    public Func<uint, JsonValue> Integer32U => v => JsonValue.FromNumber(v);

    public Func<long, JsonValue> Integer64S => v => JsonValue.FromNumber(v);

    public Func<ulong, JsonValue> Integer64U => v => JsonValue.FromNumber(v);

    public Func<string, JsonValue> String => JsonValue.FromString;
}