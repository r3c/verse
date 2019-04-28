using System;
using System.IO;

namespace Verse.DecoderDescriptors.Tree
{
    internal class TreeDecoder<TState, TNative, TEntity> : IDecoder<TEntity>
    {
        public event ErrorEvent Error;

        private readonly ReaderCallback<TState, TNative, TEntity> callback;

        private readonly Func<TEntity> constructor;

        private readonly IReader<TState, TNative> reader;

        public TreeDecoder(IReader<TState, TNative> reader, Func<TEntity> constructor,
            ReaderCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.constructor = constructor;
            this.reader = reader;
        }

        public IDecoderStream<TEntity> Open(Stream input)
        {
            var state = this.reader.Start(input, (p, m) => this.Error?.Invoke(p, m));

            return new TreeDecoderStream<TState, TNative, TEntity>(this.reader, this.constructor, this.callback, state);
        }
    }
}
