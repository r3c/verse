using System;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	public class JSONEncoder<T> : IEncoder<T>
	{
		#region Attributes
		
        private Encoding	encoding;
        
        #endregion
        
        #region Constructors

		public	JSONEncoder (Encoding encoding)
		{
			this.encoding = encoding;
		}
		
		#endregion

    	#region Methods

        public bool	Encode (Stream stream, T instance)
        {
            throw new NotImplementedException ();
        }

        #endregion
	}
}
