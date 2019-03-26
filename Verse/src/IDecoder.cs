using System.IO;

namespace Verse
{
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
        event DecodeError Error;

        /// <summary>
        /// Open read-enabled stream for decoding entities out of it.
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="decoderStream">Decoder stream instance</param>
        /// <returns>True if stream was successfully open for reading, false otherwise</returns>
        bool TryOpen(Stream input, out IDecoderStream<TEntity> decoderStream);
    }
}
