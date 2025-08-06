using System;
using System.Diagnostics.CodeAnalysis;

namespace Verse;

/// <summary>
/// Entity decoder, reads an entity from input stream using a serialization
/// format depending on implementation.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IDecoderStream<TEntity> : IDisposable
{
    /// <summary>
    /// Read entity from input stream.
    /// </summary>
    /// <param name="entity">Output entity</param>
    /// <returns>True if decoding succeeded, false otherwise</returns>
    bool TryDecode([NotNullWhen(true)] out TEntity? entity);
}