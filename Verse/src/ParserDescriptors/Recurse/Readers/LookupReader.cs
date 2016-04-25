using System;
using System.Collections.Generic;

namespace Verse.ParserDescriptors.Recurse.Readers
{
    abstract class LookupReader<TEntity, TValue, TState, TIndex> : AbstractReader<TEntity, TValue, TState>
    {
        private readonly Dictionary<TIndex, Enter<TEntity, TState>> fields = new Dictionary<TIndex, Enter<TEntity, TState>>();

        protected abstract bool TryLookup(string name, out TIndex index);

        public override void DeclareField(string name, Enter<TEntity, TState> enter)
        {
            TIndex index;

            if (!this.TryLookup(name, out index))
                throw new InvalidOperationException("can't declare unknown field '" + name + "'");

            if (this.fields.ContainsKey(index))
                throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

            this.fields[index] = enter;
        }
    }
}
