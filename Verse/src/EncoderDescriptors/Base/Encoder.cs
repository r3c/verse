using System.IO;

namespace Verse.EncoderDescriptors.Base
{
    class Encoder<TEntity, TState> : IEncoder<TEntity>
    {
        public event EncodeError Error;

        private readonly IWriterSession<TState> session;

        private readonly IWriter<TState, TEntity> writer;

        public Encoder(IWriterSession<TState> session, IWriter<TState, TEntity> writer)
        {
            this.session = session;
            this.writer = writer;
        }

        public bool TryOpen(Stream output, out IEncoderStream<TEntity> encoderStream)
        {
            if (!this.session.Start(output, (p, m) => this.Error?.Invoke(p, m), out var state))
            {
                encoderStream = default;

                return false;
            }

            encoderStream = new EncoderStream<TEntity, TState>(session, writer, state);

            return true;
        }
    }
}
