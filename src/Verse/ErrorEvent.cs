namespace Verse;

/// <summary>
/// Error event delegate.
/// </summary>
/// <param name="position">Position of error in stream</param>
/// <param name="message">Error verbose message</param>
public delegate void ErrorEvent(int position, string message);