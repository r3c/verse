using System.IO;

namespace Verse
{
	/// <summary>
	/// Entity encoder, writes entity to output stream using a serialization
	/// format depending on implementation.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IEncoder<TEntity>
	{
		#region Events

		/// <summary>
		/// Encoding error event.
		/// </summary>
		event EncodeError Error;

		#endregion

		#region Methods

		/// <summary>
		/// Write entity to output stream.
		/// </summary>
		/// <param name="input">Input entity</param>
		/// <param name="output">Output stream</param>
		/// <returns>True if encoding succeeded, false otherwise</returns>
		bool Encode(TEntity input, Stream output);

		#endregion
	}
}