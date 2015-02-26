using System;

namespace Verse.Schemas
{
	public abstract class AbstractSchema<T> : ISchema<T>
	{
		#region Properties

		public abstract IBuilderDescriptor<T> BuilderDescriptor
		{
			get;
		}

		public abstract IParserDescriptor<T> ParserDescriptor
		{
			get;
		}

		#endregion

		#region Methods / Abstract

		public abstract IBuilder<T>	CreateBuilder ();

		public abstract IParser<T>	CreateParser ();

		#endregion
	}
}
