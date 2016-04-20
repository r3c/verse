using System;
using System.IO;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Readers.String;
using Verse.ParserDescriptors.Recurse.Readers.String.Nodes;

namespace Verse.ParserDescriptors.Recurse.Readers
{
    abstract class StringReader<TEntity, TValue, TState> : IReader<TEntity, TValue, TState>
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

        public BranchNode<TEntity, TValue, TState> RootNode
        {
            get
            {
                return this.rootNode;
            }
        }

        #endregion

        #region Attributes

        private Enter<TEntity, TState> array = null;

        private BranchNode<TEntity, TValue, TState> rootNode = new BranchNode<TEntity, TValue, TState>();

        private ParserAssign<TEntity, TValue> value = null;

        #endregion

        #region Methods / Abstract

        public abstract IReader<TOther, TValue, TState> Create<TOther>();

        public abstract IBrowser<TEntity> ReadArray(Func<TEntity> constructor, TState state);

        public abstract bool ReadValue(ref TEntity target, TState state);

        public abstract bool Start(Stream stream, ParserError onError, out TState state);

        public abstract void Stop(TState state);

        #endregion

        #region Methods / Public

        public void DeclareArray(Enter<TEntity, TState> items)
        {
            if (this.array != null)
                throw new InvalidOperationException("can't declare items twice on a descriptor");

            this.array = items;
        }

        public void DeclareField(string name, Enter<TEntity, TState> enter)
        {
            BranchNode<TEntity, TValue, TState> next = this.rootNode;

            foreach (char character in name)
                next = next.Connect(character);

            if (next.enter != null)
                throw new InvalidOperationException("can't declare same field twice on a descriptor");

            next.enter = enter;
        }

        public void DeclareValue(ParserAssign<TEntity, TValue> value)
        {
            if (this.value != null)
                throw new InvalidOperationException("can't declare value twice on a descriptor");

            this.value = value;
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