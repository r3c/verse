using Verse.Formats.Protobuf;

namespace Verse.Formats;

internal class ProtobufFormat : IFormat<ProtobufValue>
{
    public static readonly ProtobufFormat Instance = new();

    /// <inheritdoc/>
    public ProtobufValue DefaultValue => ProtobufValue.Empty;

    /// <inheritdoc/>
    public IEncoderAdapter<ProtobufValue> From => ProtobufEncoderAdapter.Instance;

    /// <inheritdoc/>
    public IDecoderAdapter<ProtobufValue> To => ProtobufDecoderAdapter.Instance;
}