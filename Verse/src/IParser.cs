using System.IO;

namespace Verse
{
    public interface IParser<T>
    {
        #region Methods

        bool Parse (Stream input, out T output);

        #endregion
    }
}
