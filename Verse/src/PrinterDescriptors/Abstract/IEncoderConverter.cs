using System;

namespace Verse.PrinterDescriptors.Abstract
{
    interface IEncoderConverter<TTo>
    {
        #region Methods

        Converter<TFrom, TTo> Get<TFrom>();

        #endregion
    }
}