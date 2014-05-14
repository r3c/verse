using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Verse.Tools;

namespace Verse
{
	public class Linker
	{
		#region Events

		public event Action<Type, string>	Error;

		#endregion

		#region Methods / Public / Instance

		public bool LinkBuilder<T> (IBuilderDescriptor<T> descriptor)
		{
			return this.LinkBuilder (descriptor, new Dictionary<Type, object> ());
		}

		public bool LinkParser<T> (IParserDescriptor<T> descriptor)
		{
			return this.LinkParser (descriptor, new Dictionary<Type, object> ());
		}

		#endregion

		#region Methods / Public / Static

		public static IBuilder<T> CreateBuilder<T> (ISchema<T> schema)
		{
			Linker	linker;
			string	message;
			Type	type;

			linker = new Linker ();
			linker.Error += (t, m) =>
			{
				message = string.Empty;
				type = t;
			};

			message = "unknown";
			type = typeof (T);

			if (!linker.LinkBuilder (schema.BuilderDescriptor))
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture, "can't link builder for type '{0}' (error with type '{1}', {2})", typeof (T), type, message), "schema");

			return schema.CreateBuilder ();
		}

		public static IParser<T> CreateParser<T> (ISchema<T> schema)
		{
			Linker	linker;
			string	message;
			Type	type;

			linker = new Linker ();
			linker.Error += (t, m) =>
			{
				message = string.Empty;
				type = t;
			};

			message = "unknown";
			type = typeof (T);

			if (!linker.LinkParser (schema.ParserDescriptor))
				throw new ArgumentException (string.Format (CultureInfo.InvariantCulture, "can't link parser for type '{0}' (error with type '{1}', {2})", typeof (T), type, message), "schema");

			return schema.CreateParser ();
		}

		#endregion

		#region Methods / Private / Instance

		private bool LinkBuilder<T> (IBuilderDescriptor<T> descriptor, Dictionary<Type, object> parents)
		{
			object		access;
			Type[]		arguments;
			TypeFilter	filter;
			Type		inner;
			Type		type;

			try
			{
				descriptor.IsValue ();

				return true;
			}
			catch (InvalidCastException) // FIXME: hack
			{
			}

			type = typeof (T);

			parents = new Dictionary<Type, object> (parents);
			parents[type] = descriptor;

			// Target type is an array
			if (type.IsArray)
			{
				inner = type.GetElementType ();
				access = Linker.MakeAccessArray (inner);
 
				return this.LinkBuilderArray (descriptor, inner, access, parents);
			}

			// Target type implements IEnumerable<> interface
			filter = new TypeFilter ((t, c) => t.IsGenericType && t.GetGenericTypeDefinition () == typeof (IEnumerable<>));

			foreach (Type iface in type.FindInterfaces (filter, null))
			{
				arguments = iface.GetGenericArguments ();

				// Found interface, inner elements type is "inner"
				if (arguments.Length == 1)
				{
					inner = arguments[0];
					access = Linker.MakeAccessArray (inner);

					return this.LinkBuilderArray (descriptor, inner, access, parents);
				}
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || property.Attributes.HasFlag (PropertyAttributes.SpecialName))
					continue;

				access = Linker.MakeAccessField (property);

				if (!this.LinkBuilderField (descriptor, property.PropertyType, property.Name, access, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag (FieldAttributes.SpecialName))
					continue;

				access = Linker.MakeAccessField (field);

				if (!this.LinkBuilderField (descriptor, field.FieldType, field.Name, access, parents))
					return false;
			}

			return true;
		}

		private bool LinkBuilderArray<T> (IBuilderDescriptor<T> descriptor, Type type, object access, IDictionary<Type, object> parents)
		{
			object	recurse;
			object	result;

			if (parents.TryGetValue (type, out recurse))
			{
				Resolver
					.Method<Func<IBuilderDescriptor<T>, Func<T, IEnumerable<object>>, IBuilderDescriptor<object>, IBuilderDescriptor<object>>> ((d, a, p) => d.IsArray (a, p), null, new [] {type})
					.Invoke (descriptor, new [] {access, recurse});
			}
			else
			{
				recurse = Resolver
					.Method<Func<IBuilderDescriptor<T>, Func<T, IEnumerable<object>>, IBuilderDescriptor<object>>> ((d, a) => d.IsArray (a), null, new [] {type})
					.Invoke (descriptor, new [] {access});

				result = Resolver
					.Method<Func<Linker, IBuilderDescriptor<object>, Dictionary<Type, object>, bool>> ((l, d, p) => l.LinkBuilder (d, p), null, new [] {type})
					.Invoke (this, new object[] {recurse, parents});

				if (!(result is bool))
					throw new InvalidOperationException ("internal error");

				if (!(bool)result)
					return false;
			}

			return true;
		}

		private bool LinkBuilderField<T> (IBuilderDescriptor<T> descriptor, Type type, string name, object access, IDictionary<Type, object> parents)
		{
			object	recurse;
			object	result;

			if (parents.TryGetValue (type, out recurse))
			{
				Resolver
					.Method<Func<IBuilderDescriptor<T>, string, Func<T, object>, IBuilderDescriptor<object>, IBuilderDescriptor<object>>> ((d, n, a, p) => d.HasField (n, a, p), null, new [] {type})
					.Invoke (descriptor, new object[] {name, access, recurse});
			}
			else
			{
				recurse = Resolver
					.Method<Func<IBuilderDescriptor<T>, string, Func<T, object>, IBuilderDescriptor<object>>> ((d, n, a) => d.HasField (n, a), null, new [] {type})
					.Invoke (descriptor, new object[] {name, access});

				result = Resolver
					.Method<Func<Linker, IBuilderDescriptor<object>, Dictionary<Type, object>, bool>> ((l, d, p) => l.LinkBuilder (d, p), null, new [] {type})
					.Invoke (this, new object[] {recurse, parents});

				if (!(result is bool))
					throw new InvalidOperationException ("internal error");

				if (!(bool)result)
					return false;
			}

			return true;
		}

		private bool LinkParser<T> (IParserDescriptor<T> descriptor, Dictionary<Type, object> parents)
		{
			Type[]			arguments;
			object			assign;
			TypeFilter		filter;
			Type			inner;
			ParameterInfo[]	parameters;
			Type			source;
			Type			type;

			try
			{
				descriptor.IsValue ();

				return true;
			}
			catch (InvalidCastException) // FIXME: hack
			{
			}

			type = typeof (T);

			parents = new Dictionary<Type, object> (parents);
			parents[type] = descriptor;

			// Target type is an array
			if (type.IsArray)
			{
				inner = type.GetElementType ();
				assign = Linker.MakeAssignArray (inner);
 
				return this.LinkParserArray (descriptor, inner, assign, parents);
			}

			// Target type implements IEnumerable<> interface
			filter = new TypeFilter ((t, c) => t.IsGenericType && t.GetGenericTypeDefinition () == typeof (IEnumerable<>));

			foreach (Type iface in type.FindInterfaces (filter, null))
			{
				arguments = iface.GetGenericArguments ();

				// Found interface, inner elements type is "inner"
				if (arguments.Length == 1)
				{
					inner = arguments[0];

					// Search constructor compatible with IEnumerable<>
					foreach (ConstructorInfo constructor in type.GetConstructors ())
					{
						parameters = constructor.GetParameters ();

						if (parameters.Length != 1)
							continue;

						source = parameters[0].ParameterType;

						if (!source.IsGenericType || source.GetGenericTypeDefinition () != typeof (IEnumerable<>))
							continue;

						arguments = source.GetGenericArguments ();

						if (arguments.Length != 1 || inner != arguments[0])
							continue;

						assign = Linker.MakeAssignArray (constructor, inner);

						return this.LinkParserArray (descriptor, inner, assign, parents);
					}
				}
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || property.Attributes.HasFlag (PropertyAttributes.SpecialName))
					continue;

				assign = Linker.MakeAssignField (property);

				if (!this.LinkParserField (descriptor, property.PropertyType, property.Name, assign, parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag (FieldAttributes.SpecialName))
					continue;

				assign = Linker.MakeAssignField (field);

				if (!this.LinkParserField (descriptor, field.FieldType, field.Name, assign, parents))
					return false;
			}

			return true;
		}

		private bool LinkParserArray<T> (IParserDescriptor<T> descriptor, Type type, object assign, IDictionary<Type, object> parents)
		{
			object	recurse;
			object	result;

			if (parents.TryGetValue (type, out recurse))
			{
				Resolver
					.Method<Func<IParserDescriptor<T>, ParserAssign<T, IEnumerable<object>>, IParserDescriptor<object>, IParserDescriptor<object>>> ((d, a, p) => d.IsArray (a, p), null, new [] {type})
					.Invoke (descriptor, new [] {assign, recurse});
			}
			else
			{
				recurse = Resolver
					.Method<Func<IParserDescriptor<T>, ParserAssign<T, IEnumerable<object>>, IParserDescriptor<object>>> ((d, a) => d.IsArray (a), null, new [] {type})
					.Invoke (descriptor, new [] {assign});

				result = Resolver
					.Method<Func<Linker, IParserDescriptor<object>, Dictionary<Type, object>, bool>> ((l, d, p) => l.LinkParser (d, p), null, new [] {type})
					.Invoke (this, new object[] {recurse, parents});

				if (!(result is bool))
					throw new InvalidOperationException ("internal error");

				if (!(bool)result)
					return false;
			}

			return true;
		}

		private bool LinkParserField<T> (IParserDescriptor<T> descriptor, Type type, string name, object assign, IDictionary<Type, object> parents)
		{
			object	recurse;
			object	result;

			if (parents.TryGetValue (type, out recurse))
			{
				Resolver
					.Method<Func<IParserDescriptor<T>, string, ParserAssign<T, object>, IParserDescriptor<object>, IParserDescriptor<object>>> ((d, n, a, p) => d.HasField (n, a, p), null, new [] {type})
					.Invoke (descriptor, new object[] {name, assign, recurse});
			}
			else
			{
				recurse = Resolver
					.Method<Func<IParserDescriptor<T>, string, ParserAssign<T, object>, IParserDescriptor<object>>> ((d, n, a) => d.HasField (n, a), null, new [] {type})
					.Invoke (descriptor, new object[] {name, assign});

				result = Resolver
					.Method<Func<Linker, IParserDescriptor<object>, Dictionary<Type, object>, bool>> ((l, d, p) => l.LinkParser (d, p), null, new [] {type})
					.Invoke (this, new object[] {recurse, parents});

				if (!(result is bool))
					throw new InvalidOperationException ("internal error");

				if (!(bool)result)
					return false;
			}

			return true;
		}

		private void OnError (Type type, string message)
		{
			Action<Type, string>	error;

			error = this.Error;

			if (error != null)
				error (type, message);
		}

		#endregion

		#region Methods / Private / Static

		private static object MakeAccessArray (Type inner)
		{
			Type			enumerable;
			ILGenerator		generator;
			DynamicMethod	method;

			enumerable = typeof (IEnumerable<>).MakeGenericType (inner);
			method = new DynamicMethod (string.Empty, enumerable, new [] {enumerable}, inner.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (Func<, >).MakeGenericType (enumerable, enumerable));
		}

		private static object MakeAccessField (FieldInfo field)
		{
			ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, field.FieldType, new [] {field.DeclaringType}, field.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldfld, field);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (Func<, >).MakeGenericType (field.DeclaringType, field.FieldType));
		}

		private static object MakeAccessField (PropertyInfo property)
		{
			ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, property.PropertyType, new [] {property.DeclaringType}, property.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Call, property.GetGetMethod ());
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (Func<, >).MakeGenericType (property.DeclaringType, property.PropertyType));
		}

		/// <summary>
		/// Generate ParserAssign delegate from IEnumerable to any compatible
		/// object, using a constructor taking the IEnumerable as its argument.
		/// </summary>
		/// <param name="constructor">Compatible constructor</param>
		/// <param name="inner">Inner elements type</param>
		/// <returns>ParserAssign delegate</returns>
		private static object MakeAssignArray (ConstructorInfo constructor, Type inner)
		{
			Type			enumerable;
			ILGenerator		generator;
			DynamicMethod	method;

			enumerable = typeof (IEnumerable<>).MakeGenericType (inner);
			method = new DynamicMethod (string.Empty, null, new [] {constructor.DeclaringType.MakeByRefType (), enumerable}, constructor.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Newobj, constructor);

			if (constructor.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Stobj, constructor.DeclaringType);
			else
				generator.Emit (OpCodes.Stind_Ref);

			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (ParserAssign<, >).MakeGenericType (constructor.DeclaringType, enumerable));
		}

		/// <summary>
		/// Generate ParserAssign delegate from IEnumerable to compatible array
		/// type, using Linq Enumerable.ToArray conversion.
		/// </summary>
		/// <param name="inner">Inner elements type</param>
		/// <returns>ParserAssign delegate</returns>
		private static object MakeAssignArray (Type inner)
		{
			MethodInfo		converter;
			Type			enumerable;
			ILGenerator		generator;
			DynamicMethod	method;

			converter = Resolver.Method<Func<IEnumerable<object>, object[]>> ((e) => e.ToArray (), null, new [] {inner});
			enumerable = typeof (IEnumerable<>).MakeGenericType (inner);
			method = new DynamicMethod (string.Empty, null, new [] {inner.MakeArrayType ().MakeByRefType (), enumerable}, converter.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);
			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Call, converter);
			generator.Emit (OpCodes.Stind_Ref);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (ParserAssign<, >).MakeGenericType (inner.MakeArrayType (), enumerable));
		}

		private static object MakeAssignField (FieldInfo field)
		{
			ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, null, new [] {field.DeclaringType.MakeByRefType (), field.FieldType}, field.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);

			if (!field.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldind_Ref);

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Stfld, field);
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (ParserAssign<, >).MakeGenericType (field.DeclaringType, field.FieldType));
		}

		private static object MakeAssignField (PropertyInfo property)
		{
			ILGenerator		generator;
			DynamicMethod	method;

			method = new DynamicMethod (string.Empty, null, new [] {property.DeclaringType.MakeByRefType (), property.PropertyType}, property.Module, true);

			generator = method.GetILGenerator ();
			generator.Emit (OpCodes.Ldarg_0);

			if (!property.DeclaringType.IsValueType)
				generator.Emit (OpCodes.Ldind_Ref);

			generator.Emit (OpCodes.Ldarg_1);
			generator.Emit (OpCodes.Call, property.GetSetMethod ());
			generator.Emit (OpCodes.Ret);

			return method.CreateDelegate (typeof (ParserAssign<, >).MakeGenericType (property.DeclaringType, property.PropertyType));
		}

		#endregion
	}
}
