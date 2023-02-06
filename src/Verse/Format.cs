using Verse.Formats;
using Verse.Formats.Json;
using Verse.Formats.Protobuf;
using Verse.Formats.RawProtobuf;

namespace Verse;

public static class Format
{
    public static IFormat<JsonValue> Json => JsonFormat.Instance;
    public static IFormat<ProtobufValue> Protobuf => ProtobufFormat.Instance;
    public static IFormat<RawProtobufValue> RawProtobuf => RawProtobufFormat.Instance;
    public static IFormat<string> String => StringFormat.Instance;
}