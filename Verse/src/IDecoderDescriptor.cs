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
		#region Methods

		/// <summary>
		/// Register constructor which will be used as replacement for default
		/// one when a field, element or value of associated type must be
		/// created before its content is read from stream. 
		/// </summary>
		/// <typeparam name="TMember">Constructed type</typeparam>
		/// <param name="constructor">Type constructor</param>
		void CanCreate<TMember>(Func<TMember> constructor);

		/// <summary>
		/// Declare new named schema field and reuse previously existing
		/// descriptor to define how it should be decoded. This method
		/// can be used to describe recursive schemas.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="assign">Assign child entity to current one</param>
		/// <param name="parent">Existing decoder descriptor</param>
		/// <returns>Field decoder descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent);

		/// <summary>
		/// Declare new named schema field which should be decoded into a new
		/// child entity. Resulting descriptor defines how this field should
		/// be decoded into child entity.
		/// </summary>
		/// <typeparam name="TField">Field type</typeparam>
		/// <param name="name">Field name</param>
		/// <param name="assign">Assign child entity to current one</param>
		/// <returns>Field decoder descriptor</returns>
		IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign);

		/// <summary>
		/// Declare new named schema field which should be decoded into current
		/// entity. Resulting descriptor defines how contents under this field
		/// should be decoded into entity.
		/// </summary>
		/// <param name="name">Field name</param>
		/// <returns>Entity decoder descriptor</returns>
		IDecoderDescriptor<TEntity> HasField(string name);

		/// <summary>
		/// Declare new elements collection within current entity, and reuse
		/// existing decoder to describe them.
		/// </summary>
		/// <typeparam name="TItem">Array element type</typeparam>
		/// <param name="assign">Elements to parent entity assignment delegate</param>
		/// <param name="parent">Existing decoder descriptor for this elements
		/// collection, needed if you want to declare recursive entities</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign, IDecoderDescriptor<TItem> parent);

		/// <summary>
		/// Declare new elements collection within current entity.
		/// </summary>
		/// <typeparam name="TItem">Array element type</typeparam>
		/// <param name="assign">Elements to parent entity assignment delegate</param>
		/// <returns>Element decoder descriptor</returns>
		IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign);

		/// <summary>
		/// Declare entity as a value. Entity type must have a known decoder
		/// declared (through its schema), otherwise you'll get a type error
		/// when calling this method.
		/// </summary>
		void IsValue();

		#endregion
	}
}