using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Tree;

internal interface IWriterDefinition<TState, TNative, TEntity>
{
    WriterCallback<TState, TNative, TEntity> Callback { get; set; }

    Dictionary<string, WriterCallback<TState, TNative, TEntity>> Fields { get; }

    IWriterDefinition<TState, TNative, TOther> Create<TOther>();
}