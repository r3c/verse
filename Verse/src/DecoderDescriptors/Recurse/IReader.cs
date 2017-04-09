using System;
using System.IO;

namespace Verse.DecoderDescriptors.Recurse
{
    interface IReader<TEntity, TValue, TState>
    {
        #region Properties

        bool HoldArray
        {
            get;
        }

        bool HoldValue
        {
            get;
        }

        #endregion

        #region Methods

        IReader<TOther, TValue, TState> Create<TOther>();

        void DeclareArray(Enter<TEntity, TState> enter);

        void DeclareField(string name, Enter<TEntity, TState> enter);

        void DeclareValue(DecodeAssign<TEntity, TValue> assign);

        bool ProcessArray(ref TEntity entity, TState state);

        void ProcessValue(ref TEntity entity, TValue value);

        IBrowser<TEntity> ReadElements(Func<TEntity> constructor, TState state);

        bool ReadEntity(ref TEntity target, TState state);

        bool Start(Stream stream, DecodeError error, out TState state);

        void Stop(TState state);

        #endregion
    }
}
