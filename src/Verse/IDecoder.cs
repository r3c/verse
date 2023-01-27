using System.IO;

namespace Verse;

/// <summary>
/// Entity decoder, open stream for reading entities out of it using a
/// serialization format depending on implementation.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IDecoder<TEntity>
{
    /// <summary>
    /// Decoding error event.
    /// </summary>
    event ErrorEvent? Error;

    /// <summary>
    /// Open read-enabled stream for decoding entities out of it.
    /// </summary>
    /// <param name="input">Input stream</param>
    /// <returns>Decoder stream instance</returns>
    IDecoderStream<TEntity> Open(Stream input);
}