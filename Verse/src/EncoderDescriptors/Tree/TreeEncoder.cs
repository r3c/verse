using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
    internal class TreeEncoder<TState, TNative, TEntity> : IEncoder<TEntity>
    {
        public event ErrorEvent Error;

        private readonly WriterCallback<TState, TNative, TEntity> callback;

        private readonly IWriter<TState, TNative> session;

        public TreeEncoder(IWriter<TState, TNative> session, WriterCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.session = session;
        }

        public IEncoderStream<TEntity> Open(Stream output)
        {
            var state = this.session.Start(output, (p, m) => this.Error?.Invoke(p, m));

            return new TreeEncoderStream<TState, TNative, TEntity>(this.session, this.callback, state);
        }
    }
}
