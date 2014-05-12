using System;
using System.Collections.Generic;
using System.Reflection;

namespace Verse.Tools
{
	public static class Linker
	{
#if false
		public static bool Link<T> (Dictionary<Type, object> descriptors, IParserDescriptor<T> descriptor)
		{
			Type[]		arguments;
			Type		container;
			Type		element;
			TypeFilter	filter;

			if (this.TryLink ())
				return;

			container = typeof (T);

			descriptors = new Dictionary<Type, object> (descriptors);
			descriptors[container] = descriptor;

			// Check is container type is or implements ICollection<> interface
			if (container.IsGenericType && container.GetGenericTypeDefinition () == typeof (IEnumerable<>))
			{
				arguments = container.GetGenericArguments ();
				element = arguments.Length == 1 ? arguments[0] : null;
			}
			else
			{
				filter = new TypeFilter ((t, c) => t.IsGenericType && t.GetGenericTypeDefinition () == typeof (IEnumerable<>));
				element = null;

				foreach (Type contract in container.FindInterfaces (filter, null))
				{
					arguments = contract.GetGenericArguments ();

					if (arguments.Length == 1)
					{
						element = arguments[0];

						break;
					}
				}
			}

			// Container is an IElement<> of element type "element"
			if (element != null)
			{
				this.LinkElements (decoders, element, AbstractDecoder<T>.MakeArraySetter (container, element));

				return;
			}

			// Browse public readable and writable properties
			foreach (PropertyInfo property in container.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || (property.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
					continue;

				this.LinkAttribute (decoders, property.PropertyType, property.Name, AbstractDecoder<T>.MakeValueSetter (property));
			}

			// Browse public fields
			foreach (FieldInfo field in container.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if ((field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName)
					continue;

				this.LinkAttribute (decoders, field.FieldType, field.Name, AbstractDecoder<T>.MakeValueSetter (field));
			}
		}
#endif
	}
}
