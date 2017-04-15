using System;
using System.IO;

namespace Verse.DecoderDescriptors.Recurse
{
    class Decoder<TEntity, TValue, TState> : IDecoder<TEntity>
    {
        #region Events

        public event DecodeError Error;

        #endregion

        #region Attributes

        private readonly IReader<TEntity, TValue, TState> reader;

        #endregion

        #region Constructors

        public Decoder(IReader<TEntity, TValue, TState> reader)
        {
            this.reader = reader;
        }

        #endregion

        #region Methods / Public

        public bool Decode(Stream input, ref TEntity output)
        {
            TState state;

            if (!this.reader.Start(input, this.OnError, out state))
                return false;

            try
            {
                return this.reader.ReadEntity(ref output, state);
            }
            finally
            {
                this.reader.Stop(state);
            }
        }

        #endregion

        #region Methods / Private

        private void OnError(int position, string message)
        {
            DecodeError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        #endregion
    }
}