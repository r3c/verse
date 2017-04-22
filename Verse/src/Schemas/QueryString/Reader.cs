using System;
using System.Text;
using Verse.DecoderDescriptors.Abstract;
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
		#region Methods / Public

		public override FlatReader<TOther, ReaderState, string> Create<TOther>()
		{
			return new Reader<TOther>();
		}

		public override bool Read(Func<TEntity> constructor, ReaderState state, out TEntity entity)
		{
			entity = constructor();

			while (state.Current != -1)
			{
				EntityTree<TEntity, ReaderState> node;
				string unused;

				if (!Reader.IsUnreserved(state.Current))
				{
					state.Error("empty field");

					entity = default(TEntity);

					return false;
				}

				node = this.fields;

				do
				{
					node = node.Follow((char)state.Current);

					state.Pull();
				}
				while (Reader.IsUnreserved(state.Current));

				if (state.Current == '=')
					state.Pull();

				if (!(node.Read != null ? node.Read(ref entity, state) : this.ScanValue(state, out unused)))
					return false;

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

		public override bool ReadValue(ReaderState state, out TEntity target)
		{
			string value;

			if (!this.ScanValue(state, out value))
			{
				target = default(TEntity);

				return false;
			}

			if (this.converter != null)
				target = this.converter(value);
			else
				target = default(TEntity);

			return true;
		}

		#endregion

		#region Methods / Private

		private bool ScanValue(ReaderState state, out string value)
		{
			var buffer = new byte[state.Encoding.GetMaxByteCount(1)];
			var builder = new StringBuilder(32);

			while (state.Current != -1)
			{
				int count;
				int current = state.Current;
				int hex1;
				int hex2;

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
					for (count = 0; state.Current == '%'; ++count)
					{
						if (count >= buffer.Length)
						{
							value = string.Empty;

							return false;
						}

						state.Pull();

						if (state.Current == -1)
						{
							value = string.Empty;

							return false;
						}

						hex1 = Reader.DecimalFromHexa((char)state.Current);

						state.Pull();

						if (state.Current == -1)
						{
							value = string.Empty;

							return false;
						}

						hex2 = Reader.DecimalFromHexa((char)state.Current);

						state.Pull();

						if (hex1 < 0 || hex2 < 0)
						{
							value = string.Empty;

							return false;
						}

						buffer[count] = (byte)((hex1 << 4) + hex2);
					}

					builder.Append(state.Encoding.GetChars(buffer, 0, count));
				}
				else
					break;
			}

			value = builder.ToString();

			return true;
		}

		#endregion
	}
}
