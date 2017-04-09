using System;
using System.Collections.Generic;

namespace Verse.EncoderDescriptors.Recurse
{
    abstract class PatternWriter<TEntity, TValue, TState> : AbstractWriter<TEntity, TValue, TState>
    {
        public Dictionary<string, Enter<TEntity, TState>> Fields = new Dictionary<string, Enter<TEntity, TState>>();

        public override void DeclareField(string name, Enter<TEntity, TState> enter)
        {
            if (this.Fields.ContainsKey(name))
                throw new InvalidOperationException("can't declare same field '" + name + "' twice on same descriptor");

            this.Fields[name] = enter;
        }
    }
}
