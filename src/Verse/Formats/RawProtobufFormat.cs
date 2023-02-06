using Verse.Formats.RawProtobuf;

namespace Verse.Formats;

internal class RawProtobufFormat : IFormat<RawProtobufValue>
{
    public static readonly RawProtobufFormat Instance = new();

    /// <inheritdoc/>
    public RawProtobufValue DefaultValue => new(0, RawProtobufWireType.VarInt);

    /// <inheritdoc/>
    public IEncoderAdapter<RawProtobufValue> From => RawProtobufEncoderAdapter.Instance;

    /// <inheritdoc/>
    public IDecoderAdapter<RawProtobufValue> To => RawProtobufDecoderAdapter.Instance;
}