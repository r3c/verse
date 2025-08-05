using Verse.Formats.Json;
using Verse.Schemas.Json;

namespace Verse.Formats;

internal class JsonFormat : IFormat<JsonValue>
{
    public static readonly JsonFormat Instance = new();

    /// <inheritdoc/>
    public JsonValue DefaultValue => JsonValue.Undefined;

    /// <inheritdoc/>
    public IEncoderAdapter<JsonValue> From => JsonEncoderAdapter.Instance;

    /// <inheritdoc/>
    public IDecoderAdapter<JsonValue> To => JsonDecoderAdapter.Instance;
}