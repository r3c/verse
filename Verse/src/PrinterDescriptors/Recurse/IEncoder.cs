using System;

namespace Verse.PrinterDescriptors.Recurse
{
    internal interface IEncoder<TOutput>
    {
        #region Methods

        Converter<TInput, TOutput> Get<TInput>();

        #endregion
    }
}