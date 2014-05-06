using System;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	public sealed class WriterContext
	{
		#region Properties

		public int	Position
		{
			get
			{
				return this.position;
			}
		}

		#endregion

		#region Attributes

		private int						position;

		private readonly StreamWriter	writer;

		#endregion

		#region Constructors

		public WriterContext (Stream stream, Encoding encoding)
		{
			this.position = 0;
			this.writer = new StreamWriter (stream, encoding);
		}

		#endregion

		#region Methods

		public void Flush ()
		{
			this.writer.Flush ();
		}

		public void Push (char c)
		{
			this.writer.Write (c);

			++this.position;
		}

		public void Push (string s)
		{
			this.writer.Write (s);

			this.position += s.Length;
		}

		#endregion
	}
}
