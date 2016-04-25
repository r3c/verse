using System;

namespace Verse.ParserDescriptors.Recurse.Readers.Pattern
{
    interface INode<TEntity, TValue, TState>
    {
        bool IsConnected
        {
            get;
        }

        #region Methods

        void Assign(ref TEntity target, TValue value);

        bool Enter(ref TEntity target, IReader<TEntity, TValue, TState> unknown, TState state);

        INode<TEntity, TValue, TState> Follow(char c);

        #endregion
    }
}