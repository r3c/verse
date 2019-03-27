
namespace Verse.Schemas
{
	/// <summary>
	/// Base class for schema implementations.
	/// </summary>
	/// <typeparam name="TEntity">Entity type</typeparam>
	public abstract class AbstractSchema<TEntity> : ISchema<TEntity>
	{
		/// <inheritdoc/>
		public abstract IDecoderDescriptor<TEntity> DecoderDescriptor
		{
			get;
		}

		/// <inheritdoc/>
		public abstract IEncoderDescriptor<TEntity> EncoderDescriptor
		{
			get;
		}

		/// <inheritdoc/>
		public abstract IDecoder<TEntity> CreateDecoder();

		/// <inheritdoc/>
		public abstract IEncoder<TEntity> CreateEncoder();
	}
}