using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.Tools;

namespace Verse.DecoderDescriptors
{
	abstract class AbstractDecoderDescriptor<TEntity, TState, TValue> : IDecoderDescriptor<TEntity>
	{
		private readonly Dictionary<Type, object> constructors;

		private readonly IDecoderConverter<TValue> converter;

		private readonly IReader<TEntity, TState> reader;

		private readonly IReaderSession<TState> session;

		protected AbstractDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, IReader<TEntity, TState> reader)
		{
			this.constructors = new Dictionary<Type, object>();
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent);

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign);

		public abstract IDecoderDescriptor<TEntity> HasField(string name);

		public abstract IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign, IDecoderDescriptor<TItem> parent);

		public abstract IDecoderDescriptor<TItem> HasItems<TItem>(DecodeAssign<TEntity, IEnumerable<TItem>> assign);

		public abstract void IsValue();

		public void CanCreate<TField>(Func<TField> constructor)
		{
		    this.constructors[typeof (TField)] = constructor ?? throw new ArgumentNullException(nameof(constructor));
		}

		public IDecoder<TEntity> CreateDecoder()
		{
			return new Decoder<TEntity, TState>(this.GetConstructor<TEntity>(), this.session, this.reader);
		}

		protected Func<TField> GetConstructor<TField>()
		{
		    if (this.constructors.TryGetValue(typeof (TField), out var constructor))
				return (Func<TField>)constructor;

			return Generator.CreateConstructor<TField>();
		}

		protected Converter<TValue, TEntity> GetConverter()
		{
			return this.converter.Get<TEntity>();
		}
	}
}