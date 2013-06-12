using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;

namespace Verse.Models.JSON.Writers
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

		public override void	WriteArrayBegin (bool empty)
		{
			base.WriteArrayBegin (empty);

			if (!empty)
			{
				this.writer.Write ('\n');

				++this.level;

				this.Indent ();
			}
		}

		public override void	WriteArrayEnd (bool empty)
		{
			if (!empty)
			{
				this.writer.Write ('\n');

				--this.level;

				this.Indent ();
			}

			base.WriteArrayEnd (empty);
		}

		public override void	WriteComma ()
		{
			base.WriteComma ();

			this.writer.Write ('\n');

			this.Indent ();
		}

		public override void	WriteColon ()
		{
			base.WriteColon ();

			this.writer.Write (' ');
		}

		public override void	WriteObjectBegin (bool empty)
		{
			base.WriteObjectBegin (empty);

			if (!empty)
			{
				this.writer.Write ('\n');

				++this.level;

				this.Indent ();
			}
		}

		public override void	WriteObjectEnd (bool empty)
		{
			if (!empty)
			{
				this.writer.Write ('\n');

				--this.level;

				this.Indent ();
			}

			base.WriteObjectEnd (empty);
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
