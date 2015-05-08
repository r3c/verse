using System;
using System.Collections.Generic;
using Verse.PrinterDescriptors.Recurse;

namespace Verse.PrinterDescriptors
{
    internal class RecursePrinterDescriptor<TEntity, TContext, TNative> : AbstractPrinterDescriptor<TEntity>
    {
        #region Attributes

        private readonly Container<TEntity, TContext, TNative> container;

        private readonly IEncoder<TNative> encoder;

        #endregion

        #region Constructors

        public RecursePrinterDescriptor(IEncoder<TNative> encoder)
        {
            this.container = new Container<TEntity, TContext, TNative>();
            this.encoder = encoder;
        }

        #endregion

        #region Methods / Public

        public IPrinter<TEntity> CreatePrinter(IWriter<TContext, TNative> writer)
        {
            return new Printer<TEntity, TContext, TNative>(this.container, writer);
        }

        public override IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IPrinterDescriptor<TField> parent)
        {
            RecursePrinterDescriptor<TField, TContext, TNative> descriptor;

            descriptor = parent as RecursePrinterDescriptor<TField, TContext, TNative>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.HasField(name, access, descriptor);
        }

        public override IPrinterDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access)
        {
            return this.HasField(name, access, new RecursePrinterDescriptor<TField, TContext, TNative>(this.encoder));
        }

        public override IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, IPrinterDescriptor<TElement> parent)
        {
            RecursePrinterDescriptor<TElement, TContext, TNative> descriptor;

            descriptor = parent as RecursePrinterDescriptor<TElement, TContext, TNative>;

            if (descriptor == null)
                throw new ArgumentOutOfRangeException("parent", "incompatible descriptor type");

            return this.IsArray(access, descriptor);
        }

        public override IPrinterDescriptor<TElement> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access)
        {
            return this.IsArray(access, new RecursePrinterDescriptor<TElement, TContext, TNative>(this.encoder));
        }

        public override void IsValue<TValue>(Func<TEntity, TValue> access)
        {
            Converter<TValue, TNative> convert;

            convert = this.encoder.Get<TValue>();

            this.container.value = (source) => convert(access(source));
        }

        #endregion

        #region Methods / Private

        private RecursePrinterDescriptor<TField, TContext, TNative> HasField<TField>(string name, Func<TEntity, TField> access, RecursePrinterDescriptor<TField, TContext, TNative> descriptor)
        {
            Container<TField, TContext, TNative> recurse;

            recurse = descriptor.container;

            this.container.fields[name] = (source, writer, context) => writer.WriteValue(access(source), recurse, context);

            return descriptor;
        }

        private RecursePrinterDescriptor<TElement, TContext, TNative> IsArray<TElement>(Func<TEntity, IEnumerable<TElement>> access, RecursePrinterDescriptor<TElement, TContext, TNative> descriptor)
        {
            Container<TElement, TContext, TNative> recurse;

            if (this.container.items != null)
                throw new InvalidOperationException("can't declare items twice on same descriptor");

            recurse = descriptor.container;

            this.container.items = (source, writer, context) => writer.WriteArray(access(source), recurse, context);

            return descriptor;
        }

        #endregion
    }
}