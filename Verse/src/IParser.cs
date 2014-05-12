using System.IO;

namespace Verse
{
	public interface IParser<T>
	{
		#region Events

		event ParserError	Error;

		#endregion

		#region Methods

		bool Parse (Stream input, out T output);

		#endregion
	}
}
