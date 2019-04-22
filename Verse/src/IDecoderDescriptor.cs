using System;
using System.Collections.Generic;

namespace Verse
{
	/// <summary>
	/// Decoder descriptor receives instructions about how to decode given
	/// <typeparamref name="TEntity"/> type. Those instructions are used when
	/// an actual decoder is used to read entity from a stream.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public interface IDecoderDescriptor<TEntity>
	{
		/// <summary>
		/// Declare entity as a collection of elements and reuse previously
		/// existing descriptor to define how they should be decoded. This
		/// method can be used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="constructor">Element constructor</param>
		/// <param name="setter">Elements setter to current entity</param>
		/// <param name="descriptor">Existing decoder descriptor</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TElement> HasElements<TElement>(Func<TElement> constructor,
			Setter<TEntity, IEnumerable<TElement>> setter,
			IDecoderDescriptor<TElement> descriptor);

		/// <summary>
		/// Declare entity as a collection of elements. Resulting descriptor
		/// defines how elements should be decoded.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="constructor">Element constructor</param>
		/// <param name="setter">Elements setter to current entity</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TElement> HasElements<TElement>(Func<TElement> constructor,
			Setter<TEntity, IEnumerable<TElement>> setter);

		/// <summary>
		/// Declare new named field on current object entity and reuse existing
		/// descriptor to define how it should be decoded. This method can be
		/// used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="constructor">Field constructor</param>
		/// <param name="setter">Field setter to current entity</param>
		/// <param name="descriptor">Existing decoder descriptor</param>
		/// <returns>Object decoder field descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, Func<TField> constructor,
			Setter<TEntity, TField> setter, IDecoderDescriptor<TField> descriptor);

		/// <summary>
		/// Declare new named field on current object entity. Resulting
		/// descriptor defines how it should be decoded.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="constructor">Field constructor</param>
		/// <param name="setter">Field setter to current entity</param>
		/// <returns>Object decoder field descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, Func<TField> constructor,
			Setter<TEntity, TField> setter);

		/// <summary>
		/// Declare new named field on current object entity without creating a
		/// dedicated instance for this field. This method can be used to
		/// flatten complex hierarchies when mapping them.
		/// </summary>
		/// <param name="name">Field name</param>
		/// <returns>Current entity field descriptor</returns>
		IDecoderDescriptor<TEntity> HasField(string name);

		/// <summary>
		/// Declare entity as a value and use given setter to assign it. Value
		/// type must be natively compatible with current schema or have a
		/// custom decoder declared.
		/// </summary>
		/// <typeparam name="TValue">Value type</typeparam>
		/// <param name="setter">Value to entity converter</param>
		void HasValue<TValue>(Setter<TEntity, TValue> setter);

		/// <summary>
		/// Declare entity as a value. Its type must be natively compatible
		/// with current schema or have a custom decoder declared.
		/// </summary>
		void HasValue();
	}
}
