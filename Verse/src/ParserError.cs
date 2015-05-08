namespace Verse
{
    /// <summary>
    /// Parsing error delegate.
    /// </summary>
    /// <param name="position">Position of error in input stream</param>
    /// <param name="message">Error message</param>
    public delegate void ParserError(int position, string message);
}