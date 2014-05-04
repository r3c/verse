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

		IBuilder<T>	GenerateBuilder ();

		IParser<T>	GenerateParser (Func<T> constructor);

		IParser<T>	GenerateParser ();

		#endregion
	}
}
