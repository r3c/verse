using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Reflection.Emit;

using Verse.Dynamics;
using Verse.Events;

namespace Verse.Models
{
	public abstract class	AbstractEncoder<T> : IEncoder<T>
	{
		#region Events

		public event StreamErrorEvent	OnStreamError;

		public event TypeErrorEvent		OnTypeError;

		#endregion

		#region Methods / Abstract

		protected abstract AbstractEncoder<U>	DefineAttribute<U> (string name, EncoderValueGetter<T, U> getter);

		protected abstract AbstractEncoder<U>	DefineElements<U> (EncoderArrayGetter<T, U> getter);

		protected abstract AbstractEncoder<U>	DefinePairs<U> (EncoderMapGetter<T, U> getter);

		public abstract bool					Encode (Stream stream, T instance);

		public abstract void					HasAttribute<U> (string name, EncoderValueGetter<T, U> getter, IEncoder<U> encoder);

		public abstract void					HasElements<U> (EncoderArrayGetter<T, U> getter, IEncoder<U> encoder);

		public abstract void					HasPairs<U> (EncoderMapGetter<T, U> getter, IEncoder<U> encoder);

		protected abstract bool					TryLink ();

		#endregion

		#region Methods / Public

		public IEncoder<U>	HasAttribute<U> (string name, EncoderValueGetter<T, U> getter)
		{
			return this.DefineAttribute (name, getter);
		}

		public IEncoder<U>	HasElements<U> (EncoderArrayGetter<T, U> getter)
		{
			return this.DefineElements (getter);
		}

		public IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter)
		{
			return this.DefinePairs (getter);
		}

		public void	Link ()
		{
			this.AutoLink (new Dictionary<Type, object> ());
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

		private void	AutoLink (Dictionary<Type, object> encoders)
		{
			Type[]		arguments;
			Type		container;
			TypeFilter	filter;
			Type		inner;

			if (this.TryLink ())
				return;

			container = typeof (T);

			encoders = new Dictionary<Type, object> (encoders);
			encoders[container] = this;

			// Check is container type is or implements IEnumerable<> interface
			if (container.IsGenericType && container.GetGenericTypeDefinition () == typeof (IEnumerable<>))
			{
				arguments = container.GetGenericArguments ();
				inner = arguments.Length == 1 ? arguments[0] : null;
			}
			else
			{
				filter = new TypeFilter ((type, criteria) => type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IEnumerable<>));
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

			// Container is an IEnumerable<> of element type "inner"
			if (inner != null)
			{
				if (inner.IsGenericType && inner.GetGenericTypeDefinition () == typeof (KeyValuePair<,>))
				{
					arguments = inner.GetGenericArguments ();

					if (arguments.Length == 2 && arguments[0] == typeof (string))
					{
						this.LinkPairs (encoders, arguments[1], AbstractEncoder<T>.MakeMapGetter (container, arguments[1]));

						return;
					}
				}

				this.LinkElements (encoders, inner, AbstractEncoder<T>.MakeArrayGetter (container, inner));

				return;
			}

			// Browse public readable and writable properties
			foreach (PropertyInfo property in container.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || (property.Attributes & PropertyAttributes.SpecialName) == PropertyAttributes.SpecialName)
					continue;

				this.LinkAttribute (encoders, property.PropertyType, property.Name, AbstractEncoder<T>.MakeValueGetter (property));
			}

			// Browse public fields
			foreach (FieldInfo field in container.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if ((field.Attributes & FieldAttributes.SpecialName) == FieldAttributes.SpecialName)
					continue;

				this.LinkAttribute (encoders, field.FieldType, field.Name, AbstractEncoder<T>.MakeValueGetter (field));
			}
		}

		private static void	InvokeLink (Type type, object target, Dictionary<Type, object> encoders)
		{
			Resolver<AbstractEncoder<object>>
				.Method<Dictionary<Type, object>> ((encoder, e) => encoder.AutoLink (e), new Type[] {type})
				.Invoke (target, new object[] {encoders});
		}

		private void	LinkAttribute (Dictionary<Type, object> encoders, Type type, string name, object getter)
		{
			object	known;

			if (encoders.TryGetValue (type, out known))
			{
				Resolver<AbstractEncoder<T>>
					.Method<string, EncoderValueGetter<T, object>, IEncoder<object>> ((e, n, g, r) => e.HasAttribute (n, g, r), null, new Type[] {type})
					.Invoke (this, new object[] {name, getter, known});
			}
			else
			{
				AbstractEncoder<T>.InvokeLink (type, Resolver<AbstractEncoder<T>>
					.Method<string, EncoderValueGetter<T, object>, AbstractEncoder<object>> ((e, n, g) => e.DefineAttribute (n, g), null, new Type[] {type})
					.Invoke (this, new object[] {name, getter}), encoders);
			}
		}

		private void	LinkElements (Dictionary<Type, object> encoders, Type type, object getter)
		{
			object	known;

			if (encoders.TryGetValue (type, out known))
			{
				Resolver<AbstractEncoder<T>>
					.Method<EncoderArrayGetter<T, object>, IEncoder<object>> ((e, g, r) => e.HasElements (g, r), null, new Type[] {type})
					.Invoke (this, new object[] {getter, known});
			}
			else
			{
				AbstractEncoder<T>.InvokeLink (type, Resolver<AbstractEncoder<T>>
					.Method<EncoderArrayGetter<T, object>, AbstractEncoder<object>> ((e, g) => e.DefineElements (g), null, new Type[] {type})
					.Invoke (this, new object[] {getter}), encoders);
			}
		}

		private void	LinkPairs (Dictionary<Type, object> encoders, Type type, object getter)
		{
			object	known;

			if (encoders.TryGetValue (type, out known))
			{
				Resolver<AbstractEncoder<T>>
					.Method<EncoderMapGetter<T, object>, IEncoder<object>> ((e, g, r) => e.HasPairs (g, r), null, new Type[] {type})
					.Invoke (this, new object[] {getter, known});
			}
			else
			{
				AbstractEncoder<T>.InvokeLink (type, Resolver<AbstractEncoder<T>>
					.Method<EncoderMapGetter<T, object>, AbstractEncoder<object>> ((e, g) => e.DefinePairs (g), null, new Type[] {type})
					.Invoke (this, new object[] {getter}), encoders);
			}
		}

		private static object	MakeArrayGetter (Type container, Type inner)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, typeof (IEnumerable<>).MakeGenericType (inner), new Type[] {container}, container.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (EncoderArrayGetter<,>).MakeGenericType (container, inner));
		}

		private static object	MakeMapGetter (Type container, Type inner)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, typeof (IEnumerable<>).MakeGenericType (typeof (KeyValuePair<,>).MakeGenericType (typeof (string), inner)), new Type[] {container}, container.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (EncoderMapGetter<,>).MakeGenericType (container, inner));
		}

		private static object	MakeValueGetter (FieldInfo field)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, field.FieldType, new Type[] {field.DeclaringType}, field.Module, true);

			generator = method.GetILGenerator ();

			if (field.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldarga_S, 0);
			else
				generator.Emit (OpCodes.Ldarg_0);

			generator.Emit (OpCodes.Ldfld, field);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (EncoderValueGetter<,>).MakeGenericType (field.DeclaringType, field.FieldType));
		}

		private static object	MakeValueGetter (PropertyInfo property)
		{
        	ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, property.PropertyType, new Type[] {property.DeclaringType}, property.Module, true);

			generator = method.GetILGenerator ();

			if (property.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldarga_S, 0);
			else
				generator.Emit (OpCodes.Ldarg_0);

			generator.Emit (OpCodes.Call, property.GetGetMethod ());
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (EncoderValueGetter<,>).MakeGenericType (property.DeclaringType, property.PropertyType));
		}

		#endregion
	}
}
