using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
    internal class TreeEncoder<TState, TNative, TEntity> : IEncoder<TEntity>
    {
        public event ErrorEvent Error;

        private readonly WriterCallback<TState, TNative, TEntity> callback;

        private readonly IWriter<TState, TNative> reader;

        public TreeEncoder(IWriter<TState, TNative> reader, WriterCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.reader = reader;
        }

        public IEncoderStream<TEntity> Open(Stream output)
        {
            var state = this.reader.Start(output, (p, m) => this.Error?.Invoke(p, m));

            return new TreeEncoderStream<TState, TNative, TEntity>(this.reader, this.callback, state);
        }
    }
}
