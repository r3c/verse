using System;

namespace Verse.Test
{
	static class Identity<T>
	{
		public static readonly Func<T, T> Access = value => value;
	}
}
