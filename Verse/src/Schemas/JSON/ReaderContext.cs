using System;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	class ReaderContext
	{
		#region Properties

		public int  Current
		{
			get
			{
				return this.current;
			}
		}

		public int	Position
		{
			get
			{
				return this.position;
			}
		}

		#endregion

		#region Attributes

		private int			 			current;

		private int						position;

		private readonly StreamReader	reader;

		#endregion

		#region Constructors

		public ReaderContext (Stream stream, Encoding encoding)
		{
			this.position = 0;
			this.reader = new StreamReader (stream, encoding);

			this.Pull ();
		}

		#endregion

		#region Methods

		public void Pull ()
		{
			int c;

			c = this.reader.Read ();

			if (this.current >= 0)
				++this.position;

			this.current = c;
		}

		#endregion
	}
}
