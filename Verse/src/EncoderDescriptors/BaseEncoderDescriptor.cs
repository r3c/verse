using System;
using System.Collections.Generic;
using Verse.EncoderDescriptors.Base;

namespace Verse.EncoderDescriptors
{
	abstract class BaseEncoderDescriptor<TEntity, TValue> : IEncoderDescriptor<TEntity>
	{
		protected readonly IEncoderConverter<TValue> converter;

		protected BaseEncoderDescriptor(IEncoderConverter<TValue> converter)
		{
			this.converter = converter;
		}

		public abstract IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access, IEncoderDescriptor<TField> parent);

		public abstract IEncoderDescriptor<TField> HasField<TField>(string name, Func<TEntity, TField> access);

		public abstract IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access, IEncoderDescriptor<TItem> parent);

		public abstract IEncoderDescriptor<TItem> HasItems<TItem>(Func<TEntity, IEnumerable<TItem>> access);

		public abstract void IsValue();

		public IEncoderDescriptor<TEntity> HasField(string name)
		{
			return this.HasField(name, source => source);
		}

		protected Converter<TEntity, TValue> GetConverter()
		{
			return this.converter.Get<TEntity>();
		}
	}
}