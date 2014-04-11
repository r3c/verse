using System;
using System.IO;

namespace Verse.ParserDescriptors.Recurse
{
    public interface IReader<C, V>
    {
		#region Methods

        bool	Begin (Stream stream, out C context);

        void	End (C context);

        bool	Read<T> (ref T target, IPointer<T, C, V> pointer, C context);

        #endregion
    }
}
