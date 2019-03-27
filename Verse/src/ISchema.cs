
namespace Verse
{
	/// <summary>
	/// Schema describes how to read (through a decoder descriptor) or read
	/// (through an encoder descriptor) any instance of type
	/// <typeparamref name="TEntity"/> using a given serialization format which
	/// depends on the actual implementation.
	/// </summary>
	/// <typeparam name="TEntity">Associated entity type</typeparam>
	public interface ISchema<TEntity>
	{
		/// <summary>
		/// Get decoder descriptor for this schema and entity type.
		/// </summary>
		IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get;
		}

		/// <summary>
		/// Get encoder descriptor for this schema and entity type.
		/// </summary>
		IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get;
		}

		/// <summary>
		/// Create an entity decoder based on instructions passed to the
		/// decoder descriptor associated to this schema.
		/// </summary>
		/// <returns>Decoder descriptor</returns>
		IDecoder<TEntity> CreateDecoder();

		/// <summary>
		/// Create an entity encoder based on instructions passed to the
		/// encoder descriptor associated to this schema.
		/// </summary>
		/// <returns>Encoder descriptor</returns>
		IEncoder<TEntity> CreateEncoder();
	}
}