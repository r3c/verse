using System;
using System.Globalization;
using System.IO;

namespace Verse.Schemas.JSON
{
	class WriterState
	{
		#region Attributes / Instance

		private string currentKey;

		private readonly EncodeError error;

		private readonly bool ignoreNull;

		private int position;

		private bool needComma;

		private readonly StreamWriter writer;

		#endregion

		#region Attributes / Static

		private static readonly char[][] ascii = new char[128][];

		private static readonly char[] hexa = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

		#endregion

		#region Constructors

		public WriterState(Stream stream, EncodeError error, JSONSettings settings)
		{
			this.currentKey = null;
			this.error = error;
			this.ignoreNull = settings.IgnoreNull;
			this.needComma = false;
			this.position = 0;
			this.writer = new StreamWriter(stream, settings.Encoding);
		}

		static WriterState()
		{
			for (int i = 0; i < 32; ++i)
				WriterState.ascii[i] = new[] { '\\', 'u', '0', '0', WriterState.hexa[(i >> 4) & 0xF], WriterState.hexa[(i >> 0) & 0xF] };

			for (int i = 32; i < 128; ++i)
				WriterState.ascii[i] = new[] { (char)i };

			WriterState.ascii['\b'] = new[] { '\\', 'b' };
			WriterState.ascii['\f'] = new[] { '\\', 'f' };
			WriterState.ascii['\n'] = new[] { '\\', 'n' };
			WriterState.ascii['\r'] = new[] { '\\', 'r' };
			WriterState.ascii['\t'] = new[] { '\\', 't' };
			WriterState.ascii['\\'] = new[] { '\\', '\\' };
			WriterState.ascii['"'] = new[] { '\\', '\"' };
		}

		#endregion

		#region Methods / Public

		public void ArrayBegin()
		{
			this.BeforeNonNull();
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
			this.BeforeNonNull();
			this.writer.Write('{');

			this.needComma = false;
		}

		public void ObjectEnd()
		{
			this.writer.Write('}');

			this.needComma = true;
		}

		public void String(string value)
		{
			this.writer.Write('"');

			foreach (char c in value)
			{
				if (c < 128)
					this.writer.Write(WriterState.ascii[(int)c]);
				else
				{
					this.writer.Write('\\');
					this.writer.Write('u');
					this.writer.Write(WriterState.hexa[(c >> 12) & 0xF]);
					this.writer.Write(WriterState.hexa[(c >> 8) & 0xF]);
					this.writer.Write(WriterState.hexa[(c >> 4) & 0xF]);
					this.writer.Write(WriterState.hexa[(c >> 0) & 0xF]);
				}
			}

			this.writer.Write('"');
		}

		public void Value(JSONValue value)
		{
			switch (value.Type)
			{
				case JSONType.Boolean:
					this.BeforeNonNull();
					this.writer.Write(value.Boolean ? "true" : "false");

					break;

				case JSONType.Number:
					this.BeforeNonNull();
					this.writer.Write(value.Number.ToString(CultureInfo.InvariantCulture));

					break;

				case JSONType.String:
					this.BeforeNonNull();
					this.String(value.String);

					break;

				default:
					if (this.ignoreNull)
					{
						this.currentKey = null;

						return;
					}

					this.BeforeNonNull();
					this.writer.Write("null");

					break;
			}

			this.needComma = true;
		}

		#endregion

		#region Methods / Private

		private void BeforeNonNull()
		{
			if (this.needComma)
				this.writer.Write(',');

			if (this.currentKey != null)
			{
				this.String(this.currentKey);

				this.writer.Write(':');
				this.currentKey = null;
			}
		}

		#endregion
	}
}