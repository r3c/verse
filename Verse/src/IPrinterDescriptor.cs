using System;
using System.Collections.Generic;

namespace Verse
{
    /// <summary>
    /// Printer descriptor receives instructions about how to print given
    /// <typeparamref name="TEntity"/> type. Those instructions are used when
    /// an actual printer is used to print entity to a stream.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public interface IPrinterDescriptor<TEntity>
    {
        #region Methods

        /// <summary>
        /// Declare new named field in current entity, and reuse existing printer
        /// descriptor to describe it.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="access">Parent entity to field accessor delegate</param>
        /// <param name="parent">Existing printer descriptor for this field,
        /// needed if you want to declare recursive entities</param>
        /// <returns>Field printer descriptor</returns>
        IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IPrinterDescriptor<TField> parent);

        /// <summary>
        /// Declare new named field in current entity.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="access">Parent entity to field accessor delegate</param>
        /// <returns>Field printer descriptor</returns>
        IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

        /// <summary>
        /// Declare new named field in current entity, but keep describing its
        /// content on parent entity type rather than a new type.
        /// </summary>
        /// <param name="name">Field name</param>
        /// <returns>Entity printer descriptor</returns>
        IPrinterDescriptor<TEntity> HasField(string name);

        /// <summary>
        /// Declare new elements collection within current entity, and reuse
        /// existing printer to describe them.
        /// </summary>
        /// <typeparam name="TElement">Element type</typeparam>
        /// <param name="access">Parent entity to elements accessor delegate</param>
        /// <param name="parent">Existing printer descriptor for this elements
        /// collection, needed if you want to declare recursive entities</param>
        /// <returns>Element printer descriptor</returns>
        IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IPrinterDescriptor<TElement> parent);

        /// <summary>
        /// Declare new elements collection within current entity.
        /// </summary>
        /// <typeparam name="TElement">Element type</typeparam>
        /// <param name="access">Parent entity to elements accessor delegate</param>
        /// <returns>Element printer descriptor</returns>
        IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access);

/*
        IPrinterDescriptor<U> IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access, IPrinterDescriptor<U> parent);

        IPrinterDescriptor<U> IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access);
*/

        /// <summary>
        /// Declare accessible value within current entity.
        /// </summary>
        /// <typeparam name="TCompatible">Value type before conversion</typeparam>
        /// <param name="access">Parent entity to value accessor delegate</param>
        void IsValue<TCompatible>(Func<TEntity, TCompatible> access);

        /// <summary>
        /// Declare entity as a value. Entity type must have a known encoder
        /// declared (through its schema), otherwise you'll get a type error
        /// when calling this method.
        /// </summary>
        void IsValue();

        #endregion
    }
}