namespace Verse.EncoderDescriptors.Tree;

internal delegate bool WriterCallback<TState, TNative, in TEntity>(IWriter<TState, TNative> reader, TState state,
    TEntity source);