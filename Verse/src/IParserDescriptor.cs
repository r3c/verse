using System;
using System.Collections.Generic;

namespace Verse
{
    /// <summary>
    /// Parser descriptor receives instructions about how to parse given
    /// <typeparamref name="TEntity"/> type. Those instructions are used when
    /// an actual parser is used to read entity from a stream.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IParserDescriptor<TEntity>
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
        /// Declare new named field in current entity, and reuse existing parser
        /// descriptor to describe it.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="assign">Field to parent entity assignment delegate</param>
        /// <param name="parent">Existing parser descriptor for this field,
        /// needed if you want to declare recursive entities</param>
        /// <returns>Field parser descriptor</returns>
        IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent);

        /// <summary>
        /// Declare new named field in current entity.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <typeparam name="TKey">Key type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="assign">Field to parent entity assignment delegate</param>
        /// <returns>Field parser descriptor</returns>
        IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign);

        /// <summary>
        /// Declare new named field in current entity, but keep describing its
        /// content on parent entity type rather than a new type.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <typeparam name="TEntity">Field type</typeparam>
        /// <returns>Entity parser descriptor</returns>
        IParserDescriptor<TEntity> HasField(string name);

        /// <summary>
        /// Declare new elements collection within current entity, and reuse
        /// existing parser to describe them.
        /// </summary>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <param name="assign">Elements to parent entity assignment delegate</param>
        /// <param name="parent">Existing parser descriptor for this elements
        /// collection, needed if you want to declare recursive entities</param>
        /// <returns>Element parser descriptor</returns>
        IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, IParserDescriptor<TElement> parent);

        /// <summary>
        /// Declare new elements collection within current entity.
        /// </summary>
        /// <typeparam name="TElement">Array element type</typeparam>
        /// <param name="assign">Elements to parent entity assignment delegate</param>
        /// <returns>Element parser descriptor</returns>
        IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign);

        /// <summary>
        /// Declare assignable value within current entity.
        /// </summary>
        /// <typeparam name="TCompatible">Value type after conversion</typeparam>
        /// <param name="assign">Value to parent entity assignment delegate</param>
        void IsValue<TCompatible>(ParserAssign<TEntity, TCompatible> assign);

        /// <summary>
        /// Declare entity as a value. Entity type must have a known decoder
        /// declared (through its schema), otherwise you'll get a type error
        /// when calling this method.
        /// </summary>
        void IsValue();

        #endregion
    }
}