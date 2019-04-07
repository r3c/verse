using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Tools;

namespace Verse
{
	/// <summary>
	/// Utility class able to scan any type (using reflection) to automatically
	/// declare its fields to a decoder or encoder descriptor.
	/// </summary>
	public static class Linker
	{
		/// <summary>
		/// Shorthand method to create a decoder from given schema. This is
		/// equivalent to creating a new linker, use it on schema's decoder
		/// descriptor and throw exception whenever error event is fired.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <param name="schema">Entity schema</param>
		/// <returns>Entity decoder</returns>
		public static IDecoder<TEntity> CreateDecoder<TEntity>(ISchema<TEntity> schema)
		{
			if (!Linker.LinkDecoder(schema.DecoderDescriptor, new Dictionary<Type, object>()))
				throw new ArgumentException($"can't link decoder for type '{typeof(TEntity)}'", nameof(schema));

			return schema.CreateDecoder();
		}

		/// <summary>
		/// Shorthand method to create an encoder from given schema. This is
		/// equivalent to creating a new linker, use it on schema's encoder
		/// descriptor and throw exception whenever error event is fired.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <param name="schema">Entity schema</param>
		/// <returns>Entity encoder</returns>
		public static IEncoder<TEntity> CreateEncoder<TEntity>(ISchema<TEntity> schema)
		{
			if (!Linker.LinkEncoder(schema.EncoderDescriptor, new Dictionary<Type, object>()))
				throw new ArgumentException($"can't link encoder for type '{typeof(TEntity)}'", nameof(schema));

			return schema.CreateEncoder();
		}

		private static bool LinkDecoder<T>(IDecoderDescriptor<T> descriptor, Dictionary<Type, object> parents)
		{
			try
			{
				descriptor.IsValue();

				return true;
			}
			catch (InvalidCastException) // FIXME: hack
			{
			}

			var type = typeof(T);

			parents[type] = descriptor;

			// Target type is an array
			if (type.IsArray)
			{
				var element = type.GetElementType();
				var assign = Generator.CreateArraySetter(element);

				return Linker.LinkDecoderArray(descriptor, element, assign, parents);
			}

			// Target type implements IEnumerable<> interface
			foreach (var iface in type.GetInterfaces())
			{
				// Found interface, inner elements type is "argument"
				if (!Resolver.HasSameGenericDefinitionThan<IEnumerable<object>>(iface, out var itemType))
					continue;

				// Search constructor compatible with IEnumerable<>
				foreach (ConstructorInfo constructor in type.GetConstructors())
				{
					var parameters = constructor.GetParameters();

					if (parameters.Length != 1 || !Resolver.HasSameGenericDefinitionThan<IEnumerable<object>>(parameters[0].ParameterType, out var argument) || argument != itemType)
						continue;

					var assign = Generator.CreateConstructorSetter(constructor);

					return Linker.LinkDecoderArray(descriptor, itemType, assign, parents);
				}
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var setter = Generator.CreatePropertySetter(property);

				if (!Linker.LinkDecoderField(descriptor, property.PropertyType, property.Name, setter, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var setter = Generator.CreateFieldSetter(field);

				if (!Linker.LinkDecoderField(descriptor, field.FieldType, field.Name, setter, parents))
					return false;
			}

			return true;
		}

		private static bool LinkDecoderArray<TEntity>(IDecoderDescriptor<TEntity> descriptor, Type type, object setter,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				var hasKnownItems = Resolver.GetMethod<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>,
					IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p));

				Resolver
					.ChangeGenericMethodArguments(hasKnownItems, type)
					.Invoke(descriptor, new[] { setter, recurse });

				return true;
			}

			var hasItems = Resolver.GetMethod<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>,
				IDecoderDescriptor<object>>>((d, a) => d.HasItems(a));

			var itemDescriptor = Resolver
				.ChangeGenericMethodArguments(hasItems, type)
				.Invoke(descriptor, new[] { setter });

			var linkItems = Resolver.GetMethod<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
				(d, p) => Linker.LinkDecoder(d, p));

			var result = Resolver
				.ChangeGenericMethodArguments(linkItems, type)
				.Invoke(null, new object[] { itemDescriptor, parents });

			return result is bool success && success;
		}

		private static bool LinkDecoderField<TEntity>(IDecoderDescriptor<TEntity> descriptor, Type type, string name,
			object setter, IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				var hasKnownField = Resolver.GetMethod<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>,
					IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p));

				Resolver
					.ChangeGenericMethodArguments(hasKnownField, type)
					.Invoke(descriptor, new object[] { name, setter, recurse });

				return true;
			}

			var hasField = Resolver
				.GetMethod<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>,
					IDecoderDescriptor<object>>>((d, n, a) => d.HasField(n, a));

			var fieldDescriptor = Resolver
				.ChangeGenericMethodArguments(hasField, type)
				.Invoke(descriptor, new object[] { name, setter });

			var linkField = Resolver.GetMethod<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
				(d, p) => Linker.LinkDecoder(d, p));

			var result = Resolver
				.ChangeGenericMethodArguments(linkField, type)
				.Invoke(null, new object[] { fieldDescriptor, parents });

			return result is bool success && success;
		}

		private static bool LinkEncoder<T>(IEncoderDescriptor<T> descriptor, Dictionary<Type, object> parents)
		{
			try
			{
				descriptor.IsValue();

				return true;
			}
			catch (InvalidCastException) // FIXME: hack
			{
			}

			var type = typeof(T);

			parents[type] = descriptor;

			// Target type is an array
			if (type.IsArray)
			{
				var element = type.GetElementType();
				var getter = Generator.CreateIdentity(typeof(IEnumerable<>).MakeGenericType(element));

				return Linker.LinkEncoderArray(descriptor, element, getter, parents);
			}

			// Target type implements IEnumerable<> interface
			foreach (Type iface in type.GetInterfaces())
			{
				if (!Resolver.HasSameGenericDefinitionThan<IEnumerable<object>>(iface, out var argument))
					continue;

				var getter = Generator.CreateIdentity(typeof(IEnumerable<>).MakeGenericType(argument));

				return Linker.LinkEncoderArray(descriptor, argument, getter, parents);
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var getter = Generator.CreatePropertyGetter(property);

				if (!Linker.LinkEncoderField(descriptor, property.PropertyType, property.Name, getter, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var getter = Generator.CreateFieldGetter(field);

				if (!Linker.LinkEncoderField(descriptor, field.FieldType, field.Name, getter, parents))
					return false;
			}

			return true;
		}

		private static bool LinkEncoderArray<T>(IEncoderDescriptor<T> descriptor, Type type, object getter,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				var hasKnownItems = Resolver.GetMethod<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>,
					IEncoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p));

				Resolver
					.ChangeGenericMethodArguments(hasKnownItems, type)
					.Invoke(descriptor, new[] { getter, recurse });

				return true;
			}

			var hasItems = Resolver.GetMethod<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>>>(
				(d, a) => d.HasItems(a));

			var itemDescriptor = Resolver
				.ChangeGenericMethodArguments(hasItems, type)
				.Invoke(descriptor, new[] { getter });

			var linkItems = Resolver.GetMethod<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>(
				(d, p) => Linker.LinkEncoder(d, p));

			var result = Resolver
				.ChangeGenericMethodArguments(linkItems, type)
				.Invoke(null, new object[] { itemDescriptor, parents });

			return result is bool success && success;
		}

		private static bool LinkEncoderField<T>(IEncoderDescriptor<T> descriptor, Type type, string name, object getter,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				var hasKnownField = Resolver.GetMethod<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>,
					IEncoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p));

				Resolver
					.ChangeGenericMethodArguments(hasKnownField, type)
					.Invoke(descriptor, new object[] { name, getter, recurse });

				return true;
			}

			var hasField = Resolver.GetMethod<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>>>(
				(d, n, a) => d.HasField(n, a));

			var fieldDescriptor = Resolver
				.ChangeGenericMethodArguments(hasField, type)
				.Invoke(descriptor, new object[] { name, getter });

			var linkField = Resolver.GetMethod<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>(
				(d, p) => Linker.LinkEncoder(d, p));

			var result = Resolver
				.ChangeGenericMethodArguments(linkField, type)
				.Invoke(null, new object[] { fieldDescriptor, parents });

			return result is bool success && success;
		}
	}
}