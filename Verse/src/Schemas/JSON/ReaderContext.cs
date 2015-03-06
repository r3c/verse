using System;
using System.IO;
using System.Text;

namespace Verse.Schemas.JSON
{
	class ReaderContext
	{
		#region Properties

		public int Current
		{
			get
			{
				return this.current;
			}
		}

		public int Position
		{
			get
			{
				return this.position;
			}
		}

		#endregion

		#region Attributes

		private int current;

		private readonly Decoder decoder;

		private int position;

		private readonly Stream stream;

		#endregion

		#region Constructors

		public ReaderContext (Stream stream, Encoding encoding)
		{
			this.decoder = encoding.GetDecoder ();
			this.position = 0;
			this.stream = stream;

			this.Pull ();
		}

		#endregion

		#region Methods

		public unsafe void Pull ()
		{
			byte* bytes = stackalloc byte[1];
			char* chars = stackalloc char[1];
			int next;

			while (true)
			{
				next = this.stream.ReadByte ();

				if (next == -1)
				{
					this.current = -1;

					break;
				}

				bytes[0] = (byte)next;

				++this.position;

				if (this.decoder.GetChars (bytes, 1, chars, 1, false) != 0)
				{
					this.current = chars[0];

					break;
				}
			}
		}

		public bool ReadCharacter (out char character)
		{
			int nibble;
			int previous;
			int value;

			previous = this.current;

			this.Pull ();

			if (previous < 0)
			{
				character = default (char);

				return false;
			}

			if (previous != (int)'\\')
			{
				character = (char)previous;

				return true;
			}

			previous = this.current;

			this.Pull ();

			switch (previous)
			{
				case -1:
					character = default (char);

					return false;

				case (int)'"':
					character = '"';

					return true;

				case (int)'\\':
					character = '\\';

					return true;

				case (int)'b':
					character = '\b';

					return true;

				case (int)'f':
					character = '\f';

					return true;

				case (int)'n':
					character = '\n';

					return true;

				case (int)'r':
					character = '\r';

					return true;

				case (int)'t':
					character = '\t';

					return true;

				case (int)'u':
					value = 0;

					for (int i = 0; i < 4; ++i)
					{
						previous = this.current;

						this.Pull ();

						if (previous >= (int)'0' && previous <= (int)'9')
							nibble = previous - (int)'0';
						else if (previous >= (int)'A' && previous <= (int)'F')
							nibble = previous - (int)'A' + 10;
						else if (previous >= (int)'a' && previous <= (int)'f')
							nibble = previous - (int)'a' + 10;
						else
						{
							character = default (char);

							return false;
						}

						value = (value << 4) + nibble;
					}

					character = (char)value;

					return true;

				default:
					character = (char)previous;

					return true;
			}
		}

		#endregion
	}
}
