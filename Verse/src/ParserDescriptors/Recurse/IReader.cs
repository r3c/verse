using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
	public interface IReader<C, V>
	{
		#region Events

		event ParseError	Error;

		#endregion

		#region Methods

		bool	Read<T> (ref T target, IPointer<T, C, V> pointer, C context);

		bool	Start (Stream stream, out C context);

		void	Stop (C context);

		#endregion
	}
}
