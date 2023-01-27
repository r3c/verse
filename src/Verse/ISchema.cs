namespace Verse;

/// <summary>
/// Schema describes how to read (through a decoder descriptor) or read
/// (through an encoder descriptor) any instance of type
/// <typeparamref name="TEntity"/> using a given serialization format which
/// depends on the actual implementation.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
/// <typeparam name="TEntity">Associated entity type</typeparam>
public interface ISchema<TNative, TEntity>
{
    /// <summary>
    /// Get value adapter for decoding schema values to native C# types.
    /// </summary>
    IDecoderAdapter<TNative> DecoderAdapter { get; }

    /// <summary>
    /// Get decoder descriptor for this schema and entity type.
    /// </summary>
    IDecoderDescriptor<TNative, TEntity> DecoderDescriptor { get; }

    /// <summary>
    /// Get value adapter for encoding native C# types to schema values.
    /// </summary>
    IEncoderAdapter<TNative> EncoderAdapter { get; }

    /// <summary>
    /// Get encoder descriptor for this schema and entity type.
    /// </summary>
    IEncoderDescriptor<TNative, TEntity> EncoderDescriptor { get; }

    /// <summary>
    /// Native value considered as null for this schema.
    /// </summary>
    TNative NullValue { get; }

    /// <summary>
    /// Create an entity decoder based on instructions passed to the
    /// decoder descriptor associated to this schema.
    /// </summary>
    /// <returns>Decoder descriptor</returns>
    IDecoder<TEntity> CreateDecoder();

    /// <summary>
    /// Create an entity encoder based on instructions passed to the
    /// encoder descriptor associated to this schema.
    /// </summary>
    /// <returns>Encoder descriptor</returns>
    IEncoder<TEntity> CreateEncoder();
}