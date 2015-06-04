using System;

namespace Verse.ParserDescriptors.Flat
{
    internal interface IDecoder<in TInput>
    {
        #region Methods

        Converter<TInput, TOutput> Get<TOutput>();

        #endregion
    }
}