using System;

namespace Verse.ParserDescriptors.Abstract
{
    interface IDecoderConverter<TFrom>
    {
        #region Methods

        Converter<TFrom, TTo> Get<TTo>();

        #endregion
    }
}