using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	public class JSONPrinter : IDisposable
	{
		#region Attributes / Instance

		protected StreamWriter	writer;

		#endregion

		#region Attributes / Static
		
		private static readonly char[]		hexadecimals = new char[16] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		private static readonly string[]	strings = new string[128];

		#endregion

		#region Constructors / Instance

		public	JSONPrinter (Stream stream, Encoding encoding)
		{
			this.writer = new StreamWriter (stream, encoding);
		}

		#endregion
		
		#region Constructors / Static
		
		static	JSONPrinter ()
		{
			for (int i = 0; i < 10; ++i)
				JSONPrinter.strings[i] = "\\u000" + i;

			for (int i = 10; i < 128; ++i)
				JSONPrinter.strings[i] = ((char)i).ToString ();

			JSONPrinter.strings[(int)'\b'] = "\\b";
			JSONPrinter.strings[(int)'\f'] = "\\f";
			JSONPrinter.strings[(int)'\n'] = "\\n";
			JSONPrinter.strings[(int)'\r'] = "\\r";
			JSONPrinter.strings[(int)'\t'] = "\\t";
			JSONPrinter.strings[(int)'"'] = "\\\"";
			JSONPrinter.strings[(int)'\\'] = "\\\\";
		}
		
		#endregion

		#region Methods
		
		public void Dispose ()
		{
			this.writer.Dispose ();
		}

		public virtual void	WriteArrayBegin (bool empty)
		{
			this.writer.Write ('[');
		}

		public virtual void	WriteArrayEnd (bool empty)
		{
			this.writer.Write (']');
		}

		public virtual void	WriteBoolean (bool value)
		{
			this.writer.Write (value ? "true" : "false");
		}

		public virtual void	WriteColon ()
		{
			this.writer.Write (':');
		}

		public virtual void	WriteComma ()
		{
			this.writer.Write (',');
		}

		public virtual void	WriteNull ()
		{
			this.writer.Write ("null");
		}

		public virtual void	WriteNumber (double value)
		{
			this.writer.Write (value.ToString (CultureInfo.InvariantCulture));
		}

		public virtual void	WriteObjectBegin (bool empty)
		{
			this.writer.Write ('{');
		}

		public virtual void	WriteObjectEnd (bool empty)
		{
			this.writer.Write ('}');
		}

		public virtual void	WriteString (string value)
		{
			this.writer.Write ('"');

			foreach (char c in value)
			{
				if ((int)c < 128)
					writer.Write (JSONPrinter.strings[(int)c]);
				else if ((int)c < 65536)
					writer.Write ("\\u" + JSONPrinter.hexadecimals[(int)c >> 12 & 0x0F] + JSONPrinter.hexadecimals[(int)c >> 8 & 0x0F] + JSONPrinter.hexadecimals[(int)c >> 4 & 0x0F] + JSONPrinter.hexadecimals[(int)c & 0x0F]);
				else
					writer.Write ('?');
			}

			this.writer.Write ('"');
		}

		#endregion
	}
}
