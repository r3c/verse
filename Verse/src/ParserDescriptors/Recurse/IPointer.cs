using System;

namespace Verse.ParserDescriptors.Recurse
{
	interface IPointer<T, C, V>
	{
		#region Properties

		bool	CanAssign
		{
			get;
		}

		#endregion

		#region Methods

		void				Assign (ref T target, V value);

		bool				Enter (ref T target, IReader<C, V> reader, C context);

		IPointer<T, C, V>	Follow (char c);

		#endregion
	}
}
