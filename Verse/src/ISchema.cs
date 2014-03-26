using System;

namespace Verse
{
    public interface ISchema<T> : IParserDescriptor<T>
    {
        #region Methods

        IParser<T>	GetParser (Func<T> constructor);

        IParser<T>	GetParser ();

        #endregion
    }
}
