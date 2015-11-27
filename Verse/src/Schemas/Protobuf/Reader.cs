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
            switch (context.ReadingAction)
            {
                case ReaderContext.ReadingActionType.UseHeader:
                    this.FollowNode(context.Reader.FieldNumber, ref target, container, context);

                    break;

                case ReaderContext.ReadingActionType.ReadHeader:
                    int fieldIndex;

                    while (context.ReadHeader(out fieldIndex))
                    {
                        context.AddObject(fieldIndex);

                        this.FollowNode(fieldIndex, ref target, container, context);
                    }

                    break;

                default:
                    context.ReadingAction = ReaderContext.ReadingActionType.ReadHeader;

                    if (container.items != null)
                        return container.items(ref target, this, context);

                    if (container.value == null)
                    {
                        // if it's not object, ignore
                        if ((context.Reader.WireType != WireType.StartGroup && context.Reader.WireType != WireType.String) ||
                            !container.fields.HasSubNode)
                        {
                            context.Reader.SkipField();

                            return true;
                        }

                        return this.ReadObjectValue(ref target, container, context);
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

                    break;
            }

            return true;
        }

        private bool ReadObjectValue<TEntity>(ref TEntity target, Container<TEntity, ReaderContext, Value> container, ReaderContext context)
        {
            int visitCount;
            SubItemToken lastSubItem;

            lastSubItem = ProtoReader.StartSubItem(context.Reader);
            visitCount = context.VisitCount();

            if (visitCount == -1)
                return false;

            if (!context.EnterObject())
                return false;

            while (ProtoReader.HasSubValue(WireType.None, context.Reader))
            {
                int fieldIndex;
                INode<TEntity, ReaderContext, Value> node;

                context.ReadHeader(out fieldIndex);
                context.AddObject(fieldIndex);

                node = Reader.GetNode(container.fields, fieldIndex);

                if (visitCount == 1 && node.IsConnected)
                {
                    context.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

                    if (!node.Enter(ref target, this, context))
                        return false;
                }
                else
                {
                    int index;

                    node = container.fields;
                    index = visitCount - 1;

                    foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
                        node = node.Follow(digit);

                    context.ReadingAction = ReaderContext.ReadingActionType.UseHeader;

                    if (!node.Enter(ref target, this, context))
                        return false;
                }
            }

            context.LeaveObject();

            ProtoReader.EndSubItem(lastSubItem, context.Reader);

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

        private void FollowNode<TEntity>(int fieldIndex, ref TEntity target, Container<TEntity, ReaderContext, Value> container, ReaderContext context)
        {
            INode<TEntity, ReaderContext, Value> node;

            node = Reader.GetNode(container.fields, fieldIndex);

            context.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

            node.Enter(ref target, this, context);
        }

        private IBrowser<TEntity> ReadSubItemArray<TEntity>(
            Func<TEntity> constructor,
            Container<TEntity, ReaderContext, Value> container,
            ReaderContext context)
        {
            SubItemToken lastSubItem;
            BrowserMove<TEntity> move;

            if (container.value != null)
                return this.ReadValueArray(constructor, container, context);

            context.ReadingAction = ReaderContext.ReadingActionType.ReadHeader;

            lastSubItem = ProtoReader.StartSubItem(context.Reader);

            if (!context.EnterObject())
            {
                return new Browser<TEntity>((int index, out TEntity current) =>
                {
                    current = default(TEntity);
                    return BrowserState.Failure;
                });
            }

            move = (int index, out TEntity current) =>
            {
                current = constructor();

                context.AddObject(index);

                if (!ProtoReader.HasSubValue(WireType.None, context.Reader))
                {
                    context.LeaveObject();

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

            context.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

            bool first = true;

            move = (int index, out TEntity current) =>
            {
                current = constructor();
                context.AddObject(index);

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

        private static INode<TEntity, ReaderContext, Value> GetNode<TEntity>(
            INode<TEntity, ReaderContext, Value> rootNode,
            int fieldIndex)
        {
            INode<TEntity, ReaderContext, Value> node;

            node = rootNode.Follow('_');

            foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
                node = node.Follow(digit);

            return node;
        }

        #endregion
    }

}
