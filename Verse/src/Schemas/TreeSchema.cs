using System;
using Verse.BuilderDescriptors;
using Verse.BuilderDescriptors.Recurse;
using Verse.ParserDescriptors;
using Verse.ParserDescriptors.Recurse;
using Verse.Schemas.Tree;
using Verse.Tools;

namespace Verse.Schemas
{
	public abstract class TreeSchema<T, CR, CW, V> : ISchema<T>
	{
		#region Properties

		public IBuilderDescriptor<T>	BuilderDescriptor
		{
			get
			{
				return this.builderDescriptor;
			}
		}

		public IParserDescriptor<T>		ParserDescriptor
		{
			get
			{
				return this.parserDescriptor;
			}
		}

		#endregion

		#region Attributes

		private readonly RecurseBuilderDescriptor<T, CW, V>	builderDescriptor;

		private readonly RecurseParserDescriptor<T, CR, V>	parserDescriptor;

		#endregion

		#region Constructors

		protected TreeSchema (IDecoder<V> decoder, IEncoder<V> encoder)
		{
			this.builderDescriptor = new RecurseBuilderDescriptor<T, CW, V> (encoder);
			this.parserDescriptor = new RecurseParserDescriptor<T, CR, V> (decoder);
		}

		#endregion

		#region Methods / Abstract

		protected abstract IReader<CR, V> GetReader ();

		protected abstract IWriter<CW, V> GetWriter ();

		#endregion

		#region Methods / Public

		public IBuilder<T> GenerateBuilder ()
		{
			return new Builder<T, CW, V> (this.builderDescriptor.Pointer, this.GetWriter ());
		}

		public IParser<T> GenerateParser (Func<T> constructor)
		{
			return new Parser<T, CR, V> (constructor, this.parserDescriptor.Pointer, this.GetReader ());
		}

		public IParser<T> GenerateParser ()
		{
			return this.GenerateParser (Generator.Constructor<T> ());
		}

		#endregion
	}
}
