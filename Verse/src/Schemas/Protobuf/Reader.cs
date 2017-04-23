using System;
using System.Collections.Generic;
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

		private readonly List<EntityReader<TEntity, ReaderState>> arrayFields = new List<EntityReader<TEntity, ReaderState>>();

		private readonly List<EntityReader<TEntity, ReaderState>> objectFields = new List<EntityReader<TEntity, ReaderState>>();

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

		public override void DeclareField(string name, EntityReader<TEntity, ReaderState> enter)
		{
			List<EntityReader<TEntity, ReaderState>> fields;
			int index;

			if (int.TryParse(name, out index))
				fields = this.arrayFields;
			else if (name.Length > 0 && name[0] == '_' && int.TryParse(name.Substring(1), out index))
				fields = this.objectFields;
			else
				throw new ArgumentOutOfRangeException("name", name, "Protobuf schema only supports _X and X as field names, where X is an integer");

			while (fields.Count <= index)
				fields.Add(null);

			fields[index] = enter;
		}

		public override bool Read(ref TEntity entity, ReaderState state)
		{
			int fieldIndex;

			switch (state.ReadingAction)
			{
				case ReaderState.ReadingActionType.UseHeader:
					return this.FollowNode(state.Reader.FieldNumber, ref entity, state);

				case ReaderState.ReadingActionType.ReadHeader:
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
						return this.ReadArray(ref entity, state);

					if (!this.IsValue)
					{
						// if it's not object, ignore
						if ((state.Reader.WireType != WireType.StartGroup && state.Reader.WireType != WireType.String) ||
						    (this.arrayFields.Count == 0 && this.objectFields.Count == 0))
						{
							state.Reader.SkipField();

							return true;
						}

						return this.ReadObjectValue(ref entity, state);
					}

					switch (state.Reader.WireType)
					{
						case WireType.Fixed32:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadSingle()));

							return true;

						case WireType.Fixed64:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadDouble()));

							return true;

						case WireType.String:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadString()));

							return true;

						case WireType.Variant:
							entity = this.ConvertValue(new ProtobufValue(state.Reader.ReadInt64()));

							return true;
					}

					state.Error("wire type not supported, skipped");
					state.Reader.SkipField();

					return true;
			}
		}

		#endregion

		#region Methods / Private

		private static bool Ignore(ReaderState state)
		{
			var dummy = default(TEntity);

			return Reader<TEntity>.emptyReader.Read(ref dummy, state);
		}

		private bool FollowNode(int fieldIndex, ref TEntity target, ReaderState state)
		{
			state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

			return fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null
				? this.objectFields[fieldIndex](ref target, state)
				: Reader<TEntity>.Ignore(state);
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

				state.ReadHeader(out fieldIndex);
				state.AddObject(fieldIndex);

				if (visitCount == 1 && fieldIndex < this.objectFields.Count && this.objectFields[fieldIndex] != null)
				{
					state.ReadingAction = ReaderState.ReadingActionType.ReadValue;

					if (!this.objectFields[fieldIndex](ref target, state))
						return false;
				}
				else
				{
					int index = visitCount - 1;

					state.ReadingAction = ReaderState.ReadingActionType.UseHeader;

					if (!(index < this.arrayFields.Count && this.arrayFields[index] != null
						? this.arrayFields[index](ref target, state)
						: Reader<TEntity>.Ignore(state)))
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

				current = constructor();

				if (this.Read(ref current, state))
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

				current = constructor();

				if (!this.Read(ref current, state))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		#endregion
	}
}
