namespace Verse
{
	/// <summary>
	/// Decoding error delegate.
	/// </summary>
	/// <param name="position">Position of error in input stream</param>
	/// <param name="message">Error message</param>
	public delegate void DecodeError(int position, string message);
}