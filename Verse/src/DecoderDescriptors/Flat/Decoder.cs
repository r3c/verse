using System.IO;

namespace Verse.DecoderDescriptors.Flat
{
    internal class Decoder<TEntity, TContext, TNative> : IDecoder<TEntity>
    {
        #region Events

        public event DecodeError Error;

        #endregion

        #region Attributes

        private readonly Container<TEntity, TContext, TNative> container;

        private readonly IReader<TContext, TNative> reader;

        #endregion

        #region Constructors

        public Decoder(Container<TEntity, TContext, TNative> container, IReader<TContext, TNative> reader)
        {
            reader.Error += this.OnError;

            this.container = container;
            this.reader = reader;
        }

        #endregion

        #region Methods / Public

        public bool Decode(Stream input, ref TEntity output)
        {
            TContext context;

            if (!this.reader.Start(input, out context))
                return false;

            try
            {
                return this.reader.Read(ref output, this.container, context);
            }
            finally
            {
                this.reader.Stop(context);
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