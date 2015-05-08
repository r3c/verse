using System;
using System.IO;

namespace Verse.PrinterDescriptors.Recurse
{
    internal class Printer<TEntity, TContext, TNative> : IPrinter<TEntity>
    {
        #region Events

        public event PrinterError Error;

        #endregion

        #region Attributes

        private readonly Container<TEntity, TContext, TNative> container;

        private readonly IWriter<TContext, TNative> writer;

        #endregion

        #region Constructors

        public Printer(Container<TEntity, TContext, TNative> container, IWriter<TContext, TNative> writer)
        {
            writer.Error += this.OnError;

            this.container = container;
            this.writer = writer;
        }

        #endregion

        #region Methods / Public

        public bool Print(TEntity input, Stream output)
        {
            TContext context;

            if (!this.writer.Start(output, out context))
                return false;

            try
            {
                this.writer.WriteValue(input, this.container, context);
            }
            finally
            {
                this.writer.Stop(context);
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