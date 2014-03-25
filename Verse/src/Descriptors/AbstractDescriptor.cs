using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Verse.Descriptors
{
    public abstract class AbstractDescriptor<T> : IDescriptor<T>
    {
        #region Methods / Abstract

        public abstract IDescriptor<U> ForChildren<U>(DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create);

        public abstract IDescriptor<T> ForChildren();

        public abstract IDescriptor<U> ForField<U>(string name, DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create);

        public abstract IDescriptor<T> ForField(string name);

        public abstract void LetValue<U>(DescriptorAssign<T, U> assign);

        #endregion

        #region Methods / Public

        public IDescriptor<U> ForChildren<U>(DescriptorAssign<T, U> assign)
        {
            Func<U> create;

            create = AbstractDescriptor<T>.MakeConstructor<U>();

            return this.ForChildren(assign, (ref T target) => create());
        }

        public IDescriptor<U> ForField<U>(string name, DescriptorAssign<T, U> assign)
        {
            Func<U> create;

            create = AbstractDescriptor<T>.MakeConstructor<U>();

            return this.ForField(name, assign, (ref T target) => create());
        }

        #endregion

        #region Methods / Private

        public static Func<U> MakeConstructor<U>()
        {
            ConstructorInfo	constructor;
            ILGenerator		generator;
            DynamicMethod	method;
            Type			type;

            type = typeof(U);
            constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, Type.DefaultBinder, Type.EmptyTypes, new ParameterModifier[0]);

            if (constructor == null)
                return () => default(U);

            method = new DynamicMethod(string.Empty, type, Type.EmptyTypes, constructor.Module, true);

            generator = method.GetILGenerator();
            generator.Emit(OpCodes.Newobj, constructor);
            generator.Emit(OpCodes.Ret);

            return (Func<U>)method.CreateDelegate(typeof(Func<U>));
        }

        #endregion
    }
}
