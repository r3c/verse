using System;
using System.Collections.Generic;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Nodes;
using Verse.Tools;

namespace Verse.ParserDescriptors
{
	class RecurseParserDescriptor<T, C, V> : AbstractParserDescriptor<T>
	{
		#region Attributes

		private readonly Container<T, C, V>	container;

		private readonly IDecoder<V>		decoder;

		#endregion

		#region Constructors

		public RecurseParserDescriptor (IDecoder<V> decoder)
		{
			this.container = new Container<T, C, V> ();
			this.decoder = decoder;
		}

		#endregion

		#region Methods / Public

		public IParser<T> GetParser (Func<T> constructor, IReader<C, V> reader)
		{
			return new Parser<T, C, V> (constructor, this.container, reader);
		}

		public override IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign, IParserDescriptor<U> parent)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = parent as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "invalid target descriptor type");

			this.ConnectField (name, this.EnterInner (descriptor.container, assign));

			return parent;
		}

		public override IParserDescriptor<U> HasField<U> (string name, ParserAssign<T, U> assign)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = new RecurseParserDescriptor<U, C, V> (this.decoder);

			this.ConnectField (name, this.EnterInner (descriptor.container, assign));

			return descriptor;
		}

		public override IParserDescriptor<T> HasField (string name)
		{
			RecurseParserDescriptor<T, C, V>	descriptor;

			descriptor = new RecurseParserDescriptor<T, C, V> (this.decoder);

			this.ConnectField (name, this.EnterSelf (descriptor.container));

			return descriptor;
		}

		public override IParserDescriptor<U> HasItems<U> (ParserAssign<T, IEnumerable<U>> assign, IParserDescriptor<U> parent)
		{
			RecurseParserDescriptor<U, C, V>	descriptor;

			descriptor = parent as RecurseParserDescriptor<U, C, V>;

			if (descriptor == null)
				throw new ArgumentOutOfRangeException ("parent", "incompatible descriptor type");

			return this.HasItems (assign, descriptor);
		}

		public override IParserDescriptor<U> HasItems<U> (ParserAssign<T, IEnumerable<U>> assign)
		{
			return this.HasItems (assign, new RecurseParserDescriptor<U, C, V> (this.decoder));
		}

		public override void IsValue<U> (ParserAssign<T, U> assign)
		{
			Converter<V, U>	convert;

			convert = this.decoder.Get<U> ();

			this.container.value = (ref T target, V value) => assign (ref target, convert (value));
		}

		#endregion

		#region Methods / Private

		private void ConnectField (string name, Follow<T, C, V> enter)
		{
			BranchNode<T, C, V>	next;

			next = this.container.fields;

			foreach (char c in name)
				next = next.Connect (c);

			next.enter = enter;
		}

		private Follow<T, C, V> EnterInner<U> (Container<U, C, V> child, ParserAssign<T, U> assign)
		{
			Func<T, U>	constructor;

			constructor = this.GetConstructor<U> ();

			return (ref T target, IReader<C, V> reader, C context) =>
			{
				U   inner;

				inner = constructor (target);

				if (!reader.Read (ref inner, child, context))
					return false;

				assign (ref target, inner);

				return true;
			};
		}

		private Follow<T, C, V> EnterSelf (Container<T, C, V> child)
		{
			return (ref T target, IReader<C, V> reader, C context) => reader.Read (ref target, child, context);
		}

		private IParserDescriptor<U> HasItems<U> (ParserAssign<T, IEnumerable<U>> assign, RecurseParserDescriptor<U, C, V> parent)
		{
			Func<T, U>			constructor;
			Container<U, C, V>	recurse;

			if (this.container.items != null)
				throw new InvalidOperationException ("can't declare items twice on same descriptor");

			constructor = this.GetConstructor<U> ();
			recurse = parent.container;

			this.container.items = (ref T target, IReader<C, V> reader, C context) =>
			{
				IBrowser<U>	browser;
				T			source;

				source = target;
				browser = reader.ReadItems (() => constructor (source), recurse, context);
				assign (ref target, new Iterator<U> (browser));

				while (browser.MoveNext ())
					;

				return browser.Success;
			};

			return parent;
		}

		#endregion
	}
}
