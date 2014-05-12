using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.BuilderDescriptors.Recurse
{
	interface IWriter<C, V>
	{
		#region Events

		event BuilderError	Error;

		#endregion

		#region Methods

		bool	Start (Stream stream, out C context);

		void	Stop (C context);

		void	Write<T> (T source, Container<T, C, V> container, C context);

		void	WriteItems<T> (IEnumerable<T> items, Container<T, C, V> container, C context);

		#endregion
	}
}
