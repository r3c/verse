using System.IO;

namespace Verse
{
	public interface IBuilder<T>
	{
        #region Methods

        bool Build (T input, Stream output);

        #endregion
	}
}
