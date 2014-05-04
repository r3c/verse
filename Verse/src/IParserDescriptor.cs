using System;

namespace Verse
{
	public interface IParserDescriptor<T>
	{
		#region Methods

		void					CanCreate<U> (DescriptorGet<T, U> constructor);

		IParserDescriptor<U>	HasField<U> (string name, DescriptorSet<T, U> assign, IParserDescriptor<U> recurse);

		IParserDescriptor<U>	HasField<U> (string name, DescriptorSet<T, U> assign);

		IParserDescriptor<T>	HasField (string name);

		IParserDescriptor<U>	HasItems<U> (DescriptorSet<T, U> append, IParserDescriptor<U> recurse); // FIXME: Descriptor<T, IEnumerable<U>> assign?

		IParserDescriptor<U>	HasItems<U> (DescriptorSet<T, U> append); // FIXME: Descriptor<T, IEnumerable<U>> assign?

		IParserDescriptor<T>	HasItems (); // FIXME: delete?

		void					IsValue<U> (DescriptorSet<T, U> assign);

		void					IsValue ();

		#endregion
	}
}
