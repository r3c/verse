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
		/// <param name="bindings">Binding flags used to filter bound fields and properties</param>
		/// <returns>Entity decoder</returns>
		public static IDecoder<TEntity> CreateDecoder<TEntity>(ISchema<TEntity> schema, BindingFlags bindings)
		{
			if (!Linker.LinkDecoder(schema.DecoderDescriptor, bindings, new Dictionary<Type, object>()))
				throw new ArgumentException($"can't link decoder for type '{typeof(TEntity)}'", nameof(schema));

			return schema.CreateDecoder();
		}

		public static IDecoder<TEntity> CreateDecoder<TEntity>(ISchema<TEntity> schema)
		{
			return CreateDecoder(schema, BindingFlags.Instance | BindingFlags.Public);
		}

		/// <summary>
		/// Shorthand method to create an encoder from given schema. This is
		/// equivalent to creating a new linker, use it on schema's encoder
		/// descriptor and throw exception whenever error event is fired.
		/// </summary>
		/// <typeparam name="TEntity">Entity type</typeparam>
		/// <param name="schema">Entity schema</param>
		/// <param name="bindings">Binding flags used to filter bound fields and properties</param>
		/// <returns>Entity encoder</returns>
		public static IEncoder<TEntity> CreateEncoder<TEntity>(ISchema<TEntity> schema, BindingFlags bindings)
		{
			if (!Linker.LinkEncoder(schema.EncoderDescriptor, bindings, new Dictionary<Type, object>()))
				throw new ArgumentException($"can't link encoder for type '{typeof(TEntity)}'", nameof(schema));

			return schema.CreateEncoder();
		}

		public static IEncoder<TEntity> CreateEncoder<TEntity>(ISchema<TEntity> schema)
		{
			return CreateEncoder(schema, BindingFlags.Instance | BindingFlags.Public);
		}

		private static DecodeAssign<T[], IEnumerable<T>> CreateArraySetter<T>()
		{
			return (ref T[] target, IEnumerable<T> value) => target = value.ToArray();
		}

		private static Func<T, T> CreateIdentity<T>()
		{
			return v => v;
		}

		private static bool LinkDecoder<T>(IDecoderDescriptor<T> descriptor, BindingFlags bindings, Dictionary<Type, object> parents)
		{
			var type = typeof(T);

			parents[type] = descriptor;

			if (TryLinkDecoderAsValue(descriptor))
				return true;

			// Bind descriptor as an array of target type is also array
			if (type.IsArray)
			{
				var element = type.GetElementType();
				var setter = MethodResolver
					.Create<Func<DecodeAssign<object[], object[]>>>(() => Linker.CreateArraySetter<object>())
					.SetGenericArguments(element)
					.Invoke(null);

				return Linker.LinkDecoderAsArray(descriptor, bindings, element, setter, parents);
			}

			// Try to bind descriptor as an array if target type IEnumerable<>
			foreach (Type interfaceType in type.GetInterfaces())
			{
				// Make sure that interface is IEnumerable<T> and store typeof(T)
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

					return Linker.LinkDecoderAsArray(descriptor, bindings, elementType, setter, parents);
				}
			}

			// Bind readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(bindings))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var setter = Generator.CreatePropertySetter(property);

				if (!Linker.LinkDecoderAsObject(descriptor, bindings, property.PropertyType, property.Name, setter, parents))
					return false;
			}

			// Bind public instance fields
			foreach (FieldInfo field in type.GetFields(bindings))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var setter = Generator.CreateFieldSetter(field);

				if (!Linker.LinkDecoderAsObject(descriptor, bindings, field.FieldType, field.Name, setter, parents))
					return false;
			}

			return true;
		}

		private static bool LinkDecoderAsArray<TEntity>(IDecoderDescriptor<TEntity> descriptor, BindingFlags bindings,
			Type type, object setter, IDictionary<Type, object> parents)
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
				.Create<Func<IDecoderDescriptor<object>, BindingFlags, Dictionary<Type, object>, bool>>((d, f, p) => Linker.LinkDecoder(d, f, p))
				.SetGenericArguments(type)
				.Invoke(null, itemDescriptor, bindings, parents);

			return result is bool success && success;
		}

		private static bool LinkDecoderAsObject<TEntity>(IDecoderDescriptor<TEntity> descriptor, BindingFlags bindings,
			Type type, string name, object setter, IDictionary<Type, object> parents)
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
				.Create<Func<IDecoderDescriptor<object>, BindingFlags, Dictionary<Type, object>, bool>>((d, f, p) => Linker.LinkDecoder(d, f, p))
				.SetGenericArguments(type)
				.Invoke(null, fieldDescriptor, bindings, parents);

			return result is bool success && success;
		}

		private static bool LinkEncoder<T>(IEncoderDescriptor<T> descriptor, BindingFlags bindings, Dictionary<Type, object> parents)
		{
			var type = typeof(T);

			parents[type] = descriptor;

			if (TryLinkEncoderAsValue(descriptor))
				return true;

			// Bind descriptor as an array of target type is also array
			if (type.IsArray)
			{
				var element = type.GetElementType();
				var getter = MethodResolver
					.Create<Func<Func<object, object>>>(() => Linker.CreateIdentity<object>())
					.SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(element))
					.Invoke(null);

				return Linker.LinkEncoderAsArray(descriptor, bindings, element, getter, parents);
			}

			// Try to bind descriptor as an array if target type IEnumerable<>
			foreach (Type interfaceType in type.GetInterfaces())
			{
				// Make sure that interface is IEnumerable<T> and store typeof(T)
				if (!TypeResolver.Create(interfaceType).HasSameDefinitionThan<IEnumerable<object>>(out var arguments))
					continue;

				var elementType = arguments[0];
				var getter = MethodResolver
					.Create<Func<Func<object, object>>>(() => Linker.CreateIdentity<object>())
					.SetGenericArguments(typeof(IEnumerable<>).MakeGenericType(elementType))
					.Invoke(null);

				return Linker.LinkEncoderAsArray(descriptor, bindings, elementType, getter, parents);
			}

			// Try to bind descriptor as a complex object otherwise
			var fieldDescriptor = descriptor.IsObject();

			// Bind readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(bindings))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var getter = Generator.CreatePropertyGetter(property);

				if (!Linker.LinkEncoderAsObject(fieldDescriptor, bindings, property.PropertyType, property.Name, getter, parents))
					return false;
			}

			// Bind public instance fields
			foreach (FieldInfo field in type.GetFields(bindings))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var getter = Generator.CreateFieldGetter(field);

				if (!Linker.LinkEncoderAsObject(fieldDescriptor, bindings, field.FieldType, field.Name, getter, parents))
					return false;
			}

			return true;
		}

		private static bool LinkEncoderAsArray<TEntity>(IEncoderDescriptor<TEntity> descriptor, BindingFlags bindings, Type type,
			object getter, IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				MethodResolver
					.Create<Func<IEncoderDescriptor<TEntity>, Func<TEntity, IEnumerable<object>>, IEncoderDescriptor<object>, IEncoderDescriptor<object>>>((d, a, p) => d.IsArray(a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, getter, recurse);

				return true;
			}

			var itemDescriptor = MethodResolver
				.Create<Func<IEncoderDescriptor<TEntity>, Func<TEntity, IEnumerable<object>>, IEncoderDescriptor<object>>>((d, a) => d.IsArray(a))
				.SetGenericArguments(type)
				.Invoke(descriptor, getter);

			var result = MethodResolver
				.Create<Func<IEncoderDescriptor<object>, BindingFlags, Dictionary<Type, object>, bool>>((d, f, p) => Linker.LinkEncoder(d, f, p))
				.SetGenericArguments(type)
				.Invoke(null, itemDescriptor, bindings, parents);

			return result is bool success && success;
		}

		private static bool LinkEncoderAsObject<TEntity>(IEncoderObjectDescriptor<TEntity> descriptor, BindingFlags bindings,
			Type type, string name, object getter, IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				MethodResolver
					.Create<Func<IEncoderObjectDescriptor<TEntity>, string, Func<TEntity, object>, IEncoderDescriptor<object>, IEncoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p))
					.SetGenericArguments(type)
					.Invoke(descriptor, name, getter, recurse);

				return true;
			}

			var fieldDescriptor = MethodResolver
				.Create<Func<IEncoderObjectDescriptor<TEntity>, string, Func<TEntity, object>, IEncoderDescriptor<object>>>((d, n, a) => d.HasField(n, a))
				.SetGenericArguments(type)
				.Invoke(descriptor, name, getter);

			var result = MethodResolver
				.Create<Func<IEncoderDescriptor<object>, BindingFlags, Dictionary<Type, object>, bool>>((d, f, p) => Linker.LinkEncoder(d, f, p))
				.SetGenericArguments(type)
				.Invoke(null, fieldDescriptor, bindings, parents);

			return result is bool success && success;
		}

		public static bool TryLinkDecoderAsValue<TEntity>(IDecoderDescriptor<TEntity> descriptor)
		{
			try
			{
				descriptor.IsValue();

				return true;
			}
			catch (InvalidCastException)
			{
				// Invalid cast exception being thrown means binding fails
				return false;
			}
		}

		public static bool TryLinkEncoderAsValue<TEntity>(IEncoderDescriptor<TEntity> descriptor)
		{
			try
			{
				descriptor.IsValue();

				return true;
			}
			catch (InvalidCastException)
			{
				// Invalid cast exception being thrown means binding fails
				return false;
			}
		}
	}
}