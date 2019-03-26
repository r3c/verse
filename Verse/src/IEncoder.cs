using System.IO;

namespace Verse
{
    /// <summary>
    /// Entity encoder, open stream for writing entities to it using a
    /// serialization format depending on implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IEncoder<TEntity>
    {
        /// <summary>
        /// Encoding error event.
        /// </summary>
        event EncodeError Error;

        /// <summary>
        /// Open write-enabled stream for encoding entities to it.
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <param name="encoderStream">Encoder stream instance</param>
        /// <returns>True if stream was successfully open for writing, false otherwise</returns>
        bool TryOpen(Stream output, out IEncoderStream<TEntity> encoderStream);
    }
}
