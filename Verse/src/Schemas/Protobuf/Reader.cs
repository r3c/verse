using System;
using System.Globalization;
using ProtoBuf;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.Schemas.Protobuf
{
	class Reader<TEntity> : RecurseReader<TEntity, ReaderState, ProtobufValue>
	{
		#region Attributes

		private static readonly Reader<TEntity> emptyReader = new Reader<TEntity>();

		#endregion

		#region Methods / Public

		public override BrowserMove<TEntity> Browse(Func<TEntity> constructor, ReaderState state)
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

		public override RecurseReader<TOther, ReaderState, ProtobufValue> Create<TOther>()
		{
			return new Reader<TOther>();
		}

		public override bool Read(Func<TEntity> constructor, ReaderState state, out TEntity entity)
		{
			int fieldIndex;

			switch (state.ReadingAction)
			{
				case ReaderState.ReadingActionType.UseHeader:
					entity = constructor();

					return this.FollowNode(state.Reader.FieldNumber, ref entity, state);

				case ReaderState.ReadingActionType.ReadHeader:
					entity = constructor();

					while (state.ReadHeader(out fieldIndex))
					{
						state.AddObject(fieldIndex);

						if (!this.FollowNode(fieldIndex, ref entity, state))
							return false;
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
							!this.Root.HasSubNode)
						{
							state.Reader.SkipField();

							return true;
						}

						return this.ReadObjectValue(ref entity, state);
					}

					switch (state.Reader.WireType)
					{
						case WireType.Fixed32:
							entity = this.ProcessValue(new ProtobufValue(state.Reader.ReadSingle()));

							return true;

						case WireType.Fixed64:
							entity = this.ProcessValue(new ProtobufValue(state.Reader.ReadDouble()));

							return true;

						case WireType.String:
							entity = this.ProcessValue(new ProtobufValue(state.Reader.ReadString()));

							return true;

						case WireType.Variant:
							entity = this.ProcessValue(new ProtobufValue(state.Reader.ReadInt64()));

							return true;
					}

					state.Error("wire type not supported, skipped");
					state.Reader.SkipField();

					entity = default(TEntity);

					return true;
			}
		}

		#endregion

		#region Methods / Private

		private static bool Ignore(ReaderState state)
		{
			TEntity dummy;

			return Reader<TEntity>.emptyReader.Read(() => default(TEntity), state, out dummy);
		}

		private bool FollowNode(int fieldIndex, ref TEntity target, ReaderState state)
		{
			EntityTree<TEntity, ReaderState> node = Reader<TEntity>.GetNode(this.Root, fieldIndex);

			state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

			return node.Read != null ? node.Read(ref target, state) : Reader<TEntity>.Ignore(state);
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
				EntityTree<TEntity, ReaderState> node;

				state.ReadHeader(out fieldIndex);
				state.AddObject(fieldIndex);

				node = Reader<TEntity>.GetNode(this.Root, fieldIndex);

				if (visitCount == 1 && node.Read != null)
				{
					state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

					if (!node.Read(ref target, state))
						return false;
				}
				else
				{
					int index;

					node = this.Root;
					index = visitCount - 1;

					foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
						node = node.Follow(digit);

					state.ReadingAction = ReaderState.ReadingActionType.UseHeader;

					if (!(node.Read != null ? node.Read(ref target, state) : Reader<TEntity>.Ignore(state)))
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

				if (this.Read(constructor, state, out current))
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

				if (!this.Read(constructor, state, out current))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		private static EntityTree<TEntity, ReaderState> GetNode(EntityTree<TEntity, ReaderState> rootNode, int fieldIndex)
		{
			EntityTree<TEntity, ReaderState> node = rootNode.Follow('_');

			foreach (char digit in fieldIndex.ToString(CultureInfo.InvariantCulture))
				node = node.Follow(digit);

			return node;
		}

		#endregion
	}
}
