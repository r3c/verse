using System;
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

		void	Write<T> (T source, Pointer<T, C, V> pointer, C context);

		#endregion
	}
}
