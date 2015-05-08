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
    /// declare its fields to a printer or parser descriptor.
    /// </summary>
    public class Linker
    {
        #region Events

        /// <summary>
        /// Binding error occurred.
        /// </summary>
        public event Action<Type, string> Error;

        #endregion

        #region Methods / Public / Instance

        /// <summary>
        /// Scan entity type and declare fields using given parser descriptor.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="descriptor">Parser descriptor</param>
        /// <returns>True if binding succeeded, false otherwise</returns>
        public bool LinkParser<TEntity>(IParserDescriptor<TEntity> descriptor)
        {
            return this.LinkParser(descriptor, new Dictionary<Type, object>());
        }

        /// <summary>
        /// Scan entity type and declare fields using given printer descriptor.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="descriptor">Printer descriptor</param>
        /// <returns>True if binding succeeded, false otherwise</returns>
        public bool LinkPrinter<TEntity>(IPrinterDescriptor<TEntity> descriptor)
        {
            return this.LinkPrinter(descriptor, new Dictionary<Type, object>());
        }

        #endregion

        #region Methods / Public / Static

        /// <summary>
        /// Shorthand method to create a parser from given schema. This is
        /// equivalent to creating a new linker, use it on schema's parser
        /// descriptor and throw exception whenever error event is fired.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="schema">Entity schema</param>
        /// <returns>Entity parser</returns>
        public static IParser<TEntity> CreateParser<TEntity>(ISchema<TEntity> schema)
        {
            Linker linker;
            string message;
            Type type;

            linker = new Linker();
            linker.Error += (t, m) =>
            {
                message = string.Empty;
                type = t;
            };

            message = "unknown";
            type = typeof (TEntity);

            if (!linker.LinkParser(schema.ParserDescriptor))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "can't link parser for type '{0}' (error with type '{1}', {2})", typeof (TEntity), type, message), "schema");

            return schema.CreateParser();
        }

        /// <summary>
        /// Shorthand method to create a printer from given schema. This is
        /// equivalent to creating a new linker, use it on schema's Printer
        /// descriptor and throw exception whenever error event is fired.
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="schema">Entity schema</param>
        /// <returns>Entity Printer</returns>
        public static IPrinter<TEntity> CreatePrinter<TEntity>(ISchema<TEntity> schema)
        {
            Linker linker;
            string message;
            Type type;

            linker = new Linker();
            linker.Error += (t, m) =>
            {
                message = string.Empty;
                type = t;
            };

            message = "unknown";
            type = typeof (TEntity);

            if (!linker.LinkPrinter(schema.PrinterDescriptor))
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "can't link printer for type '{0}' (error with type '{1}', {2})", typeof (TEntity), type, message), "schema");

            return schema.CreatePrinter();
        }

        #endregion

        #region Methods / Private / Instance

        private bool LinkParser<T>(IParserDescriptor<T> descriptor, Dictionary<Type, object> parents)
        {
            Type[] arguments;
            object assign;
            TypeFilter filter;
            Type inner;
            ParameterInfo[] parameters;
            Type source;
            Type type;

            try
            {
                descriptor.IsValue();

                return true;
            }
            catch (InvalidCastException) // FIXME: hack
            {
            }

            type = typeof (T);

            parents = new Dictionary<Type, object>(parents);
            parents[type] = descriptor;

            // Target type is an array
            if (type.IsArray)
            {
                inner = type.GetElementType();
                assign = Linker.MakeAssignArray(inner);

                return this.LinkParserArray(descriptor, inner, assign, parents);
            }

            // Target type implements IEnumerable<> interface
            filter = new TypeFilter((t, c) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IEnumerable<>));

            foreach (Type iface in type.FindInterfaces(filter, null))
            {
                arguments = iface.GetGenericArguments();

                // Found interface, inner elements type is "inner"
                if (arguments.Length == 1)
                {
                    inner = arguments[0];

                    // Search constructor compatible with IEnumerable<>
                    foreach (ConstructorInfo constructor in type.GetConstructors())
                    {
                        parameters = constructor.GetParameters();

                        if (parameters.Length != 1)
                            continue;

                        source = parameters[0].ParameterType;

                        if (!source.IsGenericType || source.GetGenericTypeDefinition() != typeof (IEnumerable<>))
                            continue;

                        arguments = source.GetGenericArguments();

                        if (arguments.Length != 1 || inner != arguments[0])
                            continue;

                        assign = Linker.MakeAssignArray(constructor, inner);

                        return this.LinkParserArray(descriptor, inner, assign, parents);
                    }
                }
            }

            // Link public readable and writable instance properties
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                    continue;

                assign = Linker.MakeAssignField(property);

                if (!this.LinkParserField(descriptor, property.PropertyType, property.Name, assign, parents))
                    return false;
            }

            // Link public instance fields
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                    continue;

                assign = Linker.MakeAssignField(field);

                if (!this.LinkParserField(descriptor, field.FieldType, field.Name, assign, parents))
                    return false;
            }

            return true;
        }

        private bool LinkParserArray<TEntity>(IParserDescriptor<TEntity> descriptor, Type type, object assign, IDictionary<Type, object> parents)
        {
            object recurse;
            object result;

            if (parents.TryGetValue(type, out recurse))
            {
                Resolver
                    .Method<Func<IParserDescriptor<TEntity>, ParserAssign<TEntity, IEnumerable<object>>, IParserDescriptor<object>, IParserDescriptor<object>>>((d, a, p) => d.IsArray(a, p), null, new[] { type })
                    .Invoke(descriptor, new[] { assign, recurse });
            }
            else
            {
                recurse = Resolver
                    .Method<Func<IParserDescriptor<TEntity>, ParserAssign<TEntity, IEnumerable<object>>, IParserDescriptor<object>>>((d, a) => d.IsArray(a), null, new[] { type })
                    .Invoke(descriptor, new[] { assign });

                result = Resolver
                    .Method<Func<Linker, IParserDescriptor<object>, Dictionary<Type, object>, bool>>((l, d, p) => l.LinkParser(d, p), null, new[] { type })
                    .Invoke(this, new object[] { recurse, parents });

                if (!(result is bool))
                    throw new InvalidOperationException("internal error");

                if (!(bool)result)
                    return false;
            }

            return true;
        }

        private bool LinkParserField<TEntity>(IParserDescriptor<TEntity> descriptor, Type type, string name, object assign, IDictionary<Type, object> parents)
        {
            object recurse;
            object result;

            if (parents.TryGetValue(type, out recurse))
            {
                Resolver
                    .Method<Func<IParserDescriptor<TEntity>, string, ParserAssign<TEntity, object>, IParserDescriptor<object>, IParserDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p), null, new[] { type })
                    .Invoke(descriptor, new object[] { name, assign, recurse });
            }
            else
            {
                recurse = Resolver
                    .Method<Func<IParserDescriptor<TEntity>, string, ParserAssign<TEntity, object>, IParserDescriptor<object>>>((d, n, a) => d.HasField(n, a), null, new[] { type })
                    .Invoke(descriptor, new object[] { name, assign });

                result = Resolver
                    .Method<Func<Linker, IParserDescriptor<object>, Dictionary<Type, object>, bool>>((l, d, p) => l.LinkParser(d, p), null, new[] { type })
                    .Invoke(this, new object[] { recurse, parents });

                if (!(result is bool))
                    throw new InvalidOperationException("internal error");

                if (!(bool)result)
                    return false;
            }

            return true;
        }

        private bool LinkPrinter<T>(IPrinterDescriptor<T> descriptor, Dictionary<Type, object> parents)
        {
            object access;
            Type[] arguments;
            TypeFilter filter;
            Type inner;
            Type type;

            try
            {
                descriptor.IsValue();

                return true;
            }
            catch (InvalidCastException) // FIXME: hack
            {
            }

            type = typeof (T);

            parents = new Dictionary<Type, object>(parents);
            parents[type] = descriptor;

            // Target type is an array
            if (type.IsArray)
            {
                inner = type.GetElementType();
                access = Linker.MakeAccessArray(inner);

                return this.LinkPrinterArray(descriptor, inner, access, parents);
            }

            // Target type implements IEnumerable<> interface
            filter = new TypeFilter((t, c) => t.IsGenericType && t.GetGenericTypeDefinition() == typeof (IEnumerable<>));

            foreach (Type iface in type.FindInterfaces(filter, null))
            {
                arguments = iface.GetGenericArguments();

                // Found interface, inner elements type is "inner"
                if (arguments.Length == 1)
                {
                    inner = arguments[0];
                    access = Linker.MakeAccessArray(inner);

                    return this.LinkPrinterArray(descriptor, inner, access, parents);
                }
            }

            // Link public readable and writable instance properties
            foreach (PropertyInfo property in type.GetProperties(BindingFlags.Instance | BindingFlags.Public))
            {
                if (property.GetGetMethod() == null || property.GetSetMethod() == null || property.Attributes.HasFlag(PropertyAttributes.SpecialName))
                    continue;

                access = Linker.MakeAccessField(property);

                if (!this.LinkPrinterField(descriptor, property.PropertyType, property.Name, access, parents))
                    return false;
            }

            // Link public instance fields
            foreach (FieldInfo field in type.GetFields(BindingFlags.Instance | BindingFlags.Public))
            {
                if (field.Attributes.HasFlag(FieldAttributes.SpecialName))
                    continue;

                access = Linker.MakeAccessField(field);

                if (!this.LinkPrinterField(descriptor, field.FieldType, field.Name, access, parents))
                    return false;
            }

            return true;
        }

        private bool LinkPrinterArray<T>(IPrinterDescriptor<T> descriptor, Type type, object access, IDictionary<Type, object> parents)
        {
            object recurse;
            object result;

            if (parents.TryGetValue(type, out recurse))
            {
                Resolver
                    .Method<Func<IPrinterDescriptor<T>, Func<T, IEnumerable<object>>, IPrinterDescriptor<object>, IPrinterDescriptor<object>>>((d, a, p) => d.IsArray(a, p), null, new[] { type })
                    .Invoke(descriptor, new[] { access, recurse });
            }
            else
            {
                recurse = Resolver
                    .Method<Func<IPrinterDescriptor<T>, Func<T, IEnumerable<object>>, IPrinterDescriptor<object>>>((d, a) => d.IsArray(a), null, new[] { type })
                    .Invoke(descriptor, new[] { access });

                result = Resolver
                    .Method<Func<Linker, IPrinterDescriptor<object>, Dictionary<Type, object>, bool>>((l, d, p) => l.LinkPrinter(d, p), null, new[] { type })
                    .Invoke(this, new object[] { recurse, parents });

                if (!(result is bool))
                    throw new InvalidOperationException("internal error");

                if (!(bool)result)
                    return false;
            }

            return true;
        }

        private bool LinkPrinterField<T>(IPrinterDescriptor<T> descriptor, Type type, string name, object access, IDictionary<Type, object> parents)
        {
            object recurse;
            object result;

            if (parents.TryGetValue(type, out recurse))
            {
                Resolver
                    .Method<Func<IPrinterDescriptor<T>, string, Func<T, object>, IPrinterDescriptor<object>, IPrinterDescriptor<object>>>((d, n, a, p) => d.HasField(n, a, p), null, new[] { type })
                    .Invoke(descriptor, new object[] { name, access, recurse });
            }
            else
            {
                recurse = Resolver
                    .Method<Func<IPrinterDescriptor<T>, string, Func<T, object>, IPrinterDescriptor<object>>>((d, n, a) => d.HasField(n, a), null, new[] { type })
                    .Invoke(descriptor, new object[] { name, access });

                result = Resolver
                    .Method<Func<Linker, IPrinterDescriptor<object>, Dictionary<Type, object>, bool>>((l, d, p) => l.LinkPrinter(d, p), null, new[] { type })
                    .Invoke(this, new object[] { recurse, parents });

                if (!(result is bool))
                    throw new InvalidOperationException("internal error");

                if (!(bool)result)
                    return false;
            }

            return true;
        }

        private void OnError(Type type, string message)
        {
            Action<Type, string> error;

            error = this.Error;

            if (error != null)
                error(type, message);
        }

        #endregion

        #region Methods / Private / Static

        private static object MakeAccessArray(Type inner)
        {
            Type enumerable;
            ILGenerator generator;
            DynamicMethod method;

            enumerable = typeof (IEnumerable<>).MakeGenericType(inner);
            method = new DynamicMethod(string.Empty, enumerable, new[] { enumerable }, inner.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (Func<,>).MakeGenericType(enumerable, enumerable));
        }

        private static object MakeAccessField(FieldInfo field)
        {
            ILGenerator generator;
            DynamicMethod method;

            method = new DynamicMethod(string.Empty, field.FieldType, new[] { field.DeclaringType }, field.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, field);
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (Func<,>).MakeGenericType(field.DeclaringType, field.FieldType));
        }

        private static object MakeAccessField(PropertyInfo property)
        {
            ILGenerator generator;
            DynamicMethod method;

            method = new DynamicMethod(string.Empty, property.PropertyType, new[] { property.DeclaringType }, property.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, property.GetGetMethod());
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (Func<,>).MakeGenericType(property.DeclaringType, property.PropertyType));
        }

        /// <summary>
        /// Generate ParserAssign delegate from IEnumerable to any compatible
        /// object, using a constructor taking the IEnumerable as its argument.
        /// </summary>
        /// <param name="constructor">Compatible constructor</param>
        /// <param name="inner">Inner elements type</param>
        /// <returns>ParserAssign delegate</returns>
        private static object MakeAssignArray(ConstructorInfo constructor, Type inner)
        {
            Type enumerable;
            ILGenerator generator;
            DynamicMethod method;

            enumerable = typeof (IEnumerable<>).MakeGenericType(inner);
            method = new DynamicMethod(string.Empty, null, new[] { constructor.DeclaringType.MakeByRefType(), enumerable }, constructor.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Newobj, constructor);

            if (constructor.DeclaringType.IsValueType)
                generator.Emit(OpCodes.Stobj, constructor.DeclaringType);
            else
                generator.Emit(OpCodes.Stind_Ref);

            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (ParserAssign<,>).MakeGenericType(constructor.DeclaringType, enumerable));
        }

        /// <summary>
        /// Generate ParserAssign delegate from IEnumerable to compatible array
        /// type, using Linq Enumerable.ToArray conversion.
        /// </summary>
        /// <param name="inner">Inner elements type</param>
        /// <returns>ParserAssign delegate</returns>
        private static object MakeAssignArray(Type inner)
        {
            MethodInfo converter;
            Type enumerable;
            ILGenerator generator;
            DynamicMethod method;

            converter = Resolver.Method<Func<IEnumerable<object>, object[]>>((e) => e.ToArray(), null, new[] { inner });
            enumerable = typeof (IEnumerable<>).MakeGenericType(inner);
            method = new DynamicMethod(string.Empty, null, new[] { inner.MakeArrayType().MakeByRefType(), enumerable }, converter.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, converter);
            generator.Emit(OpCodes.Stind_Ref);
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (ParserAssign<,>).MakeGenericType(inner.MakeArrayType(), enumerable));
        }

        private static object MakeAssignField(FieldInfo field)
        {
            ILGenerator generator;
            DynamicMethod method;

            method = new DynamicMethod(string.Empty, null, new[] { field.DeclaringType.MakeByRefType(), field.FieldType }, field.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);

            if (!field.DeclaringType.IsValueType)
                generator.Emit(OpCodes.Ldind_Ref);

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, field);
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (ParserAssign<,>).MakeGenericType(field.DeclaringType, field.FieldType));
        }

        private static object MakeAssignField(PropertyInfo property)
        {
            ILGenerator generator;
            DynamicMethod method;

            method = new DynamicMethod(string.Empty, null, new[] { property.DeclaringType.MakeByRefType(), property.PropertyType }, property.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);

            if (!property.DeclaringType.IsValueType)
                generator.Emit(OpCodes.Ldind_Ref);

            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, property.GetSetMethod());
            generator.Emit(OpCodes.Ret);

            return method.CreateDelegate(typeof (ParserAssign<,>).MakeGenericType(property.DeclaringType, property.PropertyType));
        }

        #endregion
    }
}