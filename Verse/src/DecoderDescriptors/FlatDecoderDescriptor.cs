using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Flat;
using Verse.DecoderDescriptors.Flat.Nodes;

namespace Verse.DecoderDescriptors
{
    internal class FlatDecoderDescriptor<TEntity, TContext> : AbstractDecoderDescriptor<TEntity, string>
    {
        #region Attributes

        private readonly Container<TEntity, TContext, string> container;

        #endregion

        #region Constructors

        public FlatDecoderDescriptor(IDecoderConverter<string> converter) :
            base(converter)
        {
            this.container = new Container<TEntity, TContext, string>();
        }

        #endregion

        #region Methods / Public

        public IDecoder<TEntity> CreateDecoder(IReader<TContext, string> reader)
        {
            return new Decoder<TEntity, TContext, string>(this.GetConstructor<TEntity>(), this.container, reader);
        }

        public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
        {
            FlatDecoderDescriptor<TField, TContext> descriptor;

            descriptor = parent as FlatDecoderDescriptor<TField, TContext>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

            return this.HasField(name, assign, descriptor);
        }

        public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
        {
            return this.HasField(name, assign, new FlatDecoderDescriptor<TField, TContext>(this.converter));
        }

        public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent)
        {
            throw new NotImplementedException("array is not supported");
        }

        public override IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign)
        {
            throw new NotImplementedException("array is not supported");
        }

        public override void IsValue<TValue>(DecodeAssign<TEntity, TValue> assign)
        {
            Converter<string, TValue> convert = this.GetConverter<TValue>();

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

        private IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, FlatDecoderDescriptor<TField, TContext> descriptor)
        {
            Func<TField> constructor;
            Container<TField, TContext, string> container;

            constructor = this.GetConstructor<TField>();
            container = descriptor.container;

            this.Connect(name, (ref TEntity target, IReader<TContext, string> reader, TContext context) =>
            {
                TField inner;

                if (!reader.ReadValue(constructor, container, context, out inner))
                    return false;

                assign(ref target, inner);

                return true;
            });

            return descriptor;
        }

        #endregion
    }
}