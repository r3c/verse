using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.PrinterDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
    internal class Writer : IWriter<WriterContext, Value>
    {
        #region Events

        public event PrinterError Error
        {
            add
            {
            }
            remove
            {
            }
        }

        #endregion

        #region Attributes

        private readonly JSONSettings settings;

        #endregion

        #region Constructors

        public Writer(JSONSettings settings)
        {
            this.settings = settings;
        }

        #endregion

        #region Methods

        public bool Start(Stream stream, out WriterContext context)
        {
            context = new WriterContext(stream, this.settings);

            return true;
        }

        public void Stop(WriterContext context)
        {
            context.Flush();
        }

        public void WriteArray<TEntity>(IEnumerable<TEntity> items, Container<TEntity, WriterContext, Value> container, WriterContext context)
        {
            IEnumerator<TEntity> item;

            context.ArrayBegin();
            item = items.GetEnumerator();

            while (item.MoveNext())
                this.WriteValue(item.Current, container, context);

            context.ArrayEnd();
        }

        public void WriteValue<TEntity>(TEntity source, Container<TEntity, WriterContext, Value> container, WriterContext context)
        {
            IEnumerator<KeyValuePair<string, Follow<TEntity, WriterContext, Value>>> field;

            if (source == null)
                context.Value(Value.Void);
            else if (container.items != null)
                container.items(source, this, context);
            else if (container.value != null)
                context.Value(container.value(source));
            else
            {
                context.ObjectBegin();
                field = container.fields.GetEnumerator();

                while (field.MoveNext())
                {
                    context.Key(field.Current.Key);

                    field.Current.Value(source, this, context);
                }

                context.ObjectEnd();
            }
        }

        #endregion
    }
}