using System;
using System.Collections.Generic;

namespace Verse
{
	public delegate U EncoderValueGetter<T, U> (T container);
}
