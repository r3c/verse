using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Verse.Dynamics;
using Verse.Events;

namespace Verse.Models
{
	abstract class	AbstractDecoder<T> : IDecoder<T>
	{
		#region Events
		
		public event StreamErrorEvent	OnStreamError;

		public event TypeErrorEvent		OnTypeError;
		
		#endregion

		#region Methods / Abstract

		public abstract bool		Decode (Stream stream, out T instance);

		public abstract IDecoder<U>	HasField<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter);

		public abstract IDecoder<U>	HasItems<U> (Func<U> generator, DecoderArraySetter<T, U> setter);

		public abstract IDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter);

		protected abstract bool		TryLink ();

		#endregion

		#region Methods / Public

		public void	Link ()
		{
			Type[]		arguments;
			Type		container;
			TypeFilter	filter;
			Type		inner;

			if (this.TryLink ())
				return;

			container = typeof (T);

			// Check whether type has items or pairs
			filter = new TypeFilter ((type, criteria) => type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IEnumerable<>));

			foreach (Type contract in container.FindInterfaces (filter, null))
			{
				arguments = contract.GetGenericArguments ();

				if (arguments.Length == 1)
				{
					inner = arguments[0];

					if (inner.IsGenericType && inner.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
					{
						arguments = inner.GetGenericArguments ();
	
						if (arguments.Length == 2 && arguments[0] == typeof (string))
						{
							inner = arguments[1];

							AbstractDecoder<T>.LinkInvoke (inner, MethodResolver
								.Resolve<Func<IDecoder<T>, DecoderMapSetter<T, object>, IDecoder<object>>> ((decoder, setter) => decoder.HasPairs (setter))
								.MakeGenericMethod (inner)
								.Invoke (this, new object[] {AbstractDecoder<T>.MakeMapSetter (container, inner)}));

							return;
						}
					}

					AbstractDecoder<T>.LinkInvoke (inner, MethodResolver
						.Resolve<Func<IDecoder<T>, DecoderArraySetter<T, object>, IDecoder<object>>> ((decoder, setter) => decoder.HasItems (setter))
						.MakeGenericMethod (inner)
						.Invoke (this, new object[] {AbstractDecoder<T>.MakeArraySetter (container, inner)}));

					return;
				}
			}

			// Browse public readable and writable properties
			foreach (PropertyInfo property in container.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || (property.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
					continue;

				AbstractDecoder<T>.LinkInvoke (property.PropertyType, MethodResolver
					.Resolve<Func<IDecoder<T>, string, DecoderValueSetter<T, object>, IDecoder<object>>> ((decoder, name, setter) => decoder.HasField (name, setter))
					.MakeGenericMethod (property.PropertyType)
					.Invoke (this, new object[] {property.Name, AbstractDecoder<T>.MakeValueSetter (property)}));
			}

			// Browse public fields
			foreach (FieldInfo field in container.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if ((field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName)
					continue;

				AbstractDecoder<T>.LinkInvoke (field.FieldType, MethodResolver
					.Resolve<Func<IDecoder<T>, string, DecoderValueSetter<T, object>, IDecoder<object>>> ((decoder, name, setter) => decoder.HasField (name, setter))
					.MakeGenericMethod (field.FieldType)
					.Invoke (this, new object[] {field.Name, AbstractDecoder<T>.MakeValueSetter (field)}));
			}
		}

		public IDecoder<U>	HasField<U> (string name, DecoderValueSetter<T, U> setter)
		{
			return this.HasField<U> (name, ConstructorGenerator.Generate<U> (), setter);
		}

		public IDecoder<U>	HasItems<U> (DecoderArraySetter<T, U> setter)
		{
			return this.HasItems<U> (ConstructorGenerator.Generate<U> (), setter);
		}

		public IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter)
		{
			return this.HasPairs<U> (ConstructorGenerator.Generate<U> (), setter);
		}

		#endregion
		
		#region Methods / Protected

		protected void	EventStreamError (long position, string message)
		{
			StreamErrorEvent	error;

			error = this.OnStreamError;

			if (error != null)
				error (position, message);
		}

		protected void	EventTypeError (Type type, string value)
		{
			TypeErrorEvent	error;

			error = this.OnTypeError;

			if (error != null)
				error (type, value);
		}

		#endregion

		#region Methods / Private

		private static void	LinkInvoke (Type type, object decoder)
		{
			typeof (IDecoder<>)
				.MakeGenericType (type)
				.GetMethod ("Link", BindingFlags.Instance | BindingFlags.Public)
				.Invoke (decoder, null);
		}

		private static object	MakeArraySetter (Type container, Type inner)
		{
        	ILGenerator		generator;
			Type			generic;
			DynamicMethod	method;

			generic = typeof (ICollection<>).MakeGenericType (inner);
			method = new DynamicMethod (string.Empty, null, new Type[] {container.MakeByRefType (), generic}, container.Module, true);

			generator = method.GetILGenerator ();

			if (container.IsArray)
			{
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, generic.GetProperty ("Count", BindingFlags.Instance | BindingFlags.Public).GetGetMethod ());
				generator.Emit (OpCodes.Call, typeof (Array).GetMethod ("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (inner));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Ldc_I4_0);
				generator.Emit (OpCodes.Callvirt, generic.GetMethod ("CopyTo", BindingFlags.Instance | BindingFlags.Public));
				generator.Emit (OpCodes.Ret);

				return method.CreateDelegate (typeof (DecoderArraySetter<,>).MakeGenericType (container, inner));
			}

			throw new NotImplementedException ();
		}

		private static object	MakeMapSetter (Type container, Type inner)
		{
			throw new NotImplementedException ();
		}

		private static object	MakeValueSetter (FieldInfo field)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, null, new Type[] {field.DeclaringType.MakeByRefType (), field.FieldType}, field.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);

			if (!field.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldind_Ref);

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Stfld, field);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (DecoderValueSetter<,>).MakeGenericType (field.DeclaringType, field.FieldType));
		}

		private static object	MakeValueSetter (PropertyInfo property)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, null, new Type[] {property.DeclaringType.MakeByRefType (), property.PropertyType}, property.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);

			if (!property.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldind_Ref);

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Call, property.GetSetMethod ());
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (DecoderValueSetter<,>).MakeGenericType (property.DeclaringType, property.PropertyType));
		}

		#endregion
	}
}
