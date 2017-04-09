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
        void CanCreate<TMember>(Func<TEntity, TMember> constructor);

        /// <summary>
        /// Declare new named field in current entity, and reuse existing decoder
        /// descriptor to describe it.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="assign">Field to parent entity assignment delegate</param>
        /// <param name="parent">Existing decoder descriptor for this field,
        /// needed if you want to declare recursive entities</param>
        /// <returns>Field decoder descriptor</returns>
        IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent);

        /// <summary>
        /// Declare new named field in current entity.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="assign">Field to parent entity assignment delegate</param>
        /// <returns>Field decoder descriptor</returns>
        IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign);

        /// <summary>
        /// Declare new named field in current entity, but keep describing its
        /// content on parent entity type rather than a new type.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <typeparam name="TEntity">Field type</typeparam>
        /// <returns>Entity decoder descriptor</returns>
        IDecoderDescriptor<TEntity> HasField(string name);

        /// <summary>
        /// Declare new elements collection within current entity, and reuse
        /// existing decoder to describe them.
        /// </summary>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <param name="assign">Elements to parent entity assignment delegate</param>
        /// <param name="parent">Existing decoder descriptor for this elements
        /// collection, needed if you want to declare recursive entities</param>
        /// <returns>Element decoder descriptor</returns>
        IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent);

        /// <summary>
        /// Declare new elements collection within current entity.
        /// </summary>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <param name="assign">Elements to parent entity assignment delegate</param>
        /// <returns>Element decoder descriptor</returns>
        IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign);

        /// <summary>
        /// Declare assignable value within current entity.
        /// </summary>
        /// <typeparam name="TCompatible">Value type after conversion</typeparam>
        /// <param name="assign">Value to parent entity assignment delegate</param>
        void IsValue<TCompatible>(DecodeAssign<TEntity, TCompatible> assign);

        /// <summary>
        /// Declare entity as a value. Entity type must have a known decoder
        /// declared (through its schema), otherwise you'll get a type error
        /// when calling this method.
        /// </summary>
        void IsValue();

        #endregion
    }
}