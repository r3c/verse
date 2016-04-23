using System;
using System.IO;

namespace Verse.PrinterDescriptors.Recurse
{
    class Printer<TEntity, TValue, TState> : IPrinter<TEntity>
    {
        #region Events

        public event PrinterError Error;

        #endregion

        #region Attributes

        private readonly IWriter<TEntity, TValue, TState> writer;

        #endregion

        #region Constructors

        public Printer(IWriter<TEntity, TValue, TState> writer)
        {
            this.writer = writer;
        }

        #endregion

        #region Methods / Public

        public bool Print(TEntity input, Stream output)
        {
            TState state;

            if (!this.writer.Start(output, this.OnError, out state))
                return false;

            try
            {
                this.writer.WriteValue(input, state);
            }
            finally
            {
                this.writer.Stop(state);
            }

            return true;
        }

        #endregion

        #region Methods / Private

        private void OnError(int position, string message)
        {
            PrinterError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        #endregion
    }
}