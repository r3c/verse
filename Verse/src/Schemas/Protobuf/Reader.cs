using System;
using System.Globalization;
using System.IO;
using ProtoBuf;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
{
    internal class Reader : IReader<ReaderContext, Value>
    {
        #region Events

        public event ParserError Error;

        #endregion

        #region Methods / Public

        public IBrowser<TEntity> ReadArray<TEntity>(
            Func<TEntity> constructor,
            Container<TEntity, ReaderContext, Value> container,
            ReaderContext context)
        {
            switch (context.Reader.WireType)
            {
                case WireType.StartGroup:
                case WireType.String:
                    return this.ReadSubItemArray(constructor, container, context);

                default:
                    return this.ReadValueArray(constructor, container, context);
            }
        }

        public bool ReadValue<TEntity>(ref TEntity target, Container<TEntity, ReaderContext, Value> container, ReaderContext context)
        {
            if (context.HeaderToRead)
            {
                int fieldIndex;

                while (context.ReadHeader(out fieldIndex))
                {
                    INode<TEntity, ReaderContext, Value> node;

                    node = container.fields;

                    foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
                        node = node.Follow(digit);

                    context.HeaderToRead = false;

                    node.Enter(ref target, this, context);
                }
            }
            else
            {
                context.HeaderToRead = true;

                if (container.items != null)
                    return container.items(ref target, this, context);

                if (container.value == null)
                {
                    context.Reader.SkipField();
                    return true;
                }

                switch (context.Reader.WireType)
                {
                    case WireType.Fixed32:
                        container.value(ref target, new Value(context.Reader.ReadSingle()));
                        break;

                    case WireType.Fixed64:
                        container.value(ref target, new Value(context.Reader.ReadDouble()));
                        break;

                    case WireType.String:
                        container.value(ref target, new Value(context.Reader.ReadString()));
                        break;

                    case WireType.Variant:
                        container.value(ref target, new Value(context.Reader.ReadInt64()));
                        break;

                    default:
                        this.OnError(context.Position, "wire type not supported, skipped");

                        context.Reader.SkipField();

                        break;
                }
            }

            return true;
        }

        public bool Start(Stream stream, out ReaderContext context)
        {
            context = new ReaderContext(stream);

            return true;
        }

        public void Stop(ReaderContext context)
        {
        }

        #endregion

        #region Methods / Private

        private IBrowser<TEntity> ReadSubItemArray<TEntity>(
            Func<TEntity> constructor,
            Container<TEntity, ReaderContext, Value> container,
            ReaderContext context)
        {
            SubItemToken lastSubItem;
            BrowserMove<TEntity> move;

            if (container.value != null)
                return this.ReadValueArray(constructor, container, context);

            context.HeaderToRead = true;
            lastSubItem = ProtoReader.StartSubItem(context.Reader);

            move = (int index, out TEntity current) =>
            {
                current = constructor();

                if (!ProtoReader.HasSubValue(WireType.None, context.Reader))
                {
                    ProtoReader.EndSubItem(lastSubItem, context.Reader);

                    return BrowserState.Success;
                }

                if (!this.ReadValue(ref current, container, context))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };

            return new Browser<TEntity>(move);
        }

        private IBrowser<TEntity> ReadValueArray<TEntity>(
            Func<TEntity> constructor,
            Container<TEntity, ReaderContext, Value> container,
            ReaderContext context)
        {
            BrowserMove<TEntity> move;

            context.HeaderToRead = false;
            bool first = true;

            move = (int index, out TEntity current) =>
            {
                current = constructor();

                if (first)
                {
                    if (!this.ReadValue(ref current, container, context))
                        return BrowserState.Failure;

                    first = false;
                }
                else
                {
                    return BrowserState.Success;
                }

                return BrowserState.Continue;
            };

            return new Browser<TEntity>(move);
        }

        private void OnError(int position, string message)
        {
            ParserError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        #endregion
    }

}
