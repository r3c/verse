using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	class JSONEncoder<T> : AbstractEncoder<T>
	{
		#region Attributes
		
		private Dictionary<Type, object>	converters;
		
        private Encoding					encoding;
        
        #endregion
        
        #region Constructors

		public	JSONEncoder (Encoding encoding, Dictionary<Type, object> converters)
		{
			this.converters = converters;
			this.encoding = encoding;
		}
		
		#endregion

    	#region Methods

        public override bool	Encode (Stream stream, T instance)
        {
            throw new NotImplementedException ();
        }

        #endregion
	}
}
