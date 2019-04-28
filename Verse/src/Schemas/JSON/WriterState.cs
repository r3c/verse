using System;
using System.Globalization;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	internal class WriterState : IDisposable
	{
		private const char AsciiUpperBound = (char) 128;

		private bool isEmpty;

		private string nextKey;

		private bool needComma;

		private readonly bool omitNull;

		private readonly StreamWriter writer;

		private static readonly string[] Ascii = new string[WriterState.AsciiUpperBound];

		private static readonly char[] Hexa =
			{'0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

		public WriterState(Stream stream, Encoding encoding, bool omitNull)
		{
			this.isEmpty = true;
			this.nextKey = null;
			this.needComma = false;
			this.omitNull = omitNull;
			this.writer = new StreamWriter(stream, encoding, 1024, true);
		}

		static WriterState()
		{
			for (var i = 0; i < 32; ++i)
				WriterState.Ascii[i] = "\\u00" + WriterState.Hexa[(i >> 4) & 0xF] + WriterState.Hexa[(i >> 0) & 0xF];

			for (var i = 32; i < WriterState.AsciiUpperBound; ++i)
				WriterState.Ascii[i] = new string((char) i, 1);

			WriterState.Ascii['\b'] = "\\b";
			WriterState.Ascii['\f'] = "\\f";
			WriterState.Ascii['\n'] = "\\n";
			WriterState.Ascii['\r'] = "\\r";
			WriterState.Ascii['\t'] = "\\t";
			WriterState.Ascii['\\'] = "\\\\";
			WriterState.Ascii['"'] = "\\\"";
		}

		public void ArrayBegin()
		{
			this.AppendPrefix();
			this.writer.Write('[');

			this.isEmpty = false;
			this.needComma = false;
		}

		public void ArrayEnd()
		{
			this.writer.Write(']');

			this.isEmpty = false;
			this.needComma = true;
		}

		public void Dispose()
		{
			this.writer.Dispose();
		}

		public void Flush()
		{
			if (this.isEmpty)
				this.AppendNull();

			this.isEmpty = true;
			this.nextKey = null;
			this.needComma = false;
		}

		public void Key(string key)
		{
			this.nextKey = key;
		}

		public void ObjectBegin()
		{
			this.AppendPrefix();
			this.writer.Write('{');

			this.isEmpty = false;
			this.needComma = false;
		}

		public void ObjectEnd()
		{
			this.writer.Write('}');

			this.isEmpty = false;
			this.needComma = true;
		}

		public void Value(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					this.AppendPrefix();
					this.writer.Write(value.Boolean ? "true" : "false");

					break;

				case JSONType.Number:
					this.AppendPrefix();
					this.writer.Write(value.Number.ToString(CultureInfo.InvariantCulture));

					break;

				case JSONType.String:
					this.AppendPrefix();
					WriterState.WriteString(this.writer, value.String);

					break;

				default:
					if (this.omitNull)
					{
						this.nextKey = null;

						return;
					}

					this.AppendPrefix();
					this.AppendNull();

					break;
			}

			this.isEmpty = false;
			this.needComma = true;
		}

		private void AppendNull()
		{
			this.writer.Write("null");
		}

		private void AppendPrefix()
		{
			if (this.needComma)
				this.writer.Write(',');

			if (this.nextKey == null)
				return;

			WriterState.WriteString(this.writer, this.nextKey);

			this.writer.Write(':');
			this.nextKey = null;
		}

		private static void WriteString(TextWriter writer, string value)
		{
			writer.Write('"');

			foreach (var c in value)
			{
				if (c < WriterState.AsciiUpperBound)
					writer.Write(WriterState.Ascii[c]);
				else
				{
					writer.Write("\\u");
					writer.Write(WriterState.Hexa[(c >> 12) & 0xF]);
					writer.Write(WriterState.Hexa[(c >> 8) & 0xF]);
					writer.Write(WriterState.Hexa[(c >> 4) & 0xF]);
					writer.Write(WriterState.Hexa[(c >> 0) & 0xF]);
				}
			}

			writer.Write('"');
		}
	}
}