namespace Verse
{
    /// <summary>
    /// Printing error delegate.
    /// </summary>
    /// <param name="position">Position of error in output stream</param>
    /// <param name="message">Error message</param>
    public delegate void PrinterError(int position, string message);
}