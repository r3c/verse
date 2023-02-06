namespace Verse;

public interface IFormat<TNative>
{
    /// <summary>
    /// Native value considered as default.
    /// </summary>
    TNative DefaultValue { get; }

    /// <summary>
    /// Get value adapter for encoding native C# types to format values.
    /// </summary>
    IEncoderAdapter<TNative> From { get; }

    /// <summary>
    /// Get value adapter for decoding format values to native C# types.
    /// </summary>
    IDecoderAdapter<TNative> To { get; }
}