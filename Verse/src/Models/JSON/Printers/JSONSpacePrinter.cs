using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Verse.Models.JSON.Printers
{
	public class JSONSpacePrinter : JSONPrinter
	{
		#region Attributes

		private string	space;

		#endregion
		
		#region Constructors
		
		public	JSONSpacePrinter (Stream stream, Encoding encoding, string space) :
			base (stream, encoding)
		{
			this.space = space;
		}

		public	JSONSpacePrinter (Stream stream, Encoding encoding) :
			this (stream, encoding, " ")
		{
		}

		#endregion

		#region Methods

		public override void	PrintColon ()
		{
			base.PrintColon ();

			this.writer.Write (this.space);
		}

		public override void	PrintComma()
		{
			base.PrintComma ();

			this.writer.Write (this.space);
		}

		#endregion
	}
}
