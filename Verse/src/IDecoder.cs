using System.IO;

namespace Verse
{
    /// <summary>
    /// Entity decoder, reads an entity from input stream using a serialization
    /// format depending on implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IDecoder<TEntity>
    {
        #region Events

        /// <summary>
        /// Decoding error event.
        /// </summary>
        event DecodeError Error;

        #endregion

        #region Methods

        /// <summary>
        /// Read entity from input stream.
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="output">Output entity</param>
        /// <returns>True if decoding succeeded, false otherwise</returns>
        bool Decode(Stream input, ref TEntity output);

        #endregion
    }
}