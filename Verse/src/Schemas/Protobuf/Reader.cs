using System;
using System.Globalization;
using System.IO;

using ProtoBuf;

using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Readers;
using Verse.ParserDescriptors.Recurse.Readers.String;

namespace Verse.Schemas.Protobuf
{
    class Reader<TEntity> : StringReader<TEntity, Value, ReaderContext>
    {
        #region Attributes

        private static readonly Reader<TEntity> unknown = new Reader<TEntity>();

        #endregion

        #region Methods / Public

        public override IReader<TOther, Value, ReaderContext> Create<TOther>()
        {
            return new Reader<TOther>();
        }

        public override IBrowser<TEntity> ReadArray(Func<TEntity> constructor, ReaderContext state)
        {
            switch (state.Reader.WireType)
            {
                case WireType.StartGroup:
                case WireType.String:
                    return this.ReadSubItemArray(constructor, state);

                default:
                    return this.ReadValueArray(constructor, state);
            }
        }

        public override bool ReadValue(ref TEntity target, ReaderContext state)
        {
            switch (state.ReadingAction)
            {
                case ReaderContext.ReadingActionType.UseHeader:
                    this.FollowNode(state.Reader.FieldNumber, ref target, state);

                    break;

                case ReaderContext.ReadingActionType.ReadHeader:
                    int fieldIndex;

                    while (state.ReadHeader(out fieldIndex))
                    {
                        state.AddObject(fieldIndex);

                        this.FollowNode(fieldIndex, ref target, state);
                    }

                    break;

                default:
                    state.ReadingAction = ReaderContext.ReadingActionType.ReadHeader;

                    if (this.HoldArray)
                        return this.ProcessArray(ref target, state);

                    if (!this.HoldValue)
                    {
                        // if it's not object, ignore
                        if ((state.Reader.WireType != WireType.StartGroup && state.Reader.WireType != WireType.String) ||
                            !this.RootNode.HasSubNode)
                        {
                            state.Reader.SkipField();

                            return true;
                        }

                        return this.ReadObjectValue(ref target, state);
                    }

                    switch (state.Reader.WireType)
                    {
                        case WireType.Fixed32:
                            this.ProcessValue(ref target, new Value(state.Reader.ReadSingle()));
                            break;

                        case WireType.Fixed64:
                            this.ProcessValue(ref target, new Value(state.Reader.ReadDouble()));
                            break;

                        case WireType.String:
                            this.ProcessValue(ref target, new Value(state.Reader.ReadString()));
                            break;

                        case WireType.Variant:
                            this.ProcessValue(ref target, new Value(state.Reader.ReadInt64()));
                            break;

                        default:
                            state.OnError(state.Position, "wire type not supported, skipped");
                            state.Reader.SkipField();

                            break;
                    }

                    break;
            }

            return true;
        }

        private bool ReadObjectValue(ref TEntity target, ReaderContext state)
        {
            int visitCount;
            SubItemToken lastSubItem;

            lastSubItem = ProtoReader.StartSubItem(state.Reader);
            visitCount = state.VisitCount();

            if (visitCount == -1)
                return false;

            if (!state.EnterObject())
                return false;

            while (ProtoReader.HasSubValue(WireType.None, state.Reader))
            {
                int fieldIndex;
                INode<TEntity, Value, ReaderContext> node;

                state.ReadHeader(out fieldIndex);
                state.AddObject(fieldIndex);

                node = Reader<TEntity>.GetNode(this.RootNode, fieldIndex);

                if (visitCount == 1 && node.IsConnected)
                {
                    state.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

                    if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                        return false;
                }
                else
                {
                    int index;

                    node = this.RootNode;
                    index = visitCount - 1;

                    foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
                        node = node.Follow(digit);

                    state.ReadingAction = ReaderContext.ReadingActionType.UseHeader;

                    if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                        return false;
                }
            }

            state.LeaveObject();

            ProtoReader.EndSubItem(lastSubItem, state.Reader);

            return true;
        }

        public override bool Start(Stream stream, ParserError onError, out ReaderContext state)
        {
            state = new ReaderContext(stream, onError);

            return true;
        }

        public override void Stop(ReaderContext state)
        {
        }

        #endregion

        #region Methods / Private

        private void FollowNode(int fieldIndex, ref TEntity target, ReaderContext state)
        {
            INode<TEntity, Value, ReaderContext> node;

            node = Reader<TEntity>.GetNode(this.RootNode, fieldIndex);

            state.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

            node.Enter(ref target, Reader<TEntity>.unknown, state);
        }

        private IBrowser<TEntity> ReadSubItemArray(
            Func<TEntity> constructor,
            ReaderContext state)
        {
            SubItemToken lastSubItem;
            BrowserMove<TEntity> move;

            if (this.HoldValue)
                return this.ReadValueArray(constructor, state);

            state.ReadingAction = ReaderContext.ReadingActionType.ReadHeader;

            lastSubItem = ProtoReader.StartSubItem(state.Reader);

            if (!state.EnterObject())
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

                state.AddObject(index);

                if (!ProtoReader.HasSubValue(WireType.None, state.Reader))
                {
                    state.LeaveObject();

                    ProtoReader.EndSubItem(lastSubItem, state.Reader);

                    return BrowserState.Success;
                }

                if (!this.ReadValue(ref current, state))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };

            return new Browser<TEntity>(move);
        }

        private IBrowser<TEntity> ReadValueArray(
            Func<TEntity> constructor,
            ReaderContext state)
        {
            BrowserMove<TEntity> move;

            state.ReadingAction = ReaderContext.ReadingActionType.ReadValue;

            bool first = true;

            move = (int index, out TEntity current) =>
            {
                current = constructor();
                state.AddObject(index);

                if (first)
                {
                    if (!this.ReadValue(ref current, state))
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

        private static INode<TEntity, Value, ReaderContext> GetNode(
            INode<TEntity, Value, ReaderContext> rootNode,
            int fieldIndex)
        {
            INode<TEntity, Value, ReaderContext> node;

            node = rootNode.Follow('_');

            foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
                node = node.Follow(digit);

            return node;
        }

        #endregion
    }

}
