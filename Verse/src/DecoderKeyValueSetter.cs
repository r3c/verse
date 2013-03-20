using System;

namespace Verse
{
	public delegate void DecoderKeyValueSetter<T, U> (ref T container, string key, U value);
}
