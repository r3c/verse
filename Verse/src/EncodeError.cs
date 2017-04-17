namespace Verse
{
	/// <summary>
	/// Encoding error delegate.
	/// </summary>
	/// <param name="position">Position of error in output stream</param>
	/// <param name="message">Error message</param>
	public delegate void EncodeError(int position, string message);
}