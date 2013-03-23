using System;
using System.Collections.Generic;

namespace Verse
{
	public delegate IEnumerable<KeyValuePair<string, U>> EncoderMapGetter<T, U> (T container);
}
