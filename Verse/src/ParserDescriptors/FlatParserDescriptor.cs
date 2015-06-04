using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Flat;
using Verse.ParserDescriptors.Flat.Nodes;

namespace Verse.ParserDescriptors
{
    internal class FlatParserDescriptor<TEntity, TContext> : AbstractParserDescriptor<TEntity>
    {
        #region Attributes

        private readonly Container<TEntity, TContext, string> container;

        private readonly IDecoder<string> decoder;

        #endregion

        #region Constructors

        public FlatParserDescriptor(IDecoder<string> decoder)
        {
            this.container = new Container<TEntity, TContext, string>();
            this.decoder = decoder;
        }

        #endregion

        #region Methods / Public

        public IParser<TEntity> CreateParser(IReader<TContext, string> reader)
        {
            return new Parser<TEntity, TContext, string>(this.container, reader);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent)
        {
            FlatParserDescriptor<TField, TContext> descriptor;

            descriptor = parent as FlatParserDescriptor<TField, TContext>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

            return this.HasField(name, assign, descriptor);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign)
        {
            return this.HasField(name, assign, new FlatParserDescriptor<TField, TContext>(this.decoder));
        }

        public override IParserDescriptor<TEntity> HasField(string name)
        {
            FlatParserDescriptor<TEntity, TContext> descriptor;
            Container<TEntity, TContext, string> container;

            descriptor = new FlatParserDescriptor<TEntity, TContext>(this.decoder);
            container = descriptor.container;

            this.Connect(name, (ref TEntity target, IReader<TContext, string> reader, TContext context) => reader.ReadValue(ref target, container, context));

            return descriptor;
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, IParserDescriptor<TElement> parent)
        {
            throw new NotImplementedException("array is not supported");
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign)
        {
            throw new NotImplementedException("array is not supported");
        }

        public override void IsValue<TValue>(ParserAssign<TEntity, TValue> assign)
        {
            Converter<string, TValue> convert;

            convert = this.decoder.Get<TValue>();

            this.container.value = (ref TEntity target, string value) => assign(ref target, convert(value));
        }

        #endregion

        #region Methods / Private

        private void Connect(string name, Follow<TEntity, TContext, string> enter)
        {
            BranchNode<TEntity, TContext, string> next;

            next = this.container.fields;

            foreach (char c in name)
                next = next.Connect(c);

            next.enter = enter;
        }

        private IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, FlatParserDescriptor<TField, TContext> descriptor)
        {
            Func<TEntity, TField> constructor;
            Container<TField, TContext, string> container;

            constructor = this.GetConstructor<TField>();
            container = descriptor.container;

            this.Connect(name, (ref TEntity target, IReader<TContext, string> reader, TContext context) =>
            {
                TField inner;

                inner = constructor(target);

                if (!reader.ReadValue(ref inner, container, context))
                    return false;

                assign(ref target, inner);

                return true;
            });

            return descriptor;
        }

        #endregion
    }
}