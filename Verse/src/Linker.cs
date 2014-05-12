using System;
using System.Collections.Generic;
using System.Globalization;
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
			throw new NotImplementedException ();
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

		private bool LinkParser<T> (IParserDescriptor<T> descriptor, Dictionary<Type, object> parents)
		{
			Type[]			arguments;
			ConstructorInfo	constructor;
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

			// Check if target type is or implements IEnumerable<> interface
			if (type.IsGenericType && type.GetGenericTypeDefinition () == typeof (IEnumerable<>))
			{
				arguments = type.GetGenericArguments ();
				inner = arguments.Length == 1 ? arguments[0] : null;
			}
			else
			{
				filter = new TypeFilter ((t, c) => t.IsGenericType && t.GetGenericTypeDefinition () == typeof (IEnumerable<>));
				inner = null;

				foreach (Type candidate in type.FindInterfaces (filter, null))
				{
					arguments = candidate.GetGenericArguments ();

					if (arguments.Length == 1)
					{
						inner = arguments[0];

						break;
					}
				}
			}

			// Target is an IEnumerable<> of element type "inner"
			if (inner != null)
			{
				constructor = null;

				foreach (ConstructorInfo candidate in type.GetConstructors ())
				{
					parameters = candidate.GetParameters ();

					if (parameters.Length != 1)
						continue;

					source = parameters[0].ParameterType;

					if (source.GetGenericTypeDefinition () != typeof (IEnumerable<>))
						continue;

					arguments = source.GetGenericArguments ();

					if (arguments.Length != 1 || inner != arguments[0])
						continue;

					constructor = candidate;

					break;
				}

				if (constructor == null)
				{
					this.OnError (type, "can't find compatible constructor");

					return false;
				}

				// this.LinkElements (decoders, inner, AbstractDecoder<T>.MakeArraySetter (target, inner));

				return false;
			}

			// Link public readable and writable instance properties
			foreach (PropertyInfo property in type.GetProperties (BindingFlags.Instance | BindingFlags.Public))
			{
				if (property.GetGetMethod () == null || property.GetSetMethod () == null || property.Attributes.HasFlag (PropertyAttributes.SpecialName))
					continue;

				if (!this.LinkParserField (descriptor, property.PropertyType, property.Name, Linker.MakeAssign (property), parents))
					return false;
			}

			// Link public instance fields
			foreach (FieldInfo field in type.GetFields (BindingFlags.Instance | BindingFlags.Public))
			{
				if (field.Attributes.HasFlag (FieldAttributes.SpecialName))
					continue;

				if (!this.LinkParserField (descriptor, field.FieldType, field.Name, Linker.MakeAssign (field), parents))
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

		private static object MakeAssign (FieldInfo field)
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

		private static object MakeAssign (PropertyInfo property)
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
