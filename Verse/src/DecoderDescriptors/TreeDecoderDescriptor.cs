using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Base;
using Verse.DecoderDescriptors.Tree;

namespace Verse.DecoderDescriptors
{
	class TreeDecoderDescriptor<TEntity, TState, TValue> : BaseDecoderDescriptor<TEntity, TState, TValue>
	{
		private readonly IDecoderConverter<TValue> converter;

		private readonly TreeReader<TState, TEntity, TValue> reader;

		private readonly IReaderSession<TState> session;

		public TreeDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, TreeReader<TState, TEntity, TValue> reader) :
			base(converter, session, reader)
		{
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		public override IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent)
		{
			if (!(parent is TreeDecoderDescriptor<TField, TState, TValue> descriptor))
				throw new ArgumentOutOfRangeException(nameof(parent), "invalid target descriptor type");

			var constructor = this.GetConstructor<TField>();
			var child = descriptor.reader;

			this.reader.HasField<TField>(name, (TState state, ref TEntity target) =>
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
			var child = default(TreeReader<TState, TField, TValue>);
			var constructor = this.GetConstructor<TField>();

			child = this.reader.HasField<TField>(name, (TState state, ref TEntity target) =>
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
			var child = default(TreeReader<TState, TEntity, TValue>);

			child = this.reader.HasField<TEntity>(name, (TState state, ref TEntity target) => child.Read(ref target, state));

			return new TreeDecoderDescriptor<TEntity, TState, TValue>(this.converter, this.session, child);
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign, IDecoderDescriptor<TItem> parent)
		{
			if (!(parent is TreeDecoderDescriptor<TItem, TState, TValue> descriptor))
				throw new ArgumentOutOfRangeException(nameof(parent), "incompatible descriptor type");

			this.HasItems(descriptor.reader, this.GetConstructor<TItem>(), assign);

			return descriptor;
		}

		public override IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign)
		{
			var child = this.reader.Create<TItem>();

			this.HasItems(child, this.GetConstructor<TItem>(), assign);

			return new TreeDecoderDescriptor<TItem, TState, TValue>(this.converter, this.session, child);
		}

		public override void IsValue()
		{
			this.reader.DeclareValue(this.GetConverter());
		}

		private void HasItems<TItem>(TreeReader<TState, TItem, TValue> child, Func<TItem> constructor, DecodeAssign<TEntity, IEnumerable<TItem>> assign)
		{
			this.reader.DeclareArray((TState state, ref TEntity entity) =>
			{
				using (var browser = new Browser<TItem>(child.ReadItems(constructor, state)))
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
		}
	}
}