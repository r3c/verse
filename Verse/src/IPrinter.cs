using System.IO;

namespace Verse
{
    /// <summary>
    /// Entity printer, prints entity to output stream using a serialization
    /// format depending on implementation.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IPrinter<TEntity>
    {
        #region Events

        /// <summary>
        /// Writing error event.
        /// </summary>
        event PrinterError Error;

        #endregion

        #region Methods

        /// <summary>
        /// Print input entity to target output stream.
        /// </summary>
        /// <param name="input">Input entity</param>
        /// <param name="output">Output stream</param>
        /// <returns>True if writing succeeded, false otherwise</returns>
        bool Print(TEntity input, Stream output);

        #endregion
    }
}