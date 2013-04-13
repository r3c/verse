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

			// Check is container type is or implements ICollection<> interface
			if (container.IsGenericType && container.GetGenericTypeDefinition () == typeof (ICollection<>))
			{
				arguments = container.GetGenericArguments ();
				inner = arguments.Length == 1 ? arguments[0] : null;
			}
			else
			{
				filter = new TypeFilter ((type, criteria) => type.IsGenericType && type.GetGenericTypeDefinition () == typeof (ICollection<>));
				inner = null;

				foreach (Type contract in container.FindInterfaces (filter, null))
				{
					arguments = contract.GetGenericArguments ();

					if (arguments.Length == 1)
					{
						inner = arguments[0];

						break;
					}
				}
			}

			// Container is an ICollection<> of element type "inner"
			if (inner != null)
			{
				if (inner.IsGenericType && inner.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
				{
					arguments = inner.GetGenericArguments ();

					if (arguments.Length == 2 && arguments[0] == typeof (string))
					{
						inner = arguments[1];

						AbstractDecoder<T>.LinkInvoke (inner, Resolver
							.Method<IDecoder<T>, DecoderMapSetter<T, object>, IDecoder<object>> ((decoder, setter) => decoder.HasPairs (setter), null, new Type[] {inner})
							.Invoke (this, new object[] {AbstractDecoder<T>.MakeMapSetter (container, inner)}));

						return;
					}
				}

				AbstractDecoder<T>.LinkInvoke (inner, Resolver
					.Method<IDecoder<T>, DecoderArraySetter<T, object>, IDecoder<object>> ((decoder, setter) => decoder.HasItems (setter), null, new Type[] {inner})
					.Invoke (this, new object[] {AbstractDecoder<T>.MakeArraySetter (container, inner)}));

				return;
			}

			// Browse public readable and writable properties
			foreach (PropertyInfo property in container.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || (property.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
					continue;

				AbstractDecoder<T>.LinkInvoke (property.PropertyType, Resolver
					.Method<IDecoder<T>, string, DecoderValueSetter<T, object>, IDecoder<object>> ((decoder, name, setter) => decoder.HasField (name, setter), null, new Type[] {property.PropertyType})
					.Invoke (this, new object[] {property.Name, AbstractDecoder<T>.MakeValueSetter (property)}));
			}

			// Browse public fields
			foreach (FieldInfo field in container.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if ((field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName)
					continue;

				AbstractDecoder<T>.LinkInvoke (field.FieldType, Resolver
					.Method<IDecoder<T>, string, DecoderValueSetter<T, object>, IDecoder<object>> ((decoder, name, setter) => decoder.HasField (name, setter), null, new Type[] {field.FieldType})
					.Invoke (this, new object[] {field.Name, AbstractDecoder<T>.MakeValueSetter (field)}));
			}
		}

		public IDecoder<U>	HasField<U> (string name, DecoderValueSetter<T, U> setter)
		{
			return this.HasField<U> (name, Generator.Constructor<U> (), setter);
		}

		public IDecoder<U>	HasItems<U> (DecoderArraySetter<T, U> setter)
		{
			return this.HasItems<U> (Generator.Constructor<U> (), setter);
		}

		public IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter)
		{
			return this.HasPairs<U> (Generator.Constructor<U> (), setter);
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

		private static void	LinkInvoke (Type type, object target)
		{
			Resolver
				.Method<IDecoder<object>> ((decoder) => decoder.Link (), new Type[] {type})
				.Invoke (target, null);
		}

		private static object	MakeArraySetter (Type container, Type inner)
		{
        	ILGenerator		generator;
			Label			loop;
			DynamicMethod	method;
			Label			test;

			method = new DynamicMethod (string.Empty, null, new Type[] {container.MakeByRefType (), typeof (ICollection<>).MakeGenericType (inner)}, container.Module, true);
			generator = method.GetILGenerator ();

			if (container.IsArray)
			{
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Property<ICollection<object>, int> ((collection) => collection.Count, new Type[] {inner}).GetGetMethod ());
				#warning use static reflection
				generator.Emit (OpCodes.Call, typeof (Array).GetMethod ("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (inner));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Ldc_I4_0);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, object[], int> ((collection, array, index) => collection.CopyTo (array, index), new Type[] {inner}));
				generator.Emit (OpCodes.Ret);
			}
			else
			{
				loop = generator.DefineLabel ();
				test = generator.DefineLabel ();

				generator.DeclareLocal (container);
				generator.DeclareLocal (typeof (IEnumerator<>).MakeGenericType (inner));

				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Stloc_0);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>> ((collection) => collection.Clear (), new Type[] {inner}));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, IEnumerator<object>> ((collection) => collection.GetEnumerator (), new Type[] {inner}));
				generator.Emit (OpCodes.Stloc_1);
				generator.Emit (OpCodes.Br, test);

				generator.MarkLabel (loop);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Property<IEnumerator<object>, object> ((enumerator) => enumerator.Current, new Type[] {inner}).GetGetMethod ());
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, object> ((collection, item) => collection.Add (item), new Type[] {inner}));

				generator.MarkLabel (test);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<System.Collections.IEnumerator, bool> ((enumerator) => enumerator.MoveNext ()));
				generator.Emit (OpCodes.Brtrue_S, loop);
				generator.Emit (OpCodes.Ret);
			}

			return method.CreateDelegate (typeof (DecoderArraySetter<,>).MakeGenericType (container, inner));
		}

		private static object	MakeMapSetter (Type container, Type inner)
		{
			Type			element;
        	ILGenerator		generator;
			Label			loop;
			DynamicMethod	method;
			Label			test;

			element = typeof (KeyValuePair<,>).MakeGenericType (typeof (string), inner);
			method = new DynamicMethod (string.Empty, null, new Type[] {container.MakeByRefType (), typeof (ICollection<>).MakeGenericType (element)}, container.Module, true);

			generator = method.GetILGenerator ();

			if (container.IsArray)
			{
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Property<ICollection<object>, int> ((collection) => collection.Count, new Type[] {element}).GetGetMethod ());
				#warning use static reflection
				generator.Emit (OpCodes.Call, typeof (Array).GetMethod ("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (inner));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Ldc_I4_0);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, object[], int> ((collection, array, index) => collection.CopyTo (array, index), new Type[] {element}));
				generator.Emit (OpCodes.Ret);
			}
			else
			{
				loop = generator.DefineLabel ();
				test = generator.DefineLabel ();

				generator.DeclareLocal (container);
				generator.DeclareLocal (typeof (IEnumerator<>).MakeGenericType (inner));

				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Stloc_0);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>> ((collection) => collection.Clear (), new Type[] {element}));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, IEnumerator<object>> ((collection) => collection.GetEnumerator (), new Type[] {element}));
				generator.Emit (OpCodes.Stloc_1);
				generator.Emit (OpCodes.Br, test);

				generator.MarkLabel (loop);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Property<IEnumerator<object>, object> ((enumerator) => enumerator.Current, new Type[] {element}).GetGetMethod ());
				generator.Emit (OpCodes.Callvirt, Resolver.Method<ICollection<object>, object> ((collection, item) => collection.Add (item), new Type[] {element}));

				generator.MarkLabel (test);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver.Method<System.Collections.IEnumerator, bool> ((enumerator) => enumerator.MoveNext ()));
				generator.Emit (OpCodes.Brtrue_S, loop);
				generator.Emit (OpCodes.Ret);
			}

			return method.CreateDelegate (typeof (DecoderMapSetter<,>).MakeGenericType (container, inner));
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
