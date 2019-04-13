using System.IO;

namespace Verse.EncoderDescriptors.Tree
{
    class TreeEncoder<TState, TNative, TEntity> : IEncoder<TEntity>
    {
        public event EncodeError Error;

        private readonly WriterCallback<TState, TNative, TEntity> callback;

        private readonly IWriterSession<TState, TNative> session;

        public TreeEncoder(IWriterSession<TState, TNative> session, WriterCallback<TState, TNative, TEntity> callback)
        {
            this.callback = callback;
            this.session = session;
        }

        public bool TryOpen(Stream output, out IEncoderStream<TEntity> encoderStream)
        {
            if (!this.session.Start(output, (p, m) => this.Error?.Invoke(p, m), out var state))
            {
                encoderStream = default;

                return false;
            }

            encoderStream = new TreeEncoderStream<TState, TNative, TEntity>(this.session, this.callback, state);

            return true;
        }
    }
}
