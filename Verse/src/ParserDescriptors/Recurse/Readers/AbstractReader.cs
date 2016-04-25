using System;
using System.IO;
using Verse.ParserDescriptors.Recurse;

namespace Verse.ParserDescriptors.Recurse.Readers
{
    abstract class AbstractReader<TEntity, TValue, TState> : IReader<TEntity, TValue, TState>
    {
        #region Properties

        public bool HoldArray
        {
            get
            {
                return this.array != null;
            }
        }
    
        public bool HoldValue
        {
            get
            {
                return this.value != null;
            }
        }

        #endregion

        #region Attributes

        private Enter<TEntity, TState> array = null;

        private ParserAssign<TEntity, TValue> value = null;

        #endregion

        #region Methods / Abstract

        public abstract IReader<TOther, TValue, TState> Create<TOther>();

        public abstract void DeclareField(string name, Enter<TEntity, TState> enter);

        public abstract IBrowser<TEntity> ReadElements(Func<TEntity> constructor, TState state);

        public abstract bool ReadEntity(ref TEntity target, TState state);

        public abstract bool Start(Stream stream, ParserError onError, out TState state);

        public abstract void Stop(TState state);

        #endregion

        #region Methods / Public

        public void DeclareArray(Enter<TEntity, TState> enter)
        {
            if (this.array != null)
                throw new InvalidOperationException("can't declare array twice on same descriptor");

            this.array = enter;
        }

        public void DeclareValue(ParserAssign<TEntity, TValue> assign)
        {
            if (this.value != null)
                throw new InvalidOperationException("can't declare value twice on same descriptor");

            this.value = assign;
        }

        public bool ProcessArray(ref TEntity entity, TState state)
        {
            if (this.array != null)
                return this.array(ref entity, state);

            return false;
        }

        public void ProcessValue(ref TEntity entity, TValue value)
        {
            if (this.value != null)
                this.value(ref entity, value);
        }

        #endregion
    }
}