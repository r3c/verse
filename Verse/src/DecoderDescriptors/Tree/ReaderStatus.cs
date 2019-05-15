namespace Verse.DecoderDescriptors.Tree
{
	internal enum ReaderStatus
	{
		/// <summary>
		/// Read operation has failed.
		/// </summary>
		Failed,

		/// <summary>
		/// Read operation has succeeded.
		/// </summary>
		Succeeded,

		/// <summary>
		/// Read operation result must be ignored.
		/// </summary>
		Ignored
	}
}
