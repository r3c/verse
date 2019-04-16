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
		/// <param name="converter">Elements setter to current entity</param>
		/// <param name="descriptor">Existing decoder descriptor</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter, IDecoderDescriptor<TElement> descriptor);

		/// <summary>
		/// Declare entity as a collection of elements. Resulting descriptor
		/// defines how elements should be decoded.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="converter">Elements setter to current entity</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter);

		/// <summary>
		/// Declare entity as a complex object using a setter to assign its
		/// value. Resulting field descriptor defines how its contents should
		/// be decoded.
		/// </summary>
		/// <param name="constructor">Object constructor</param>
		/// <param name="converter">Object converter to current entity</param>
		/// <returns>Object decoder field descriptor</returns>
		IDecoderObjectDescriptor<TObject> IsObject<TObject>(Func<TObject> constructor, Func<TObject, TEntity> converter);

		/// <summary>
		/// Declare entity as a complex object. Resulting field descriptor
		/// defines how its contents should be decoded.
		/// </summary>
		/// <param name="constructor">Object constructor</param>
		/// <returns>Object decoder field descriptor</returns>
		IDecoderObjectDescriptor<TEntity> IsObject(Func<TEntity> constructor);

		/// <summary>
		/// Declare entity as a value and use given converter to assign it.
		/// </summary>
		/// <param name="converter">Value to entity converter</param>
		void IsValue<TValue>(Func<TValue, TEntity> converter);

		/// <summary>
		/// Declare entity as a value. Its type must be natively compatible
		/// with current schema or have a custom decoder declared.
		/// </summary>
		void IsValue();
	}
}