using System;
using Verse.Formats.String;

namespace Verse.Formats;

internal class StringFormat : IFormat<string>
{
    public static readonly StringFormat Instance = new();

    /// <inheritdoc/>
    public string DefaultValue => string.Empty;

    /// <inheritdoc/>
    public IEncoderAdapter<string> From => throw new NotImplementedException("encoding not implemented");

    /// <inheritdoc/>
    public IDecoderAdapter<string> To => StringDecoderAdapter.Instance;
}