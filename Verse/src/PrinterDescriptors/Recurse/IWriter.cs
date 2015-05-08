using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.PrinterDescriptors.Recurse
{
    internal interface IWriter<TContext, TNative>
    {
        #region Events

        event PrinterError Error;

        #endregion

        #region Methods

        bool Start(Stream stream, out TContext context);

        void Stop(TContext context);

        void WriteArray<T>(IEnumerable<T> items, Container<T, TContext, TNative> container, TContext context);

        void WriteValue<T>(T source, Container<T, TContext, TNative> container, TContext context);

        #endregion
    }
}