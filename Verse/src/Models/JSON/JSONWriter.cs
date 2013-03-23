using System;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	abstract class JSONWriter : IDisposable
	{
		#region Attributes

		private StreamWriter	writer;

		#endregion

		#region Constructors

		protected	JSONWriter (Stream stream, Encoding encoding)
		{
			this.writer = new StreamWriter (stream, encoding);
		}

		#endregion

		#region Methods
		
		public void Dispose ()
		{
			this.writer.Dispose ();
		}

		#endregion
	}
}
