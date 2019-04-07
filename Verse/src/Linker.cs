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
				var assign = Linker.MakeAssignArray(element);

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

					var assign = Linker.MakeAssignArray(constructor, itemType);

					return Linker.LinkDecoderArray(descriptor, itemType, assign, parents);
				}
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var assign = Linker.MakeAssignField(property);

				if (!Linker.LinkDecoderField(descriptor, property.PropertyType, property.Name, assign, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var assign = Linker.MakeAssignField(field);

				if (!Linker.LinkDecoderField(descriptor, field.FieldType, field.Name, assign, parents))
					return false;
			}

			return true;
		}

		private static bool LinkDecoderArray<TEntity>(IDecoderDescriptor<TEntity> descriptor, Type type, object assign,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				Resolver
					.FindMethod<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>,
						IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p), null,
						new[] { type })
					.Invoke(descriptor, new[] { assign, recurse });

				return true;
			}

			recurse = Resolver
				.FindMethod<Func<IDecoderDescriptor<TEntity>, DecodeAssign<TEntity, IEnumerable<object>>,
					IDecoderDescriptor<object>>>((d, a) => d.HasItems(a), null, new[] { type })
				.Invoke(descriptor, new[] { assign });

			var result = Resolver
				.FindMethod<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(d, p) => Linker.LinkDecoder(d, p), null, new[] { type })
				.Invoke(null, new object[] { recurse, parents });

			return result is bool success && success;
		}

		private static bool LinkDecoderField<TEntity>(IDecoderDescriptor<TEntity> descriptor, Type type, string name,
			object assign, IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				Resolver
					.FindMethod<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>,
						IDecoderDescriptor<object>, IDecoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p),
						null, new[] { type })
					.Invoke(descriptor, new object[] { name, assign, recurse });

				return true;
			}

			recurse = Resolver
				.FindMethod<Func<IDecoderDescriptor<TEntity>, string, DecodeAssign<TEntity, object>,
					IDecoderDescriptor<object>>>((d, n, a) => d.HasField(n, a), null, new[] { type })
				.Invoke(descriptor, new object[] { name, assign });

			var result = Resolver
				.FindMethod<Func<IDecoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(d, p) => Linker.LinkDecoder(d, p), null, new[] { type })
				.Invoke(null, new object[] { recurse, parents });

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
				var access = Linker.MakeAccessArray(element);

				return Linker.LinkEncoderArray(descriptor, element, access, parents);
			}

			// Target type implements IEnumerable<> interface
			foreach (Type iface in type.GetInterfaces())
			{
				if (!Resolver.HasSameGenericDefinitionThan<IEnumerable<object>>(iface, out var argument))
					continue;

				var access = Linker.MakeAccessArray(argument);

				return Linker.LinkEncoderArray(descriptor, argument, access, parents);
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
					continue;

				var access = Linker.MakeAccessField(property);

				if (!Linker.LinkEncoderField(descriptor, property.PropertyType, property.Name, access, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
					continue;

				var access = Linker.MakeAccessField(field);

				if (!Linker.LinkEncoderField(descriptor, field.FieldType, field.Name, access, parents))
					return false;
			}

			return true;
		}

		private static bool LinkEncoderArray<T>(IEncoderDescriptor<T> descriptor, Type type, object access,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				Resolver
					.FindMethod<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>,
						IEncoderDescriptor<object>>>((d, a, p) => d.HasItems(a, p), null, new[] { type })
					.Invoke(descriptor, new[] { access, recurse });

				return true;
			}

			recurse = Resolver
				.FindMethod<Func<IEncoderDescriptor<T>, Func<T, IEnumerable<object>>, IEncoderDescriptor<object>>>(
					(d, a) => d.HasItems(a), null, new[] { type })
				.Invoke(descriptor, new[] { access });

			var result = Resolver
				.FindMethod<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(d, p) => Linker.LinkEncoder(d, p), null, new[] { type })
				.Invoke(null, new object[] { recurse, parents });

			return result is bool success && success;
		}

		private static bool LinkEncoderField<T>(IEncoderDescriptor<T> descriptor, Type type, string name, object access,
			IDictionary<Type, object> parents)
		{
			if (parents.TryGetValue(type, out var recurse))
			{
				Resolver
					.FindMethod<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>,
						IEncoderDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p), null, new[] { type })
					.Invoke(descriptor, new object[] { name, access, recurse });

				return true;
			}

			recurse = Resolver
				.FindMethod<Func<IEncoderDescriptor<T>, string, Func<T, object>, IEncoderDescriptor<object>>>(
					(d, n, a) => d.HasField(n, a), null, new[] { type })
				.Invoke(descriptor, new object[] { name, access });

			var result = Resolver
				.FindMethod<Func<IEncoderDescriptor<object>, Dictionary<Type, object>, bool>>(
					(d, p) => Linker.LinkEncoder(d, p), null, new[] { type })
				.Invoke(null, new object[] { recurse, parents });

			return result is bool success && success;
		}

		private static object MakeAccessArray(Type inner)
		{
			var enumerable = typeof(IEnumerable<>).MakeGenericType(inner);
			var method = new DynamicMethod(string.Empty, enumerable, new[] { enumerable }, inner.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(typeof(Func<,>).MakeGenericType(enumerable, enumerable));
		}

		private static object MakeAccessField(FieldInfo field)
		{
			var method = new DynamicMethod(string.Empty, field.FieldType, new[] { field.DeclaringType }, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(typeof(Func<,>).MakeGenericType(field.DeclaringType, field.FieldType));
		}

		private static object MakeAccessField(PropertyInfo property)
		{
			var method = new DynamicMethod(string.Empty, property.PropertyType, new[] { property.DeclaringType },
				property.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Call, property.GetGetMethod());
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(typeof(Func<,>).MakeGenericType(property.DeclaringType,
				property.PropertyType));
		}

		/// <summary>
		/// Generate DecoderAssign delegate from IEnumerable to any compatible
		/// object, using a constructor taking the IEnumerable as its argument.
		/// </summary>
		/// <param name="constructor">Compatible constructor</param>
		/// <param name="inner">Inner elements type</param>
		/// <returns>DecoderAssign delegate</returns>
		private static object MakeAssignArray(ConstructorInfo constructor, Type inner)
		{
			var enumerable = typeof(IEnumerable<>).MakeGenericType(inner);
			var method = new DynamicMethod(string.Empty, null,
				new[] { constructor.DeclaringType.MakeByRefType(), enumerable }, constructor.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Newobj, constructor);

			if (constructor.DeclaringType.IsValueType)
				generator.Emit(OpCodes.Stobj, constructor.DeclaringType);
			else
				generator.Emit(OpCodes.Stind_Ref);

			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(
				typeof(DecodeAssign<,>).MakeGenericType(constructor.DeclaringType, enumerable));
		}

		/// <summary>
		/// Generate DecoderAssign delegate from IEnumerable to compatible array
		/// type, using Linq Enumerable.ToArray conversion.
		/// </summary>
		/// <param name="inner">Inner elements type</param>
		/// <returns>DecoderAssign delegate</returns>
		private static object MakeAssignArray(Type inner)
		{
			var converter = Resolver.FindMethod<Func<IEnumerable<object>, object[]>>((e) => e.ToArray(), null, new[] { inner });
			var enumerable = typeof(IEnumerable<>).MakeGenericType(inner);
			var method = new DynamicMethod(string.Empty, null, new[] { inner.MakeArrayType().MakeByRefType(), enumerable },
				converter.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, converter);
			generator.Emit(OpCodes.Stind_Ref);
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(typeof(DecodeAssign<,>).MakeGenericType(inner.MakeArrayType(), enumerable));
		}

		private static object MakeAssignField(FieldInfo field)
		{
			var method = new DynamicMethod(string.Empty, null,
				new[] { field.DeclaringType.MakeByRefType(), field.FieldType },
				field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!field.DeclaringType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Stfld, field);
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(typeof(DecodeAssign<,>).MakeGenericType(field.DeclaringType, field.FieldType));
		}

		private static object MakeAssignField(PropertyInfo property)
		{
			var method = new DynamicMethod(string.Empty, null,
				new[] { property.DeclaringType.MakeByRefType(), property.PropertyType }, property.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);

			if (!property.DeclaringType.IsValueType)
				generator.Emit(OpCodes.Ldind_Ref);

			generator.Emit(OpCodes.Ldarg_1);
			generator.Emit(OpCodes.Call, property.GetSetMethod());
			generator.Emit(OpCodes.Ret);

			return method.CreateDelegate(
				typeof(DecodeAssign<,>).MakeGenericType(property.DeclaringType, property.PropertyType));
		}
	}
}