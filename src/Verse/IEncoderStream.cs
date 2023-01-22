using System;

namespace Verse;

/// <summary>
/// Entity encoder, writes entity to output stream using a serialization
/// format depending on implementation.
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
public interface IEncoderStream<in TEntity> : IDisposable
{
    /// <summary>
    /// Write entity to output stream.
    /// </summary>
    /// <param name="entity">Input entity</param>
    /// <returns>True if encoding succeeded, false otherwise</returns>
    void Encode(TEntity entity);
}