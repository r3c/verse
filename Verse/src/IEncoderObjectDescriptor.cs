using System;

namespace Verse
{
	/// <summary>
	/// Encoder object descriptor receives instructions about how to encode
	/// fields of given <typeparamref name="TEntity"/> object.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IEncoderObjectDescriptor<out TEntity>
	{
		/// <summary>
		/// Declare new named field on current object entity and reuse existing
		/// descriptor to define how it should be encoded. This method can be
		/// used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="getter">Field getter from current entity</param>
		/// <param name="descriptor">Existing encoder descriptor</param>
		/// <returns>Field encoder descriptor</returns>
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter, IEncoderDescriptor<TField> descriptor);

		/// <summary>
		/// Declare new named field on current object entity. Resulting
		/// descriptor defines how it should be encoded.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="getter">Field getter from current entity</param>
		/// <returns>Field encoder descriptor</returns>
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter);
	}
}