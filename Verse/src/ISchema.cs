using System;

namespace Verse
{
	public interface ISchema<T>
	{
		#region Properties

		IBuilderDescriptor<T>	BuilderDescriptor
		{
			get;
		}

		IParserDescriptor<T>	ParserDescriptor
		{
			get;
		}

		#endregion

		#region Methods

		IBuilder<T>	CreateBuilder ();

		IParser<T>	CreateParser (Func<T> constructor);

		IParser<T>	CreateParser ();

		#endregion
	}
}
