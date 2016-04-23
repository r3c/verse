using System.Collections.Generic;
using System.IO;

using Verse.PrinterDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
{
    class Writer<TEntity> : StringWriter<TEntity, Value, WriterState>
    {
        #region Methods

        public override IWriter<TOther, Value, WriterState> Create<TOther>()
        {
            return new Writer<TOther>();
        }

        public override bool Start(Stream stream, PrinterError onError, out WriterState state)
        {
            state = new WriterState(stream, onError);

            return true;
        }

        public override void Stop(WriterState state)
        {
        }

        public override void WriteArray(IEnumerable<TEntity> items, WriterState state)
        {
            foreach (var item in items)
                this.WriteValue(item, state);
        }

        public override void WriteValue(TEntity source, WriterState state)
        {
            if (source == null)
                return;

            if (this.Array != null)
                this.Array(source, state);
            else if (this.Value != null)
            {
                if (!state.Value(this.Value(source)))
                {
                    state.OnError(state.Position, "failed to write value");
                }
            }
            else
            {
                state.ObjectBegin();

                foreach (var field in this.Fields)
                {
                    if (field.Key.Length > 1 && field.Key[0] == '_')
                        state.Key(field.Key.Substring(1));
                    else
                        state.Key(field.Key);

                    field.Value(source, state);
                }

                state.ObjectEnd();
            }
        }

        #endregion
    }
}
