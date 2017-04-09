using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Recurse.Writers
{
    abstract class LookupWriter<TEntity, TValue, TState, TIndex> : AbstractWriter<TEntity, TValue, TState>
    {
        public readonly Dictionary<TIndex, Enter<TEntity, TState>> Fields = new Dictionary<TIndex, Enter<TEntity, TState>>();

        protected abstract bool TryLookup(string name, out TIndex index);

        public override void DeclareField(string name, Enter<TEntity, TState> enter)
        {
            TIndex index;

            if (!this.TryLookup(name, out index))
                throw new InvalidOperationException("can't declare unknown field '" + name + "'");

            if (this.Fields.ContainsKey(index))
                throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

            this.Fields[index] = enter;
        }
    }
}
