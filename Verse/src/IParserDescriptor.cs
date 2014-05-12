using System;
using System.Collections.Generic;

namespace Verse
{
	public interface IParserDescriptor<T>
	{
		#region Methods

		void					CanCreate<U> (Func<T, U> constructor);

		IParserDescriptor<U>	HasField<U> (string name, ParserAssign<T, U> assign, IParserDescriptor<U> parent);

		IParserDescriptor<U>	HasField<U> (string name, ParserAssign<T, U> assign);

		IParserDescriptor<T>	HasField (string name);

		IParserDescriptor<U>	HasItems<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent);

		IParserDescriptor<U>	HasItems<U> (ParserAssign<T, IEnumerable<U>> assign);

		void					IsValue<U> (ParserAssign<T, U> assign);

		void					IsValue ();

		#endregion
	}
}
