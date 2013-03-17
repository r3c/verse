using System;

namespace Verse
{
	public delegate void DecoderSubSetter<T, U> (ref T container, Subscript subscript, U value);
}
