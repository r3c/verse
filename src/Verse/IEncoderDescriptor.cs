using System;
using System.Collections.Generic;

namespace Verse;

/// <summary>
/// Encoder descriptor receives instructions about how to encode given
/// <typeparamref name="TEntity"/> type. Those instructions are used when
/// an actual encoder is used to write entity to a stream.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IEncoderDescriptor<TNative, out TEntity>
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
    IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>?> getter,
        IEncoderDescriptor<TNative, TElement> descriptor);

    /// <summary>
    /// Declare entity as a collection of elements. Resulting descriptor
    /// defines how elements should be encoded.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
    /// <param name="getter">Elements getter from current entity</param>
    /// <returns>Element encoder descriptor</returns>
    IEncoderDescriptor<TNative, TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>?> getter);

    /// <summary>
    /// Declare entity as an object with fields, using an intermediate object as buffer for extracting field values.
    /// Resulting descriptor defines how these fields should be encoded.
    /// </summary>
    /// <param name="converter">Entity to intermediate object converter</param>
    /// <typeparam name="TObject">Type of intermediate object</typeparam>
    /// <returns>Object decoder descriptor</returns>
    IEncoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TEntity, TObject> converter);

    /// <summary>
    /// Declare entity as an object with fields. Resulting descriptor defines how these fields should be encoded.
    /// </summary>
    /// <returns>Object decoder descriptor</returns>
    IEncoderObjectDescriptor<TNative, TEntity> IsObject();

    /// <summary>
    /// Declare entity as a value and use given converter to access it.
    /// Value type must be natively compatible with current schema or have
    /// a custom encoder declared.
    /// </summary>
    /// <param name="converter">Entity converter to native type</param>
    void IsValue(Func<TEntity, TNative> converter);
}