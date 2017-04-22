using System;
using System.Collections.Generic;
using Verse.DecoderDescriptors.Abstract;
using Verse.Tools;

namespace Verse.DecoderDescriptors
{
	abstract class AbstractDecoderDescriptor<TEntity, TState, TValue> : IDecoderDescriptor<TEntity>
	{
		#region Attributes

		private readonly Dictionary<Type, object> constructors;

		private readonly IDecoderConverter<TValue> converter;

		private readonly IReader<TEntity, TState> reader;

		private readonly IReaderSession<TState> session;

		#endregion

		#region Constructors

		protected AbstractDecoderDescriptor(IDecoderConverter<TValue> converter, IReaderSession<TState> session, IReader<TEntity, TState> reader)
		{
			this.constructors = new Dictionary<Type, object>();
			this.converter = converter;
			this.reader = reader;
			this.session = session;
		}

		#endregion

		#region Methods / Abstract

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign, IDecoderDescriptor<TField> parent);

		public abstract IDecoderDescriptor<TField> HasField<TField>(string name, DecodeAssign<TEntity, TField> assign);

		public abstract IDecoderDescriptor<TEntity> HasField(string name);

		public abstract IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign, IDecoderDescriptor<TElement> parent);

		public abstract IDecoderDescriptor<TElement> IsArray<TElement>(DecodeAssign<TEntity, IEnumerable<TElement>> assign);

		public abstract void IsValue();

		#endregion

		#region Methods / Public

		public void CanCreate<TField>(Func<TField> constructor)
		{
			if (constructor == null)
				throw new ArgumentNullException("constructor");

			this.constructors[typeof (TField)] = constructor;
		}

		public IDecoder<TEntity> CreateDecoder()
		{
			return new Decoder<TEntity, TState>(this.GetConstructor<TEntity>(), this.session, this.reader);
		}

		#endregion

		#region Methods / Protected

		protected Func<TField> GetConstructor<TField>()
		{
			object constructor;

			if (this.constructors.TryGetValue(typeof (TField), out constructor))
				return (Func<TField>)constructor;

			return Generator.Constructor<TField>();
		}

		protected Converter<TValue, TEntity> GetConverter()
		{
			return this.converter.Get<TEntity>();
		}

		#endregion
	}
}