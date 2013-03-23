using System;
using System.Collections.Generic;

namespace Verse
{
	public delegate IEnumerable<U> EncoderArrayGetter<T, U> (T container);
}
