using System;

namespace Verse
{
    public interface ISchema<T>
    {
    	#region Properties

    	IParserDescriptor<T>	ParserDescriptor
    	{
    		get;
    	}

    	IWriterDescriptor<T>	WriterDescriptor
    	{
    		get;
    	}

    	#endregion

        #region Methods

        IParser<T>	GenerateParser (Func<T> constructor);

        IParser<T>	GenerateParser ();

        IWriter<T>	GenerateWriter ();

        #endregion
    }
}
