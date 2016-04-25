using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Verse.PrinterDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
    class Writer<TEntity> : PatternWriter<TEntity, Value, WriterState>
    {
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

        public override IWriter<TOther, Value, WriterState> Create<TOther>()
        {
            return new Writer<TOther>(this.settings);
        }

        public override bool Start(Stream stream, PrinterError onError, out WriterState state)
        {
            state = new WriterState(stream, onError, this.settings);

            return true;
        }

        public override void Stop(WriterState state)
        {
            state.Flush();
        }

        public override void WriteElements(IEnumerable<TEntity> elements, WriterState state)
        {
            IEnumerator<TEntity> item;

            state.ArrayBegin();
            item = elements.GetEnumerator();

            while (item.MoveNext())
                this.WriteEntity(item.Current, state);

            state.ArrayEnd();
        }

        public override void WriteEntity(TEntity source, WriterState state)
        {
            IEnumerator<KeyValuePair<string, Enter<TEntity, WriterState>>> field;

            if (source == null)
                state.Value(JSON.Value.Void);
            else if (this.Array != null)
                this.Array(source, state);
            else if (this.Value != null)
                state.Value(this.Value(source));
            else
            {
                state.ObjectBegin();
                field = this.Fields.GetEnumerator();

                while (field.MoveNext())
                {
                    state.Key(field.Current.Key);

                    field.Current.Value(source, state);
                }

                state.ObjectEnd();
            }
        }

        #endregion
    }
}