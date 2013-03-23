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

		public override void	Bind (Func<T> builder)
		{
			throw new NotImplementedException();
		}

		public override void	Bind ()
		{
			throw new NotImplementedException();
		}

        public override bool	Encode (Stream stream, T instance)
        {
            throw new NotImplementedException ();
        }

		public override IEncoder<U>	HasArray<U> (EncoderArrayGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

		public override IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

		public override IEncoder<U>	HasMap<U> (EncoderMapGetter<T, U> getter)
		{
			throw new NotImplementedException();
		}

        #endregion
	}
}
