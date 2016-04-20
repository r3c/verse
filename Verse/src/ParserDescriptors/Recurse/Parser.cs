using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
    class Parser<TEntity, TValue, TState> : IParser<TEntity>
    {
        #region Events

        public event ParserError Error;

        #endregion

        #region Attributes

        private readonly IReader<TEntity, TValue, TState> reader;

        #endregion

        #region Constructors

        public Parser(IReader<TEntity, TValue, TState> reader)
        {
            this.reader = reader;
        }

        #endregion

        #region Methods / Public

        public bool Parse(Stream input, ref TEntity output)
        {
            TState state;

            if (!this.reader.Start(input, this.OnError, out state))
                return false;

            try
            {
                return this.reader.ReadValue(ref output, state);
            }
            finally
            {
                this.reader.Stop(state);
            }
        }

        #endregion

        #region Methods / Private

        private void OnError(int position, string message)
        {
            ParserError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        #endregion
    }
}