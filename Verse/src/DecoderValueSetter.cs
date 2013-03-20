using System;

namespace Verse
{
	public delegate void DecoderValueSetter<T, U> (ref T container, U value);
}
