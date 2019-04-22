using System;
using System.Collections.Generic;

namespace Verse
{
	/// <summary>
	/// Encoder descriptor receives instructions about how to encode given
	/// <typeparamref name="TEntity"/> type. Those instructions are used when
	/// an actual encoder is used to write entity to a stream.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IEncoderDescriptor<out TEntity>
	{
		/// <summary>
		/// Declare entity as a collection of elements and reuse existing
		/// descriptor to define how they should be encoded. This method can be
		/// used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="getter">Elements getter from current entity</param>
		/// <param name="descriptor">Existing encoder descriptor</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter,
			IEncoderDescriptor<TElement> descriptor);

		/// <summary>
		/// Declare entity as a collection of elements. Resulting descriptor
		/// defines how elements should be encoded.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="getter">Elements getter from current entity</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> HasElements<TElement>(Func<TEntity, IEnumerable<TElement>> getter);

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
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter,
			IEncoderDescriptor<TField> descriptor);

		/// <summary>
		/// Declare new named field on current object entity. Resulting
		/// descriptor defines how it should be encoded.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="getter">Field getter from current entity</param>
		/// <returns>Field encoder descriptor</returns>
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> getter);

		/// <summary>
		/// Declare new named field on current object entity without using a
		/// dedicated instance for this field. This method can be used to
		/// flatten complex hierarchies when mapping them.
		/// </summary>
		/// <param name="name">Field name</param>
		/// <returns>Current entity encoder descriptor</returns>
		IEncoderDescriptor<TEntity> HasField(string name);

		/// <summary>
		/// Declare entity as a value and use given converter to access it.
		/// Value type must be natively compatible with current schema or have
		/// a custom encoder declared.
		/// </summary>
		/// <typeparam name="TValue">Value type</typeparam>
		/// <param name="getter">Entity to value getter</param>
		void HasValue<TValue>(Func<TEntity, TValue> getter);

		/// <summary>
		/// Declare entity as a value. Its type must be natively compatible
		/// with current schema or have a custom encoder declared.
		/// </summary>
		void HasValue();
	}
}
