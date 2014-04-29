using System.IO;

namespace Verse
{
	public interface IWriter<T>
	{
		#region Events

		event WriteError	Error;

		#endregion

        #region Methods

        bool Write (T input, Stream output);

        #endregion
	}
}
