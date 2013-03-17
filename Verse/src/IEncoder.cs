using System;
using System.IO;

namespace Verse
{
	public interface IEncoder<T>
	{
		#region Methods
		
		bool	Encode (Stream stream, T instance);
		
		#endregion
	}
}
