using System.IO;

namespace Verse
{
    /// <summary>
    /// Entity parser, reads an entity from input stream using a serialization
    /// format depending on implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IParser<TEntity>
    {
        #region Events

        /// <summary>
        /// Parsing error event.
        /// </summary>
        event ParserError Error;

        #endregion

        #region Methods

        /// <summary>
        /// Parse entity from input stream.
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="output">Output entity</param>
        /// <returns>True if parsing succeeded, false otherwise</returns>
        bool Parse(Stream input, ref TEntity output);

        #endregion
    }
}