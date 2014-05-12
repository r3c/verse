using System;
using System.Collections.Generic;

namespace Verse
{
	public interface IBuilderDescriptor<T>
	{
		#region Methods

		IBuilderDescriptor<U>	HasField<U> (string name, Func<T, U> access, IBuilderDescriptor<U> parent);

		IBuilderDescriptor<U>	HasField<U> (string name, Func<T, U> access);

		IBuilderDescriptor<T>	HasField (string name);

		IBuilderDescriptor<U>	IsArray<U> (Func<T, IEnumerable<U>> access, IBuilderDescriptor<U> parent);

		IBuilderDescriptor<U>	IsArray<U> (Func<T, IEnumerable<U>> access);
/*
		IBuilderDescriptor<U>	IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access, IBuilderDescriptor<U> parent);

		IBuilderDescriptor<U>	IsMap<U> (Func<T, IEnumerable<KeyValuePair<string, U>>> access);
*/
		void					IsValue<U> (Func<T, U> access);

		void					IsValue ();

		#endregion
	}
}
