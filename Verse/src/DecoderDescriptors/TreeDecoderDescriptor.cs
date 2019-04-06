using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Base;
using Verse.DecoderDescriptors.Tree;

namespace Verse.DecoderDescriptors
{
	class TreeDecoderDescriptor<TEntity, TState, TValue> : BaseDecoderDescriptor<TEntity, TState, TValue>
	{
		private readonly IDecoderConverter<TValue> converter;

		private readonly TreeReader<TEntity, TState, TValue> reader;

		private readonly IReaderSession<TState> session;

		public TreeDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, TreeReader<TEntity, TState, TValue> reader) :
			base(converter, session, reader)
		{
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
		{
			var descriptor = parent as TreeDecoderDescriptor<TField, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException(nameof(parent), "invalid target descriptor type");

			var constructor = this.GetConstructor<TField>();
			var child = descriptor.reader;

			this.reader.HasField<TField>(name, (ref TEntity target, TState state) =>
			{
				TField field = constructor();

				if (!child.Read(ref field, state))
					return false;

				assign(ref target, field);

				return true;
			});

			return descriptor;
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign)
		{
			TreeReader<TField, TState, TValue> child = null;

			var constructor = this.GetConstructor<TField>();

			child = this.reader.HasField<TField>(name, (ref TEntity target, TState state) =>
			{
				TField field = constructor();

				if (!child.Read(ref field, state))
					return false;

				assign(ref target, field);

				return true;
			});

			return new TreeDecoderDescriptor<TField, TState, TValue>(this.converter, this.session, child);
		}

		public override IDecoderDescriptor<TEntity> HasField(string name)
		{
			TreeReader<TEntity, TState, TValue> child = null;

			child = this.reader.HasField<TEntity>(name, (ref TEntity target, TState state) => child.Read(ref target, state));

			return new TreeDecoderDescriptor<TEntity, TState, TValue>(this.converter, this.session, child);
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign, IDecoderDescriptor<TItem> parent)
		{
			var descriptor = parent as TreeDecoderDescriptor<TItem, TState, TValue>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			var constructor = this.GetConstructor<TItem>();
			var child = descriptor.reader;

			this.reader.HasItems<TItem>((ref TEntity entity, TState state) =>
			{
				using (var browser = new Browser<TItem>(child.Browse(constructor, state)))
				{
					// FIXME:
					// This forces a unnecessary copy, and introduces some
					// unexpected behavior (e.g. not enumerating the sequence
					// resulting in an empty array). Method IsArray could just
					// pass enumerable elements and let parent field assignment
					// handle the copy if needed.
					assign(ref entity, browser);

					return browser.Finish();
				}
			});

			return descriptor;
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign)
		{
			TreeReader<TItem, TState, TValue> child = null;

			var constructor = this.GetConstructor<TItem>();

			child = this.reader.HasItems<TItem>((ref TEntity entity, TState state) =>
			{
				using (var browser = new Browser<TItem>(child.Browse(constructor, state)))
				{
					// FIXME:
					// This forces a unnecessary copy, and introduces some
					// unexpected behavior (e.g. not enumerating the sequence
					// resulting in an empty array). Method IsArray could just
					// pass enumerable elements and let parent field assignment
					// handle the copy if needed.
					assign(ref entity, browser);

					return browser.Finish();
				}
			});

			return new TreeDecoderDescriptor<TItem, TState, TValue>(this.converter, this.session, child);
		}

		public override void IsValue()
		{
			this.reader.IsValue(this.GetConverter());
		}
	}
}