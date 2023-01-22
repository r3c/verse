using System;
using System.Collections.Generic;

namespace Verse;

/// <summary>
/// Decoder descriptor receives instructions about how to decode given
/// <typeparamref name="TEntity"/> type. Those instructions are used when
/// an actual decoder is used to read entity from a stream.
/// </summary>
/// <typeparam name="TNative">Schema native value type</typeparam>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IDecoderDescriptor<TNative, TEntity>
{
    /// <summary>
    /// Declare entity as a collection of elements and reuse previously
    /// existing descriptor to define how they should be decoded. This
    /// method can be used to describe recursive schemas.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
    /// <param name="converter">Elements converter to current entity</param>
    /// <param name="descriptor">Existing decoder descriptor</param>
    /// <returns>Element decoder descriptor</returns>
    IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter,
        IDecoderDescriptor<TNative, TElement> descriptor);

    /// <summary>
    /// Declare entity as a collection of elements. Resulting descriptor
    /// defines how elements should be decoded.
    /// </summary>
    /// <typeparam name="TElement">Element type</typeparam>
    /// <param name="converter">Elements setter to current entity</param>
    /// <returns>Element decoder descriptor</returns>
    IDecoderDescriptor<TNative, TElement> IsArray<TElement>(Func<IEnumerable<TElement>, TEntity> converter);

    IDecoderObjectDescriptor<TNative, TObject> IsObject<TObject>(Func<TObject> constructor,
        Func<TObject, TEntity> converter);

    IDecoderObjectDescriptor<TNative, TEntity> IsObject(Func<TEntity> constructor);

    /// <summary>
    /// Declare entity as a value and use given setter to assign it. Value
    /// type must be natively compatible with current schema or have a
    /// custom decoder declared.
    /// </summary>
    /// <param name="converter">Entity converter from native type</param>
    void IsValue(Setter<TEntity, TNative> converter);
}