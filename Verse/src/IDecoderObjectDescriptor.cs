
namespace Verse
{
	/// <summary>
	/// Decoder object descriptor receives instructions about how to decode
	/// fields of given <typeparamref name="TEntity"/> object.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IDecoderObjectDescriptor<TEntity>
	{
		/// <summary>
		/// Declare new named field on current object entity and reuse existing
		/// descriptor to define how it should be decoded. This method can be
		/// used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="setter">Field setter to current entity</param>
		/// <param name="descriptor">Existing decoder descriptor</param>
		/// <returns>Field decoder descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, Setter<TEntity, TField> setter, IDecoderDescriptor<TField> descriptor);

		/// <summary>
		/// Declare new named field on current object entity. Resulting
		/// descriptor defines how it should be decoded.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="setter">Field setter to current entity</param>
		/// <returns>Field decoder descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, Setter<TEntity, TField> setter);
	}
}