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
	public interface IEncoderDescriptor<TEntity>
	{
		#region Methods

		/// <summary>
		/// Declare new named schema field and reuse previously existing
		/// descriptor to define how it should be encoded. This method
		/// can be used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="access">Access child entity from current one</param>
		/// <param name="parent">Existing encoder descriptor</param>
		/// <returns>Field encoder descriptor</returns>
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent);

		/// <summary>
		/// Declare new named schema field which should be encoded from a new
		/// child entity. Resulting descriptor defines how this field should
		/// be encoded from child entity.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="access">Access child entity from current one</param>
		/// <returns>Field encoder descriptor</returns>
		IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

		/// <summary>
		/// Declare new named schema field which should be encoded from current
		/// entity. Resulting descriptor defines how contents under this field
		/// should be encoded from entity.
		/// </summary>
		/// <param name="name">Field name</param>
		/// <returns>Entity encoder descriptor</returns>
		IEncoderDescriptor<TEntity> HasField(string name);

		/// <summary>
		/// Declare new elements collection within current entity, and reuse
		/// existing encoder to describe them.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="access">Parent entity to elements accessor delegate</param>
		/// <param name="parent">Existing encoder descriptor for this elements
		/// collection, needed if you want to declare recursive entities</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IEncoderDescriptor<TElement> parent);

		/// <summary>
		/// Declare new elements collection within current entity.
		/// </summary>
		/// <typeparam name="TElement">Element type</typeparam>
		/// <param name="access">Parent entity to elements accessor delegate</param>
		/// <returns>Element encoder descriptor</returns>
		IEncoderDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access);

		/// <summary>
		/// Declare entity as a value. Entity type must have a known encoder
		/// declared (through its schema), otherwise you'll get a type error
		/// when calling this method.
		/// </summary>
		void IsValue();

		#endregion
	}
}