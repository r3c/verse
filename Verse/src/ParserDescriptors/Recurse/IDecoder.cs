using System;

namespace Verse.ParserDescriptors.Recurse
{
    internal interface IDecoder<TInput>
    {
        #region Methods

        Converter<TInput, TOutput> Get<TOutput>();

        #endregion
    }
}