using System;
using System.Text;
using Verse.DecoderDescriptors.Flat;

namespace Verse.Schemas.QueryString
{
	static class Reader
	{
		private static readonly int[] hexadecimals = new int[128];

		private static readonly bool[] unreserved = new bool[128];

		static Reader()
		{
			for (int i = 0; i < hexadecimals.Length; ++i)
				Reader.hexadecimals[i] = -1;

			for (int c = '0'; c <= '9'; ++c)
				Reader.hexadecimals[c] = c - '0';

			for (int c = 'A'; c <= 'F'; ++c)
				Reader.hexadecimals[c] = c - 'A' + 10;

			for (int c = 'a'; c <= 'f'; ++c)
				Reader.hexadecimals[c] = c - 'a' + 10;

			for (int c = '0'; c <= '9'; ++c)
				Reader.unreserved[c] = true;

			for (int c = 'A'; c <= 'Z'; ++c)
				Reader.unreserved[c] = true;

			for (int c = 'a'; c <= 'z'; ++c)
				Reader.unreserved[c] = true;

			Reader.unreserved['-'] = true;
			Reader.unreserved['_'] = true;
			Reader.unreserved['.'] = true;
			Reader.unreserved['!'] = true;
			Reader.unreserved['~'] = true;
			Reader.unreserved['*'] = true;
			Reader.unreserved['\''] = true;
			Reader.unreserved['('] = true;
			Reader.unreserved[')'] = true;
			Reader.unreserved[','] = true;
			Reader.unreserved['"'] = true;
			Reader.unreserved['$'] = true;
			Reader.unreserved[':'] = true;
			Reader.unreserved['@'] = true;
			Reader.unreserved['/'] = true;
			Reader.unreserved['?'] = true;
		}

		/// <summary>
		/// Return decimal value of given hexadecimal character.
		/// </summary>
		/// <param name="c">Hexadecimal character</param>
		/// <returns>Decial value, or -1 if character was not a valid
		/// hexadecimal digit</returns>
		public static int DecimalFromHexa(char c)
		{
			return c > Reader.hexadecimals.Length ? -1 : Reader.hexadecimals[c];
		}

		/// <summary>
		/// Check if character is a parameters separator (& or ;).
		/// </summary>
		/// <param name="c">Input character</param>
		/// <returns>True if character is a separator, false otherwise</returns>
		public static bool IsSeparator(int c)
		{
			return c == '&' || c == ';';
		}

		/// <summary>
		/// Check if character is unreserved, i.e. can be used in a query
		/// string without having to escape it.
		/// </summary>
		/// <param name="c">Input character</param>
		/// <remarks>Array is not supported yet (",")</remarks>
		/// <returns>True if character is unreserved, false otherwise</returns>
		public static bool IsUnreserved(int c)
		{
			return c >= 0 && c < Reader.unreserved.Length && Reader.unreserved[c];
		}
	}

	class Reader<TEntity> : FlatReader<TEntity, ReaderState, string>
	{
		public override FlatReader<TOther, ReaderState, string> Create<TOther>()
		{
			return new Reader<TOther>();
		}

		public override bool Read(ReaderState state, Func<TEntity> constructor, out TEntity entity)
		{
			entity = constructor();

			while (state.Current != -1)
			{
				if (!Reader.IsUnreserved(state.Current))
				{
					state.Error("empty field");

					return false;
				}

				var node = this.Root;

				do
				{
					node = node.Follow((char)state.Current);

					state.Pull();
				}
				while (Reader.IsUnreserved(state.Current));

				if (state.Current == '=')
					state.Pull();

				if (node.Read != null)
				{
					if (!node.Read(state, ref entity))
						return false;
				}
				else
				{
					if (!this.ScanValue(state, out _))
						return false;
				}

				if (state.Current == -1)
					break;

				if (!Reader.IsSeparator(state.Current))
				{
					state.Error("unexpected character");

					return false;
				}

				state.Pull();
			}

			return true;
		}

		public override bool ReadValue(ReaderState state, out TEntity value)
		{
			if (!this.ScanValue(state, out var raw))
			{
				value = default;

				return false;
			}

			value = this.IsValue ? this.ConvertValue(raw) : default;

			return true;
		}

		private bool ScanValue(ReaderState state, out string raw)
		{
			var buffer = new byte[state.Encoding.GetMaxByteCount(1)];
			var builder = new StringBuilder(32);

			while (state.Current != -1)
			{
				int current = state.Current;

				if (Reader.IsUnreserved(current))
				{
					builder.Append((char)current);

					state.Pull();
				}
				else if (current == '+')
				{
					builder.Append(' ');

					state.Pull();
				}
				else if (current == '%')
				{
					int count;

					for (count = 0; state.Current == '%'; ++count)
					{
						if (count >= buffer.Length)
						{
							raw = string.Empty;

							return false;
						}

						state.Pull();

						if (state.Current == -1)
						{
							raw = string.Empty;

							return false;
						}

						var hex1 = Reader.DecimalFromHexa((char)state.Current);

						state.Pull();

						if (state.Current == -1)
						{
							raw = string.Empty;

							return false;
						}

						var hex2 = Reader.DecimalFromHexa((char)state.Current);

						state.Pull();

						if (hex1 < 0 || hex2 < 0)
						{
							raw = string.Empty;

							return false;
						}

						buffer[count] = (byte)((hex1 << 4) + hex2);
					}

					builder.Append(state.Encoding.GetChars(buffer, 0, count));
				}
				else
					break;
			}

			raw = builder.ToString();

			return true;
		}
	}
}
