using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Verse.Models.JSON.Printers
{
	public class JSONIndentPrinter : JSONPrinter
	{
		#region Attributes

		private string	indent;

		private int		level;

		#endregion
		
		#region Constructors
		
		public	JSONIndentPrinter (Stream stream, Encoding encoding, string indent) :
			base (stream, encoding)
		{
			this.indent = indent;
			this.level = 0;
		}

		public	JSONIndentPrinter (Stream stream, Encoding encoding) :
			this (stream, encoding, "\t")
		{
		}

		#endregion

		#region Methods / Public

		public override void	PrintArrayBegin ()
		{
			base.PrintArrayBegin ();

			this.writer.Write ('\n');

			++this.level;

			this.Indent ();
		}

		public override void	PrintArrayEnd ()
		{
			this.writer.Write ('\n');

			--this.level;

			this.Indent ();

			base.PrintArrayEnd ();
		}

		public override void	PrintComma ()
		{
			base.PrintComma ();

			this.writer.Write ('\n');

			this.Indent ();
		}

		public override void	PrintColon ()
		{
			base.PrintColon ();

			this.writer.Write (' ');
		}

		public override void	PrintObjectBegin ()
		{
			base.PrintObjectBegin ();

			this.writer.Write ('\n');

			++this.level;

			this.Indent ();
		}

		public override void	PrintObjectEnd ()
		{
			this.writer.Write ('\n');

			--this.level;

			this.Indent ();

			base.PrintObjectEnd ();
		}

		#endregion

		#region Methods / Private

		private void	Indent ()
		{
			for (int i = this.level; i-- > 0; )
				this.writer.Write (this.indent);
		}

		#endregion
	}
}
