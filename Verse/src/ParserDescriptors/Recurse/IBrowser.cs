using System;
using System.Collections.Generic;

namespace Verse.ParserDescriptors.Recurse
{
	interface IBrowser<T> : IEnumerator<T>
	{
		bool Success
		{
			get;
		}
	}
}
