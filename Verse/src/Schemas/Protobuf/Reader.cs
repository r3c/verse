using System;
using System.Globalization;
using ProtoBuf;
using Verse.DecoderDescriptors.Recurse;
using Verse.DecoderDescriptors.Recurse.Readers;
using Verse.DecoderDescriptors.Recurse.Readers.Pattern;

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

		public override BrowserMove<TEntity> ReadElements(Func<TEntity> constructor, ReaderState state)
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

		public override bool ReadEntity(Func<TEntity> constructor, ReaderState state, out TEntity entity)
		{
			int fieldIndex;

			switch (state.ReadingAction)
			{
				case ReaderState.ReadingActionType.UseHeader:
					entity = constructor();

					this.FollowNode(state.Reader.FieldNumber, ref entity, state);

					return true;

				case ReaderState.ReadingActionType.ReadHeader:
					entity = constructor();

					while (state.ReadHeader(out fieldIndex))
					{
						state.AddObject(fieldIndex);

						this.FollowNode(fieldIndex, ref entity, state);
					}

					return true;

				default:
					state.ReadingAction = ReaderState.ReadingActionType.ReadHeader;

					if (this.IsArray)
						return this.ProcessArray(constructor, state, out entity);

					if (!this.IsValue)
					{
						entity = constructor();

						// if it's not object, ignore
						if ((state.Reader.WireType != WireType.StartGroup && state.Reader.WireType != WireType.String) ||
							!this.RootNode.HasSubNode)
						{
							state.Reader.SkipField();

							return true;
						}

						return this.ReadObjectValue(ref entity, state);
					}

					switch (state.Reader.WireType)
					{
						case WireType.Fixed32:
							entity = this.ProcessValue(new Value(state.Reader.ReadSingle()));

							return true;

						case WireType.Fixed64:
							entity = this.ProcessValue(new Value(state.Reader.ReadDouble()));

							return true;

						case WireType.String:
							entity = this.ProcessValue(new Value(state.Reader.ReadString()));

							return true;

						case WireType.Variant:
							entity = this.ProcessValue(new Value(state.Reader.ReadInt64()));

							return true;
					}

					state.Error(state.Position, "wire type not supported, skipped");
					state.Reader.SkipField();

					entity = default(TEntity);

					return true;
			}
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

		private bool ReadObjectValue(ref TEntity target, ReaderState state)
		{
			int visitCount;
			SubItemToken lastSubItem;

			lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out visitCount))
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

		private BrowserMove<TEntity> ReadSubItemArray(Func<TEntity> constructor, ReaderState state)
		{
			int dummy;
			SubItemToken lastSubItem;

			if (this.IsValue)
				return this.ReadValueArray(constructor, state);

			state.ReadingAction = ReaderState.ReadingActionType.ReadHeader;

			lastSubItem = ProtoReader.StartSubItem(state.Reader);

			if (!state.EnterObject(out dummy))
			{
				return (int index, out TEntity current) =>
				{
					current = default(TEntity);
					return BrowserState.Failure;
				};
			}

			return (int index, out TEntity current) =>
			{
				state.AddObject(index);

				if (!ProtoReader.HasSubValue(WireType.None, state.Reader))
				{
					current = default(TEntity);

					state.LeaveObject();

					ProtoReader.EndSubItem(lastSubItem, state.Reader);

					return BrowserState.Success;
				}

				if (this.ReadEntity(constructor, state, out current))
					return BrowserState.Continue;

				current = default(TEntity);

				return BrowserState.Failure;
			};
		}

		private BrowserMove<TEntity> ReadValueArray(Func<TEntity> constructor, ReaderState state)
		{
			state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

			return (int index, out TEntity current) =>
			{
				state.AddObject(index);

				if (index > 0)
				{
					current = default(TEntity);

					return BrowserState.Success;
				}

				if (!this.ReadEntity(constructor, state, out current))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		private static INode<TEntity, Value, ReaderState> GetNode(INode<TEntity, Value, ReaderState> rootNode, int fieldIndex)
		{
			INode<TEntity, Value, ReaderState> node = rootNode.Follow('_');

			foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
				node = node.Follow(digit);

			return node;
		}

		#endregion
	}
}
