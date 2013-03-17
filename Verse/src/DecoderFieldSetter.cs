using System;

namespace Verse
{
	public delegate void DecoderFieldSetter<T, U> (ref T container, U value);
}
