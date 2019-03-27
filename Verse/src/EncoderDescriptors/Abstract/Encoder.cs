using System.IO;

namespace Verse.EncoderDescriptors.Abstract
{
    class Encoder<TEntity, TState> : IEncoder<TEntity>
    {
        public event EncodeError Error;

        private readonly IWriterSession<TState> session;

        private readonly IWriter<TEntity, TState> writer;

        public Encoder(IWriterSession<TState> session, IWriter<TEntity, TState> writer)
        {
            this.session = session;
            this.writer = writer;
        }

        public bool TryOpen(Stream output, out IEncoderStream<TEntity> encoderStream)
        {
            if (!this.session.Start(output, this.OnError, out var state))
            {
                encoderStream = default;

                return false;
            }

            encoderStream = new EncoderStream<TEntity, TState>(session, writer, state);

            return true;
        }

        private void OnError(int position, string message)
        {
            this.Error?.Invoke(position, message);
        }
    }
}
