using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.EncoderDescriptors.Recurse
{
    abstract class AbstractWriter<TEntity, TValue, TState> : IWriter<TEntity, TValue, TState>
    {
        #region Attributes

        public Enter<TEntity, TState> Array = null;

        public Func<TEntity, TValue> Value = null;

        #endregion

        #region Methods / Abstract

        public abstract IWriter<TOther, TValue, TState> Create<TOther>();

        public abstract void DeclareField(string name, Enter<TEntity, TState> enter);

        public abstract bool Start(Stream stream, EncodeError error, out TState state);

        public abstract void Stop(TState state);

        public abstract void WriteElements(IEnumerable<TEntity> elements, TState state);

        public abstract void WriteEntity(TEntity source, TState state);

        #endregion

        #region Methods / Public

        public void DeclareArray(Enter<TEntity, TState> enter)
        {
            if (this.Array != null)
                throw new InvalidOperationException("can't declare array twice on same descriptor");

            this.Array = enter;            
        }

        public void DeclareValue(Func<TEntity, TValue> access)
        {
            if (this.Value != null)
                throw new InvalidOperationException("can't declare value twice on same descriptor");

            this.Value = access;
        }

        #endregion
    }
}
