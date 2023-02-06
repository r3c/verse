using System;
using System.Reflection;

namespace Verse;

/// <summary>
/// Schema linker.
/// </summary>
/// <typeparam name="TNative">Type of native values for current serialization format</typeparam>
public interface ILinker<TNative>
{
    /// <summary>
    /// Create a decoder for given schema on target entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="format">Format specification</param>
    /// <param name="schema">Entity schema</param>
    /// <returns>Entity decoder</returns>
    IDecoder<TEntity> CreateDecoder<TEntity>(IFormat<TNative> format, ISchema<TNative, TEntity> schema);

    /// <summary>
    /// Create an encoder for given schema on target entity.
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    /// <param name="format">Format specification</param>
    /// <param name="schema">Entity schema</param>
    /// <returns>Entity encoder</returns>
    IEncoder<TEntity> CreateEncoder<TEntity>(IFormat<TNative> format, ISchema<TNative, TEntity> schema);

    ILinker<TNative> SetBindingFlags(BindingFlags bindingFlags);

    ILinker<TNative> SetDecoderDescriptor<TEntity>(Action<IDecoderDescriptor<TNative, TEntity>> describe);

    ILinker<TNative> SetEncoderDescriptor<TEntity>(Action<IEncoderDescriptor<TNative, TEntity>> describe);
}