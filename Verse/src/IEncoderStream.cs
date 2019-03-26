
namespace Verse
{
	/// <summary>
	/// Entity encoder, writes entity to output stream using a serialization
	/// format depending on implementation.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IEncoderStream<in TEntity>
	{
		/// <summary>
		/// Write entity to output stream.
		/// </summary>
		/// <param name="input">Input entity</param>
		/// <returns>True if encoding succeeded, false otherwise</returns>
		bool Encode(TEntity input);
	}
}