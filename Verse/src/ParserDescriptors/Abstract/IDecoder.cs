using System;

namespace Verse.ParserDescriptors.Abstract
{
    internal interface IDecoder<TInput>
    {
        #region Methods

        Converter<TInput, TOutput> Get<TOutput>();

        #endregion
    }
}