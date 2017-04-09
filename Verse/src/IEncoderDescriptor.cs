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
        /// Declare new named field in current entity, and reuse existing encoder
        /// descriptor to describe it.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="access">Parent entity to field accessor delegate</param>
        /// <param name="parent">Existing encoder descriptor for this field,
        /// needed if you want to declare recursive entities</param>
        /// <returns>Field encoder descriptor</returns>
        IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent);

        /// <summary>
        /// Declare new named field in current entity.
        /// </summary>
        /// <typeparam name="TField">Field type</typeparam>
        /// <param name="name">Field name</param>
        /// <param name="access">Parent entity to field accessor delegate</param>
        /// <returns>Field encoder descriptor</returns>
        IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

        /// <summary>
        /// Declare new named field in current entity, but keep describing its
        /// content on parent entity type rather than a new type.
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

/*
        IEncoderDescriptor<U> IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access, IEncoderDescriptor<U> parent);

        IEncoderDescriptor<U> IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access);
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