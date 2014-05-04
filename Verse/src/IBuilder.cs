using System.IO;

namespace Verse
{
	public interface IBuilder<T>
	{
		#region Events

		event BuildError	Error;

		#endregion

		#region Methods

		bool Build (T input, Stream output);

		#endregion
	}
}
