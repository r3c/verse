using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Abstract;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Nodes;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
    internal class RecurseParserDescriptor<TEntity, TContext, TNative> : AbstractParserDescriptor<TEntity, TNative>
    {
        #region Attributes

        private readonly Container<TEntity, TContext, TNative> container;

        #endregion

        #region Constructors

        public RecurseParserDescriptor(IDecoder<TNative> decoder) :
            base(decoder)
        {
            this.container = new Container<TEntity, TContext, TNative>();
        }

        #endregion

        #region Methods / Public

        public IParser<TEntity> CreateParser(IReader<TContext, TNative> reader)
        {
            return new Parser<TEntity, TContext, TNative>(this.container, reader);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent)
        {
            RecurseParserDescriptor<TField, TContext, TNative> descriptor;

            descriptor = parent as RecurseParserDescriptor<TField, TContext, TNative>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

            return this.HasField(name, assign, descriptor);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign)
        {
            return this.HasField(name, assign, new RecurseParserDescriptor<TField, TContext, TNative>(this.decoder));
        }

        public override IParserDescriptor<TEntity> HasField(string name)
        {
            RecurseParserDescriptor<TEntity, TContext, TNative> descriptor;
            Container<TEntity, TContext, TNative> recurse;

            descriptor = new RecurseParserDescriptor<TEntity, TContext, TNative>(this.decoder);
            recurse = descriptor.container;

            this.ConnectField(name, (ref TEntity target, IReader<TContext, TNative> reader, TContext context) => reader.ReadValue(ref target, recurse, context));

            return descriptor;
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, IParserDescriptor<TElement> parent)
        {
            RecurseParserDescriptor<TElement, TContext, TNative> descriptor;

            descriptor = parent as RecurseParserDescriptor<TElement, TContext, TNative>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.IsArray(assign, descriptor);
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign)
        {
            return this.IsArray(assign, new RecurseParserDescriptor<TElement, TContext, TNative>(this.decoder));
        }

        public override void IsValue<TValue>(ParserAssign<TEntity, TValue> assign)
        {
            Converter<TNative, TValue> convert = this.GetConverter<TValue>();

            this.ConnectValue((ref TEntity target, TNative value) => assign(ref target, convert(value)));
        }

        #endregion

        #region Methods / Private

        private void ConnectField(string name, Follow<TEntity, TContext, TNative> enter)
        {
            BranchNode<TEntity, TContext, TNative> next;

            next = this.container.fields;

            foreach (char c in name)
                next = next.Connect(c);

            next.enter = enter;
        }

        private void ConnectValue(ParserAssign<TEntity, TNative> assign)
        {
            this.container.value = assign;
        }

        private IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, RecurseParserDescriptor<TField, TContext, TNative> descriptor)
        {
            Func<TEntity, TField> constructor;
            Container<TField, TContext, TNative> recurse;

            constructor = this.GetConstructor<TField>();
            recurse = descriptor.container;

            this.ConnectField(name, (ref TEntity target, IReader<TContext, TNative> reader, TContext context) =>
            {
                TField inner;

                inner = constructor(target);

                if (!reader.ReadValue(ref inner, recurse, context))
                    return false;

                assign(ref target, inner);

                return true;
            });

            return descriptor;
        }

        private IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, RecurseParserDescriptor<TElement, TContext, TNative> descriptor)
        {
            Func<TEntity, TElement> constructor;
            Container<TElement, TContext, TNative> recurse;

            if (this.container.items != null)
                throw new InvalidOperationException("can't declare items twice on same descriptor");

            constructor = this.GetConstructor<TElement>();
            recurse = descriptor.container;

            this.container.items = (ref TEntity target, IReader<TContext, TNative> reader, TContext context) =>
            {
                IBrowser<TElement> browser;
                TEntity source;

                source = target;
                browser = reader.ReadArray(() => constructor(source), recurse, context);
                assign(ref target, new Walker<TElement>(browser));

                while (browser.MoveNext())
                    ;

                return browser.Success;
            };

            return descriptor;
        }

        #endregion
    }
}