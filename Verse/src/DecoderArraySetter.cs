using System;
using System.Collections.Generic;

namespace Verse
{
	public delegate void DecoderArraySetter<T, U> (ref T container, ICollection<U> array);
}
