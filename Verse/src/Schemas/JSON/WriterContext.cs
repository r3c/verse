using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	class WriterContext
	{
		#region Properties

		public int Position
		{
			get
			{
				return this.position;
			}
		}

		#endregion

		#region Attributes / Instance

		private int position;

		private readonly StreamWriter writer;

		#endregion

		#region Attributes / Static

		private static readonly string[] ascii = new string[128];

		private static readonly char[] hexa = {'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		#endregion

		#region Constructors

		public WriterContext (Stream stream, Encoding encoding)
		{
			this.position = 0;
			this.writer = new StreamWriter (stream, encoding);
		}

		static WriterContext ()
		{
			for (int i = 0; i < 32; ++i)
				WriterContext.ascii[i] = "\\u00" + WriterContext.hexa[(i >> 4) & 0xF] + WriterContext.hexa[(i >> 0) & 0xF];

			for (int i = 32; i < 128; ++i)
				WriterContext.ascii[i] = ((char)i).ToString (CultureInfo.InvariantCulture);

			WriterContext.ascii[(int)'\b'] = "\\b";
			WriterContext.ascii[(int)'\f'] = "\\f";
			WriterContext.ascii[(int)'\n'] = "\\n";
			WriterContext.ascii[(int)'\r'] = "\\r";
			WriterContext.ascii[(int)'\t'] = "\\t";
			WriterContext.ascii[(int)'\\'] = "\\\\";
			WriterContext.ascii[(int)'"'] = "\\\"";
		}

		#endregion

		#region Methods

		public void ArrayBegin ()
		{
			this.writer.Write ('[');
		}

		public void ArrayEnd ()
		{
			this.writer.Write (']');
		}

		public void Key (string key)
		{
			this.String (key);

			this.writer.Write (':');
		}

		public void Flush ()
		{
			this.writer.Flush ();
		}

		public void Next ()
		{
			this.writer.Write (',');
		}

		public void ObjectBegin ()
		{
			this.writer.Write ('{');
		}

		public void ObjectEnd ()
		{
			this.writer.Write ('}');
		}

		public void String (string value)
		{
			this.writer.Write ('"');

			foreach (char c in value)
			{
				if (c < 128)
					this.writer.Write (WriterContext.ascii[(int)c]);
				else
				{
					this.writer.Write ("\\u");
					this.writer.Write (WriterContext.hexa[(c >> 12) & 0xF]);
					this.writer.Write (WriterContext.hexa[(c >> 8) & 0xF]);
					this.writer.Write (WriterContext.hexa[(c >> 4) & 0xF]);
					this.writer.Write (WriterContext.hexa[(c >> 0) & 0xF]);
				}
			}

			this.writer.Write ('"');
		}

		public void Value (Value value)
		{
			switch (value.Type)
			{
				case Content.Boolean:
					this.writer.Write (value.Boolean ? "true" : "false");

					break;

				case Content.Number:
					this.writer.Write (value.Number.ToString (CultureInfo.InvariantCulture));

					break;

				case Content.String:
					this.String (value.String);

					break;

				default:
					this.writer.Write ("null");

					break;
			}
		}

		#endregion
	}
}
