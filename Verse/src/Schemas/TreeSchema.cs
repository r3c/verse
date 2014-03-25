using System;
using System.IO;
using Verse.Descriptors;

namespace Verse.Schemas
{
    public abstract class TreeSchema<T, C, V> : AbstractDescriptor<T>, ISchema<T>
    {
        #region Attributes

        private readonly Func<T>                    constructor;

        private readonly TreeDescriptor<T, C, V>    descriptor;

        #endregion

        #region Constructors

        protected TreeSchema(Func<T> constructor)
        {
            this.constructor = constructor;
            this.descriptor = new TreeDescriptor<T, C, V>();
        }

        #endregion

        #region Methods / Abstract

        protected abstract TreeDescriptor<C, V>.IReader Generate();

        #endregion

        #region Methods / Public

        public bool Generate(out IParser<T> parser)
        {
            parser = new TreeParser(this.constructor, this.descriptor.Builder, this.Generate());

            return true;
        }

        public override IDescriptor<U> ForChildren<U>(DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create)
        {
            return this.descriptor.ForChildren(assign, create);
        }

        public override IDescriptor<T> ForChildren()
        {
            return this.descriptor.ForChildren();
        }

        public override IDescriptor<U> ForField<U>(string name, DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create)
        {
            return this.descriptor.ForField(name, assign, create);
        }

        public override IDescriptor<T> ForField(string name)
        {
            return this.descriptor.ForField(name);
        }

        public override void LetValue<U>(DescriptorAssign<T, U> assign)
        {
            this.descriptor.LetValue(assign);
        }

        #endregion

        #region Types

        private class TreeParser : IParser<T>
        {
            private readonly TreeDescriptor<C, V>.IBuilder<T>   builder;

            private readonly Func<T>                            constructor;

            private readonly TreeDescriptor<C, V>.IReader       reader;

            public TreeParser(Func<T> constructor, TreeDescriptor<C, V>.IBuilder<T> builder, TreeDescriptor<C, V>.IReader reader)
            {
                this.builder = builder;
                this.constructor = constructor;
                this.reader = reader;
            }

            public bool Parse(Stream input, out T output)
            {
                C   context;

                if (!this.reader.Initialize(input, out context))
                {
                    output = default(T);

                    return false;
                }

                output = this.constructor();

                return this.reader.Read(ref output, this.builder, context);
            }
        }

        #endregion
    }
}
