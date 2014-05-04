using System;
using System.Collections.Generic;

namespace Verse
{
	public interface IBuilderDescriptor<T>
	{
		#region Methods

		IBuilderDescriptor<U>	HasField<U> (string name, Func<T, U> access, IBuilderDescriptor<U> recurse);

		IBuilderDescriptor<U>	HasField<U> (string name, Func<T, U> access);

		IBuilderDescriptor<T>	HasField (string name);

		IBuilderDescriptor<U>	HasItems<U> (Func<T, IEnumerable<U>> access, IBuilderDescriptor<U> recurse);

		IBuilderDescriptor<U>	HasItems<U> (Func<T, IEnumerable<U>> access);

		void					IsValue<U> (Func<T, U> access);

		void					IsValue ();

		#endregion
	}
}
