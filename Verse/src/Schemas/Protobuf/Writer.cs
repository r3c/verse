using System.Collections.Generic;
using System.IO;

using Verse.PrinterDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
{
    internal class Writer : IWriter<WriterContext, Value>
    {
        #region Events

        public event PrinterError Error;

        #endregion

        #region Methods

        public bool Start(Stream stream, out WriterContext context)
        {
            context = new WriterContext(stream);

            return true;
        }

        public void Stop(WriterContext context)
        {
        }

        public void WriteArray<TEntity>(IEnumerable<TEntity> items, Container<TEntity, WriterContext, Value> container, WriterContext context)
        {
            foreach (var item in items)
                this.WriteValue(item, container, context);
        }

        public void WriteValue<TEntity>(TEntity source, Container<TEntity, WriterContext, Value> container, WriterContext context)
        {
            if (container.items != null)
                container.items(source, this, context);
            else if (container.value != null)
            {
                if (!context.Value(container.value(source)))
                {
                    this.OnError(context.Position, "failed to write value");
                }
            }
            else
            {
                context.ObjectBegin();

                foreach (var field in container.fields)
                {
                    if (field.Key.Length > 1 && field.Key[0] == '_')
                        context.Key(field.Key.Substring(1));
                    else
                        context.Key(field.Key);

                    field.Value(source, this, context);
                }

                context.ObjectEnd();
            }
        }

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
