using System;
using System.IO;

namespace Verse.DecoderDescriptors.Flat
{
	internal interface IReader<TContext, TNative>
	{
		#region Events

		event DecodeError Error;

		#endregion

		#region Methods

		IBrowser<TEntity> ReadArray<TEntity>(Func<TEntity> constructor, Container<TEntity, TContext, TNative> container, TContext context);

		bool Read<TEntity>(Func<TEntity> constructor, Container<TEntity, TContext, TNative> container, TContext context, out TEntity target);

		bool ReadValue<TEntity>(Func<TEntity> constructor, Container<TEntity, TContext, TNative> container, TContext context, out TEntity target);

		bool Start(Stream stream, out TContext context);

		void Stop(TContext context);

		#endregion
	}
}