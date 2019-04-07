using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Resolvers;
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

		private static DecodeAssign<T[], IEnumerable<T>> CreateArraySetter<T>()
		{
			return (ref T[] target, IEnumerable<T> value) => target = value.ToArray();
		}

		private static Func<T, T> CreateIdentity<T>()
		{
			return v => v;
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
				var setter = MethodResolver
					.Create<Func<DecodeAssign<object[], object[]>>>(() => Linker.CreateArraySetter<object>())
					.SetGenericArguments(element)
					.Invoke(null);

				return Linker.LinkDecoderArray(descriptor, element, setter, parents);
			}

			// Target type implements IEnumerable<> interface
			foreach (var interfaceType in type.GetInterfaces())
			{
				// Found interface, inner elements type is "argument"
				if (!TypeResolver.Create(interfaceType).HasSameDefinitionThan<IEnumerable<object>>(out var interfaceTypeArguments))
					continue;

				var elementType = interfaceTypeArguments[0];

				// Search constructor compatible with IEnumerable<>
				foreach (ConstructorInfo constructor in type.GetConstructors())
				{
					var parameters = constructor.GetParameters();

					if (parameters.Length != 1)
						continue;

					var parameterType = parameters[0].ParameterType;

					if (!TypeResolver.Create(parameterType).HasSameDefinitionThan<IEnumerable<object>>(out var parameterArguments) || parameterArguments[0] != elementType)
						continue;

					var setter = Generator.CreateConstructorSetter(constructor);

					return Linker.LinkDecoderArray(descriptor, elementType, setter, parents);
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
				MethodResolver
					.Create<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>, IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, setter, recurse);

				return true;
			}

			var itemDescriptor = MethodResolver
				.Create<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>, IDecoderDescriptor<object>>>((d, a) => d.HasItems(a))
				.SetGenericArguments(type)
				.Invoke(descriptor, setter);

			var result = MethodResolver
				.Create<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(IDecoderDescriptor<object> d, Dictionary<Type, object> p) => Linker.LinkDecoder(d, p))
				.SetGenericArguments(type)
				.Invoke(null, itemDescriptor, parents);

			return result is bool success && success;
		}

		private static bool LinkDecoderField<TEntity>(IDecoderDescriptor<TEntity> descriptor, Type type, string name,
			object setter, IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				MethodResolver
					.Create<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>, IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, name, setter, recurse);

				return true;
			}

			var fieldDescriptor = MethodResolver
				.Create<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>, IDecoderDescriptor<object>>>((d, n, a) => d.HasField(n, a))
				.SetGenericArguments(type)
				.Invoke(descriptor, name, setter);

			var result = MethodResolver
				.Create<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(IDecoderDescriptor<object> d, Dictionary<Type, object> p) => Linker.LinkDecoder(d, p))
				.SetGenericArguments(type)
				.Invoke(null, fieldDescriptor, parents);

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
				var getter = MethodResolver
					.Create<Func<Func<object, object>>>(() => Linker.CreateIdentity<object>())
					.SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(element))
					.Invoke(null);

				return Linker.LinkEncoderArray(descriptor, element, getter, parents);
			}

			// Target type implements IEnumerable<> interface
			foreach (Type interfaceType in type.GetInterfaces())
			{
				if (!TypeResolver.Create(interfaceType).HasSameDefinitionThan<IEnumerable<object>>(out var arguments))
					continue;

				var elementType = arguments[0];
				var getter = MethodResolver
					.Create<Func<Func<object, object>>>(() => Linker.CreateIdentity<object>())
					.SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(elementType))
					.Invoke(null);

				return Linker.LinkEncoderArray(descriptor, elementType, getter, parents);
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
				MethodResolver
					.Create<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>, IEncoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, getter, recurse);

				return true;
			}

			var itemDescriptor = MethodResolver
				.Create<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>>>((d, a) => d.HasItems(a))
				.SetGenericArguments(type)
				.Invoke(descriptor, getter);

			var result = MethodResolver
				.Create<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>((d, p) => Linker.LinkEncoder(d, p))
				.SetGenericArguments(type)
				.Invoke(null, itemDescriptor, parents);

			return result is bool success && success;
		}

		private static bool LinkEncoderField<T>(IEncoderDescriptor<T> descriptor, Type type, string name, object getter,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				MethodResolver
					.Create<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>, IEncoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, name, getter, recurse);

				return true;
			}

			var fieldDescriptor = MethodResolver
				.Create<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>>>((d, n, a) => d.HasField(n, a))
				.SetGenericArguments(type)
				.Invoke(descriptor, name, getter);

			var result = MethodResolver
				.Create<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(IEncoderDescriptor<object> d, Dictionary<Type, object> p) => Linker.LinkEncoder(d, p))
				.SetGenericArguments(type)
				.Invoke(null, fieldDescriptor, parents);

			return result is bool success && success;
		}
	}
}