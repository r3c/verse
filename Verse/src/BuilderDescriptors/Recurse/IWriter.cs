using System;
using System.Collections.Generic;
using System.IO;

namespace Verse.BuilderDescriptors.Recurse
{
	public interface IWriter<C, V>
	{
		#region Events

		event BuildError	Error;

		#endregion

		#region Methods

		bool	Start (Stream stream, out C context);

		void	Stop (C context);

		void	WriteItems<T> (IEnumerable<T> items, IPointer<T, C, V> pointer, C context);

		void	WriteKey<T> (T source, string name, IPointer<T, C, V> pointer, C context);

		void	WriteValue (V value, C context); 

		#endregion
	}
}
