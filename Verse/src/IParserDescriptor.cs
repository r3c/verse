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

		IParserDescriptor<U>	IsArray<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent);

		IParserDescriptor<U>	IsArray<U> (ParserAssign<T, IEnumerable<U>> assign);
/*
		IParserDescriptor<U>	IsMap<U> (ParserAssign<T, IEnumerable<KeyValuePair<string, U>>> assign, IParserDescriptor<U> parent);

		IParserDescriptor<U>	IsMap<U> (ParserAssign<T, IEnumerable<KeyValuePair<string, U>>> assign);
*/
		void					IsValue<U> (ParserAssign<T, U> assign);

		void					IsValue ();

		#endregion
	}
}
