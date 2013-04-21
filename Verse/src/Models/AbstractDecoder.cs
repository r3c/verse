using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Verse.Dynamics;
using Verse.Events;

namespace Verse.Models
{
	public abstract class	AbstractDecoder<T> : IDecoder<T>
	{
		#region Events
		
		public event StreamErrorEvent	OnStreamError;

		public event TypeErrorEvent		OnTypeError;
		
		#endregion

		#region Methods / Abstract

		public abstract bool					Decode (Stream stream, out T instance);

		protected abstract AbstractDecoder<U>	HasAttributeAbstract<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter);

		public abstract void					HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter, IDecoder<U> decoder);

		protected abstract AbstractDecoder<U>	HasElementsAbstract<U> (Func<U> generator, DecoderArraySetter<T, U> setter);

		public abstract void					HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter, IDecoder<U> decoder);

		protected abstract AbstractDecoder<U>	HasPairsAbstract<U> (Func<U> generator, DecoderMapSetter<T, U> setter);

		public abstract void					HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter, IDecoder<U> decoder);

		protected abstract bool					TryLink ();

		#endregion

		#region Methods / Public

		public IDecoder<U>	HasAttribute<U> (string name, Func<U> generator, DecoderValueSetter<T, U> setter)
		{
			return this.HasAttributeAbstract (name, generator, setter);
		}

		public IDecoder<U>	HasAttribute<U> (string name, DecoderValueSetter<T, U> setter)
		{
			return this.HasAttributeAbstract (name, setter);
		}

		public void	HasAttribute<U> (string name, DecoderValueSetter<T, U> setter, IDecoder<U> decoder)
		{
			this.HasAttribute (name, Generator.Constructor<U> (), setter, decoder);
		}

		public IDecoder<U>	HasElements<U> (Func<U> generator, DecoderArraySetter<T, U> setter)
		{
			return this.HasElementsAbstract (generator, setter);
		}

		public IDecoder<U>	HasElements<U> (DecoderArraySetter<T, U> setter)
		{
			return this.HasElementsAbstract (setter);
		}

		public void	HasElements<U> (DecoderArraySetter<T, U> setter, IDecoder<U> decoder)
		{
			this.HasElements (Generator.Constructor<U> (), setter, decoder);
		}

		public IDecoder<U>	HasPairs<U> (Func<U> generator, DecoderMapSetter<T, U> setter)
		{
			return this.HasPairsAbstract (generator, setter);
		}

		public IDecoder<U>	HasPairs<U> (DecoderMapSetter<T, U> setter)
		{
			return this.HasPairsAbstract (setter);
		}

		public void	HasPairs<U> (DecoderMapSetter<T, U> setter, IDecoder<U> decoder)
		{
			this.HasPairs (Generator.Constructor<U> (), setter, decoder);
		}

		public void	Link ()
		{
			this.LinkType (new Dictionary<Type, object> ());
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

		protected AbstractDecoder<U>	HasAttributeAbstract<U> (string name, DecoderValueSetter<T, U> setter)
		{
			return this.HasAttributeAbstract (name, Generator.Constructor<U> (), setter);
		}

		protected AbstractDecoder<U>	HasElementsAbstract<U> (DecoderArraySetter<T, U> setter)
		{
			return this.HasElementsAbstract (Generator.Constructor<U> (), setter);
		}

		protected AbstractDecoder<U>	HasPairsAbstract<U> (DecoderMapSetter<T, U> setter)
		{
			return this.HasPairsAbstract (Generator.Constructor<U> (), setter);
		}

		#endregion

		#region Methods / Private

		private static void	InvokeLink (Type type, object target, Dictionary<Type, object> decoders)
		{
			Resolver<AbstractDecoder<object>>
				.Method<Dictionary<Type, object>> ((decoder, d) => decoder.LinkType (d), new Type[] {type})
				.Invoke (target, new object[] {decoders});
		}

		private void	LinkDefineField (Dictionary<Type, object> decoders, Type type, string name, object setter)
		{
			object	known;

			if (decoders.TryGetValue (type, out known))
			{
				Resolver<AbstractDecoder<T>>
					.Method<string, DecoderValueSetter<T, object>, IDecoder<object>> ((d, n, s, r) => d.HasAttribute (n, s, r), null, new Type[] {type})
					.Invoke (this, new object[] {name, setter, known});
			}
			else
			{
				AbstractDecoder<T>.InvokeLink (type, Resolver<AbstractDecoder<T>>
					.Method<string, DecoderValueSetter<T, object>, AbstractDecoder<object>> ((d, n, s) => d.HasAttributeAbstract (n, s), null, new Type[] {type})
					.Invoke (this, new object[] {name, setter}), decoders);
			}
		}

		private void	LinkDefineItems (Dictionary<Type, object> decoders, Type type, object setter)
		{
			object	known;

			if (decoders.TryGetValue (type, out known))
			{
				Resolver<AbstractDecoder<T>>
					.Method<DecoderArraySetter<T, object>, IDecoder<object>> ((d, s, r) => d.HasElements (s, r), null, new Type[] {type})
					.Invoke (this, new object[] {setter, known});
			}
			else
			{
				AbstractDecoder<T>.InvokeLink (type, Resolver<AbstractDecoder<T>>
					.Method<DecoderArraySetter<T, object>, AbstractDecoder<object>> ((d, s) => d.HasElementsAbstract (s), null, new Type[] {type})
					.Invoke (this, new object[] {setter}), decoders);
			}
		}

		private void	LinkDefinePairs (Dictionary<Type, object> decoders, Type type, object setter)
		{
			object	known;

			if (decoders.TryGetValue (type, out known))
			{
				Resolver<AbstractDecoder<T>>
					.Method<DecoderMapSetter<T, object>, IDecoder<object>> ((d, s, r) => d.HasPairs (s, r), null, new Type[] {type})
					.Invoke (this, new object[] {setter, known});
			}
			else
			{
				AbstractDecoder<T>.InvokeLink (type, Resolver<AbstractDecoder<T>>
					.Method<DecoderMapSetter<T, object>, AbstractDecoder<object>> ((d, s) => d.HasPairsAbstract (s), null, new Type[] {type})
					.Invoke (this, new object[] {setter}), decoders);
			}
		}

		private void	LinkType (Dictionary<Type, object> decoders)
		{
			Type[]		arguments;
			Type		container;
			TypeFilter	filter;
			Type		inner;

			if (this.TryLink ())
				return;

			container = typeof (T);

			decoders = new Dictionary<Type, object> (decoders);
			decoders[container] = this;

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
						this.LinkDefinePairs (decoders, arguments[1], AbstractDecoder<T>.MakeMapSetter (container, arguments[1]));

						return;
					}
				}

				this.LinkDefineItems (decoders, inner, AbstractDecoder<T>.MakeArraySetter (container, inner));

				return;
			}

			// Browse public readable and writable properties
			foreach (PropertyInfo property in container.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || (property.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
					continue;

				this.LinkDefineField (decoders, property.PropertyType, property.Name, AbstractDecoder<T>.MakeValueSetter (property));
			}

			// Browse public fields
			foreach (FieldInfo field in container.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if ((field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName)
					continue;

				this.LinkDefineField (decoders, field.FieldType, field.Name, AbstractDecoder<T>.MakeValueSetter (field));
			}
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
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Property<int> ((collection) => collection.Count, new Type[] {inner}).GetGetMethod ());
				#warning use static reflection
				generator.Emit (OpCodes.Call, typeof (Array).GetMethod ("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (inner));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Ldc_I4_0);
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<object[], int> ((collection, array, index) => collection.CopyTo (array, index), new Type[] {inner}));
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
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method ((collection) => collection.Clear (), new Type[] {inner}));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<IEnumerator<object>> ((collection) => collection.GetEnumerator (), new Type[] {inner}));
				generator.Emit (OpCodes.Stloc_1);
				generator.Emit (OpCodes.Br, test);

				generator.MarkLabel (loop);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver<IEnumerator<object>>.Property<object> ((enumerator) => enumerator.Current, new Type[] {inner}).GetGetMethod ());
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<object> ((collection, item) => collection.Add (item), new Type[] {inner}));

				generator.MarkLabel (test);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver<System.Collections.IEnumerator>.Method<bool> ((enumerator) => enumerator.MoveNext ()));
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
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Property<int> ((collection) => collection.Count, new Type[] {element}).GetGetMethod ());
				#warning use static reflection
				generator.Emit (OpCodes.Call, typeof (Array).GetMethod ("Resize", BindingFlags.Public | BindingFlags.Static).MakeGenericMethod (inner));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Ldarg_0);
				generator.Emit (OpCodes.Ldind_Ref);
				generator.Emit (OpCodes.Ldc_I4_0);
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<object[], int> ((collection, array, index) => collection.CopyTo (array, index), new Type[] {element}));
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
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method ((collection) => collection.Clear (), new Type[] {element}));
				generator.Emit (OpCodes.Ldarg_1);
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<IEnumerator<object>> ((collection) => collection.GetEnumerator (), new Type[] {element}));
				generator.Emit (OpCodes.Stloc_1);
				generator.Emit (OpCodes.Br, test);

				generator.MarkLabel (loop);
				generator.Emit (OpCodes.Ldloc_0);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver<IEnumerator<object>>.Property<object> ((enumerator) => enumerator.Current, new Type[] {element}).GetGetMethod ());
				generator.Emit (OpCodes.Callvirt, Resolver<ICollection<object>>.Method<object> ((collection, item) => collection.Add (item), new Type[] {element}));

				generator.MarkLabel (test);
				generator.Emit (OpCodes.Ldloc_1);
				generator.Emit (OpCodes.Callvirt, Resolver<System.Collections.IEnumerator>.Method<bool> ((enumerator) => enumerator.MoveNext ()));
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
