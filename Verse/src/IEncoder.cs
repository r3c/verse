using System.IO;

namespace Verse
{
    /// <summary>
    /// Entity encoder, open stream for writing entities to it using a
    /// serialization format depending on implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IEncoder<in TEntity>
    {
        /// <summary>
        /// Encoding error event.
        /// </summary>
        event EncodeError Error;

        /// <summary>
        /// Open write-enabled stream for encoding entities to it.
        /// </summary>
        /// <param name="output">Output stream</param>
        /// <returns>Encoder stream instance</returns>
        IEncoderStream<TEntity> Open(Stream output);
    }
}
