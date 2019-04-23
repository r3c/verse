using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	internal class WriterState
	{
		private string currentKey;

		private readonly bool omitNull;

		private bool needComma;

		private readonly StreamWriter writer;

		private static readonly char[][] Ascii = new char[128][];

		private static readonly char[] Hexa =
			{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		public WriterState(Stream stream, Encoding encoding, bool omitNull)
		{
			this.currentKey = null;
			this.needComma = false;
			this.omitNull = omitNull;
			this.writer = new StreamWriter(stream, encoding);
		}

		static WriterState()
		{
			for (var i = 0; i < 32; ++i)
				WriterState.Ascii[i] = new[]
					{'\\', 'u', '0', '0', WriterState.Hexa[(i >> 4) & 0xF], WriterState.Hexa[(i >> 0) & 0xF]};

			for (var i = 32; i < 128; ++i)
				WriterState.Ascii[i] = new[] {(char) i};

			WriterState.Ascii['\b'] = new[] {'\\', 'b'};
			WriterState.Ascii['\f'] = new[] {'\\', 'f'};
			WriterState.Ascii['\n'] = new[] {'\\', 'n'};
			WriterState.Ascii['\r'] = new[] {'\\', 'r'};
			WriterState.Ascii['\t'] = new[] {'\\', 't'};
			WriterState.Ascii['\\'] = new[] {'\\', '\\'};
			WriterState.Ascii['"'] = new[] {'\\', '\"'};
		}

		public void ArrayBegin()
		{
			this.WritePrefix();
			this.writer.Write('[');

			this.needComma = false;
		}

		public void ArrayEnd()
		{
			this.writer.Write(']');

			this.needComma = true;
		}

		public void Key(string key)
		{
			this.currentKey = key;
		}

		public void Flush()
		{
			this.writer.Flush();
		}

		public void ObjectBegin()
		{
			this.WritePrefix();
			this.writer.Write('{');

			this.needComma = false;
		}

		public void ObjectEnd()
		{
			this.writer.Write('}');

			this.needComma = true;
		}

		public void Value(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					this.WritePrefix();
					this.writer.Write(value.Boolean ? "true" : "false");

					break;

				case JSONType.Number:
					this.WritePrefix();
					this.writer.Write(value.Number.ToString(CultureInfo.InvariantCulture));

					break;

				case JSONType.String:
					this.WritePrefix();
					this.WriteString(value.String);

					break;

				default:
					if (this.omitNull)
					{
						this.currentKey = null;

						return;
					}

					this.WritePrefix();
					this.writer.Write("null");

					break;
			}

			this.needComma = true;
		}

		private void WritePrefix()
		{
			if (this.needComma)
				this.writer.Write(',');

			if (this.currentKey != null)
			{
				this.WriteString(this.currentKey);

				this.writer.Write(':');
				this.currentKey = null;
			}
		}

		private void WriteString(string value)
		{
			this.writer.Write('"');

			foreach (var c in value)
			{
				if (c < 128)
					this.writer.Write(WriterState.Ascii[c]);
				else
				{
					this.writer.Write('\\');
					this.writer.Write('u');
					this.writer.Write(WriterState.Hexa[(c >> 12) & 0xF]);
					this.writer.Write(WriterState.Hexa[(c >> 8) & 0xF]);
					this.writer.Write(WriterState.Hexa[(c >> 4) & 0xF]);
					this.writer.Write(WriterState.Hexa[(c >> 0) & 0xF]);
				}
			}

			this.writer.Write('"');
		}
	}
}