using System;
using System.Collections.Generic;
using Verse.PrinterDescriptors.Abstract;
using Verse.PrinterDescriptors.Recurse;

namespace Verse.PrinterDescriptors
{
    class RecursePrinterDescriptor<TEntity, TValue, TState> : AbstractPrinterDescriptor<TEntity, TValue>
    {
        #region Attributes

        private readonly IWriter<TEntity, TValue, TState> writer;

        #endregion

        #region Constructors

        public RecursePrinterDescriptor(IEncoder<TValue> encoder, IWriter<TEntity, TValue, TState> writer) :
            base(encoder)
        {
            this.writer = writer;
        }

        #endregion

        #region Methods / Public

        public IPrinter<TEntity> CreatePrinter()
        {
            return new Printer<TEntity, TValue, TState>(this.writer);
        }

        public override IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IPrinterDescriptor<TField> parent)
        {
            var descriptor = parent as RecursePrinterDescriptor<TField, TValue, TState>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.HasField(name, access, descriptor);
        }

        public override IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
        {
            return this.HasField(name, access, new RecursePrinterDescriptor<TField, TValue, TState>(this.encoder, this.writer.Create<TField>()));
        }

        public override IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IPrinterDescriptor<TElement> parent)
        {
            var descriptor = parent as RecursePrinterDescriptor<TElement, TValue, TState>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.IsArray(access, descriptor);
        }

        public override IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access)
        {
            return this.IsArray(access, new RecursePrinterDescriptor<TElement, TValue, TState>(this.encoder, this.writer.Create<TElement>()));
        }

        public override void IsValue<TRaw>(Func<TEntity, TRaw> access)
        {
            Converter<TRaw, TValue> convert = this.GetConverter<TRaw>();

            this.writer.DeclareValue((source) => convert(access(source)));
        }

        #endregion

        #region Methods / Private

        private RecursePrinterDescriptor<TField, TValue, TState> HasField<TField>(string name, Func<TEntity, TField> access, RecursePrinterDescriptor<TField, TValue, TState> descriptor)
        {
            var recurse = descriptor.writer;

            this.writer.DeclareField(name, (source, state) => recurse.WriteValue(access(source), state));

            return descriptor;
        }

        private RecursePrinterDescriptor<TElement, TValue, TState> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, RecursePrinterDescriptor<TElement, TValue, TState> descriptor)
        {
            var recurse = descriptor.writer;

            this.writer.DeclareArray((source, state) => recurse.WriteArray(access(source), state));

            return descriptor;
        }

        #endregion
    }
}