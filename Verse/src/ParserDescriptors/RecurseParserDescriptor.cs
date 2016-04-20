using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Abstract;
using Verse.ParserDescriptors.Recurse;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
    class RecurseParserDescriptor<TEntity, TValue, TState> : AbstractParserDescriptor<TEntity, TValue>
    {
        #region Attributes

        private readonly IReader<TEntity, TValue, TState> reader;

        #endregion

        #region Constructors

        public RecurseParserDescriptor(IDecoder<TValue> decoder, IReader<TEntity, TValue, TState> reader) :
            base(decoder)
        {
            this.reader = reader;
        }

        #endregion

        #region Methods / Public

        public IParser<TEntity> CreateParser()
        {
            return new Parser<TEntity, TValue, TState>(this.reader);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, IParserDescriptor<TField> parent)
        {
            var descriptor = parent as RecurseParserDescriptor<TField, TValue, TState>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "invalid target descriptor type");

            return this.HasField(name, assign, descriptor);
        }

        public override IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign)
        {
            return this.HasField(name, assign, new RecurseParserDescriptor<TField, TValue, TState>(this.decoder, this.reader.Create<TField>()));
        }

        public override IParserDescriptor<TEntity> HasField(string name)
        {
            var descriptor = new RecurseParserDescriptor<TEntity, TValue, TState>(this.decoder, this.reader.Create<TEntity>());
            var recurse = descriptor.reader;

            this.reader.DeclareField(name, (ref TEntity target, TState state) => recurse.ReadValue(ref target, state));

            return descriptor;
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, IParserDescriptor<TElement> parent)
        {
            var descriptor = parent as RecurseParserDescriptor<TElement, TValue, TState>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.IsArray(assign, descriptor);
        }

        public override IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign)
        {
            return this.IsArray(assign, new RecurseParserDescriptor<TElement, TValue, TState>(this.decoder, this.reader.Create<TElement>()));
        }

        public override void IsValue<TRaw>(ParserAssign<TEntity, TRaw> assign)
        {
            Converter<TValue, TRaw> convert = this.GetConverter<TRaw>();

            this.reader.DeclareValue((ref TEntity target, TValue value) => assign(ref target, convert(value)));
        }

        #endregion

        #region Methods / Private

        private IParserDescriptor<TField> HasField<TField>(string name, ParserAssign<TEntity, TField> assign, RecurseParserDescriptor<TField, TValue, TState> descriptor)
        {
            var constructor = this.GetConstructor<TField>();
            var recurse = descriptor.reader;

            this.reader.DeclareField(name, (ref TEntity target, TState state) =>
            {
                TField field = constructor(target);

                if (!recurse.ReadValue(ref field, state))
                    return false;

                assign(ref target, field);

                return true;
            });

            return descriptor;
        }

        private IParserDescriptor<TElement> IsArray<TElement>(ParserAssign<TEntity, IEnumerable<TElement>> assign, RecurseParserDescriptor<TElement, TValue, TState> descriptor)
        {
            var constructor = this.GetConstructor<TElement>();
            var recurse = descriptor.reader;

            this.reader.DeclareArray((ref TEntity target, TState state) =>
            {
                IBrowser<TElement> browser;
                TEntity source;

                source = target;
                browser = recurse.ReadArray(() => constructor(source), state);
                assign(ref target, new Walker<TElement>(browser));

                while (browser.MoveNext())
                    ;

                return browser.Success;
            });

            return descriptor;
        }

        #endregion
    }
}