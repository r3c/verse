using System;
using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Recurse
{
	interface IBrowser<T> : IEnumerator<T>
	{
		bool Complete();
	}
}