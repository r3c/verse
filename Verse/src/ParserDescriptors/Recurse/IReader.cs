using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
    internal interface IReader<TContext, TNative>
    {
        #region Events

        event ParserError Error;

        #endregion

        #region Methods

        IBrowser<TEntity> ReadArray<TEntity>(Func<TEntity> constructor, Container<TEntity, TContext, TNative> container, TContext context);

        bool ReadValue<TEntity>(ref TEntity target, Container<TEntity, TContext, TNative> container, TContext context);

        bool Start(Stream stream, out TContext context);

        void Stop(TContext context);

        #endregion
    }
}