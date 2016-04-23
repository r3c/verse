using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.PrinterDescriptors.Recurse
{
    abstract class StringWriter<TEntity, TValue, TState> : IWriter<TEntity, TValue, TState>
    {
        #region Attributes

        public Enter<TEntity, TState> Array = null;

        public Dictionary<string, Enter<TEntity, TState>> Fields = new Dictionary<string, Enter<TEntity, TState>>();

        public Func<TEntity, TValue> Value = null;

        #endregion

        #region Methods / Abstract

        public abstract IWriter<TOther, TValue, TState> Create<TOther>();

        public abstract bool Start(Stream stream, PrinterError onError, out TState state);

        public abstract void Stop(TState state);

        public abstract void WriteArray(IEnumerable<TEntity> items, TState state);

        public abstract void WriteValue(TEntity source, TState state);

        #endregion

        #region Methods / Public

        public void DeclareArray(Enter<TEntity, TState> enter)
        {
            if (this.Array != null)
                throw new InvalidOperationException("can't declare array twice on same descriptor");

            this.Array = enter;            
        }

        public void DeclareField(string name, Enter<TEntity, TState> enter)
        {
            if (this.Fields.ContainsKey(name))
                throw new InvalidOperationException("can't declare same field twice on a descriptor");

            this.Fields[name] = enter;
        }

        public void DeclareValue(Func<TEntity, TValue> access)
        {
            if (this.Value != null)
                throw new InvalidOperationException("can't declare value twice on a descriptor");

            this.Value = access;
        }

        #endregion
    }
}
