using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Models.JSON
{
	class JSONWriter : IDisposable
	{
		#region Attributes / Instance

		private StreamWriter	writer;

		#endregion

		#region Attributes / Static
		
		private static readonly char[]		hexadecimals = new char[16] {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		private static readonly string[]	strings = new string[128];

		#endregion

		#region Constructors / Instance

		public	JSONWriter (Stream stream, Encoding encoding)
		{
			this.writer = new StreamWriter (stream, encoding);
		}

		#endregion
		
		#region Constructors / Static
		
		static	JSONWriter ()
		{
			for (int i = 0; i < 10; ++i)
				JSONWriter.strings[i] = "\\u000" + i;

			for (int i = 10; i < 128; ++i)
				JSONWriter.strings[i] = ((char)i).ToString ();

			JSONWriter.strings[(int)'\b'] = "\\b";
			JSONWriter.strings[(int)'\f'] = "\\f";
			JSONWriter.strings[(int)'\n'] = "\\n";
			JSONWriter.strings[(int)'\r'] = "\\r";
			JSONWriter.strings[(int)'\t'] = "\\t";
			JSONWriter.strings[(int)'"'] = "\\\"";
			JSONWriter.strings[(int)'\\'] = "\\\\";
		}
		
		#endregion

		#region Methods
		
		public void Dispose ()
		{
			this.writer.Dispose ();
		}

		public bool	WriteArrayBegin ()
		{
			this.writer.Write ('[');

			return true;
		}

		public bool	WriteArrayEnd ()
		{
			this.writer.Write (']');

			return true;
		}

		public bool	WriteBoolean (bool value)
		{
			this.writer.Write (value ? "true" : "false");

			return true;
		}

		public bool	WriteColon ()
		{
			this.writer.Write (':');

			return true;
		}

		public bool	WriteComma ()
		{
			this.writer.Write (',');

			return true;
		}

		public bool	WriteNull ()
		{
			this.writer.Write ("null");

			return true;
		}

		public bool	WriteNumber (double value)
		{
			this.writer.Write (value.ToString (CultureInfo.InvariantCulture));

			return true;
		}

		public bool	WriteObjectBegin ()
		{
			this.writer.Write ('{');

			return true;
		}

		public bool	WriteObjectEnd ()
		{
			this.writer.Write ('}');

			return true;
		}

		public bool	WriteString (string value)
		{
			this.writer.Write ('"');

			foreach (char c in value)
			{
				if ((int)c < 128)
					writer.Write (JSONWriter.strings[(int)c]);
				else if ((int)c < 65536)
					writer.Write ("\\u" + JSONWriter.hexadecimals[((int)c / 4096) % 16] + JSONWriter.hexadecimals[((int)c / 256) % 16] + JSONWriter.hexadecimals[((int)c / 16) % 16] + JSONWriter.hexadecimals[(int)c % 16]);
				else
					writer.Write ('?');
			}

			this.writer.Write ('"');

			return true;
		}

		#endregion
	}
}
