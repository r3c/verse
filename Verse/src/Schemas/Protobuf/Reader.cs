using System;
using System.Globalization;
using System.IO;

using ProtoBuf;

using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Readers;
using Verse.ParserDescriptors.Recurse.Readers.Pattern;

namespace Verse.Schemas.Protobuf
{
    class Reader<TEntity> : PatternReader<TEntity, Value, ReaderState>
    {
        #region Attributes

        private static readonly Reader<TEntity> unknown = new Reader<TEntity>();

        #endregion

        #region Methods / Public

        public override IReader<TOther, Value, ReaderState> Create<TOther>()
        {
            return new Reader<TOther>();
        }

        public override IBrowser<TEntity> ReadElements(Func<TEntity> constructor, ReaderState state)
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

        public override bool ReadEntity(ref TEntity target, ReaderState state)
        {
            switch (state.ReadingAction)
            {
                case ReaderState.ReadingActionType.UseHeader:
                    this.FollowNode(state.Reader.FieldNumber, ref target, state);

                    break;

                case ReaderState.ReadingActionType.ReadHeader:
                    int fieldIndex;

                    while (state.ReadHeader(out fieldIndex))
                    {
                        state.AddObject(fieldIndex);

                        this.FollowNode(fieldIndex, ref target, state);
                    }

                    break;

                default:
                    state.ReadingAction = ReaderState.ReadingActionType.ReadHeader;

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

        private bool ReadObjectValue(ref TEntity target, ReaderState state)
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
                INode<TEntity, Value, ReaderState> node;

                state.ReadHeader(out fieldIndex);
                state.AddObject(fieldIndex);

                node = Reader<TEntity>.GetNode(this.RootNode, fieldIndex);

                if (visitCount == 1 && node.IsConnected)
                {
                    state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

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

                    state.ReadingAction = ReaderState.ReadingActionType.UseHeader;

                    if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                        return false;
                }
            }

            state.LeaveObject();

            ProtoReader.EndSubItem(lastSubItem, state.Reader);

            return true;
        }

        public override bool Start(Stream stream, ParserError onError, out ReaderState state)
        {
            state = new ReaderState(stream, onError);

            return true;
        }

        public override void Stop(ReaderState state)
        {
        }

        #endregion

        #region Methods / Private

        private void FollowNode(int fieldIndex, ref TEntity target, ReaderState state)
        {
            INode<TEntity, Value, ReaderState> node;

            node = Reader<TEntity>.GetNode(this.RootNode, fieldIndex);

            state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

            node.Enter(ref target, Reader<TEntity>.unknown, state);
        }

        private IBrowser<TEntity> ReadSubItemArray(
            Func<TEntity> constructor,
            ReaderState state)
        {
            SubItemToken lastSubItem;
            BrowserMove<TEntity> move;

            if (this.HoldValue)
                return this.ReadValueArray(constructor, state);

            state.ReadingAction = ReaderState.ReadingActionType.ReadHeader;

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

                if (!this.ReadEntity(ref current, state))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };

            return new Browser<TEntity>(move);
        }

        private IBrowser<TEntity> ReadValueArray(
            Func<TEntity> constructor,
            ReaderState state)
        {
            BrowserMove<TEntity> move;

            state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

            bool first = true;

            move = (int index, out TEntity current) =>
            {
                current = constructor();
                state.AddObject(index);

                if (first)
                {
                    if (!this.ReadEntity(ref current, state))
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

        private static INode<TEntity, Value, ReaderState> GetNode(
            INode<TEntity, Value, ReaderState> rootNode,
            int fieldIndex)
        {
            INode<TEntity, Value, ReaderState> node;

            node = rootNode.Follow('_');

            foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
                node = node.Follow(digit);

            return node;
        }

        #endregion
    }

}
