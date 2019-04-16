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
		/// <param name="converter">Elements converter from current entity</param>
		/// <param name="descriptor">Existing encoder descriptor</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> converter, IEncoderDescriptor<TElement> descriptor);

		/// <summary>
		/// Declare entity as a collection of elements. Resulting descriptor
		/// defines how elements should be encoded.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="converter">Elements converter from current entity</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> converter);

		/// <summary>
		/// Declare entity as a complex object using a converter to read its
		/// value. Resulting field descriptor defines how its contents should
		/// be encoded.
		/// </summary>
		/// <param name="converter">Object converter from current entity</param>
		/// <returns>Object encoder field descriptor</returns>
		IEncoderObjectDescriptor<TObject> IsObject<TObject>(Func<TEntity, TObject> converter);

		/// <summary>
		/// Declare entity as a complex object. Resulting field descriptor
		/// defines how its contents should be encoded.
		/// </summary>
		/// <returns>Object encoder field descriptor</returns>
		IEncoderObjectDescriptor<TEntity> IsObject();

		/// <summary>
		/// Declare entity as a value and use given converter to access it.
		/// </summary>
		/// <param name="converter">Entity to value converter</param>
		void IsValue<TValue>(Func<TEntity, TValue> converter);

		/// <summary>
		/// Declare entity as a value. Its type must be natively compatible
		/// with current schema or have a custom encoder declared.
		/// </summary>
		void IsValue();
	}
}