using System;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.DecoderDescriptors.Recurse.Readers
{
	abstract class AbstractReader<TEntity, TValue, TState> : IReader<TEntity, TValue, TState>
	{
		#region Properties

		public bool IsArray
		{
			get
			{
				return this.array != null;
			}
		}

		public bool IsValue
		{
			get
			{
				return this.value != null;
			}
		}

		#endregion

		#region Attributes

		private ReadArray<TEntity, TState> array = null;

		private Converter<TValue, TEntity> value = null;

		#endregion

		#region Methods / Abstract

		public abstract IReader<TOther, TValue, TState> Create<TOther>();

		public abstract void DeclareField(string name, ReadEntity<TEntity, TState> enter);

		public abstract BrowserMove<TEntity> ReadElements(Func<TEntity> constructor, TState state);

		public abstract bool ReadEntity(Func<TEntity> constructor, TState state, out TEntity entity);

		#endregion

		#region Methods / Public

		public void DeclareArray(ReadArray<TEntity, TState> enter)
		{
			if (this.array != null)
				throw new InvalidOperationException("can't declare array twice on same descriptor");

			this.array = enter;
		}

		public void DeclareValue(Converter<TValue, TEntity> convert)
		{
			if (this.value != null)
				throw new InvalidOperationException("can't declare value twice on same descriptor");

			this.value = convert;
		}

		#endregion

		#region Methods / Protected

		protected bool ProcessArray(Func<TEntity> constructor, TState state, out TEntity entity)
		{
			if (this.array == null)
				throw new InvalidOperationException("internal error, cannot process undeclared array");

			return this.array(constructor, state, out entity);
		}

		protected TEntity ProcessValue(TValue value)
		{
			if (this.value == null)
				throw new InvalidOperationException("internal error, cannot process undeclared value");

			return this.value(value);
		}

		#endregion
	}
}