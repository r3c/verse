using System;

namespace Verse.Test
{
	static class Identity<T>
	{
		public static readonly Func<T, T> Access = value => value;

		public static readonly DecodeAssign<T, T> Assign = (ref T parent, T value) => parent = value;
	}
}
