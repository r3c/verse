using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
	interface IReader<C, V>
	{
		#region Events

		event ParserError Error;

		#endregion

		#region Methods

		IBrowser<T> ReadArray<T> (Func<T> constructor, Container<T, C, V> container, C context);

		bool ReadValue<T> (ref T target, Container<T, C, V> container, C context);

		bool Start (Stream stream, out C context);

		void Stop (C context);

		#endregion
	}
}
