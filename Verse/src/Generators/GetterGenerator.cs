using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Generators
{
	internal static class GetterGenerator
	{
		/// <Summary>
		/// Create field getter delegate for given runtime field.
		/// </Summary>
		public static Func<TEntity, TField> CreateFromField<TEntity, TField>(FieldInfo field)
		{
			if (field.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"field declaring type is not {typeof(TEntity)}", nameof(field));

			if (field.FieldType != typeof(TField))
				throw new ArgumentException($"field type is not {typeof(TField)}", nameof(field));

			var parameterTypes = new[] {typeof(TEntity)};
			var method = new DynamicMethod(string.Empty, field.FieldType, parameterTypes, field.Module, true);
			var generator = method.GetILGenerator();

			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Ldfld, field);
			generator.Emit(OpCodes.Ret);

			return (Func<TEntity, TField>) method.CreateDelegate(typeof(Func<TEntity, TField>));
		}

		/// <Summary>
		/// Create property getter delegate for given runtime property.
		/// </Summary>
		public static Func<TEntity, TProperty> CreateFromProperty<TEntity, TProperty>(PropertyInfo property)
		{
			if (property.DeclaringType != typeof(TEntity))
				throw new ArgumentException($"property declaring type is not {typeof(TEntity)}", nameof(property));

			if (property.PropertyType != typeof(TProperty))
				throw new ArgumentException($"property type is not {typeof(TProperty)}", nameof(property));

			var getter = property.GetGetMethod();

			if (getter == null)
				throw new ArgumentException("property has no getter", nameof(property));

			return (Func<TEntity, TProperty>) Delegate.CreateDelegate(typeof(Func<TEntity, TProperty>), getter);
		}

		/// <summary>
		/// Create identity getter.
		/// </summary>
		public static Func<TEntity, TEntity> CreateIdentity<TEntity>()
		{
			return e => e;
		}
	}
}
