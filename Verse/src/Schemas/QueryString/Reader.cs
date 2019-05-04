﻿using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Tree;
using Verse.LookupNodes;
using Verse.Lookups;

namespace Verse.Schemas.QueryString
{
	internal class Reader : IReader<ReaderState, string, char>
	{
		private readonly Encoding encoding;

		public Reader(Encoding encoding)
		{
			this.encoding = encoding;
		}

		public BrowserMove<TElement> ReadToArray<TElement>(ReaderState state, Func<TElement> constructor,
			ReaderCallback<ReaderState, string, char, TElement> callback)
		{
			return (int index, out TElement element) =>
			{
				element = default;

				return BrowserState.Failure;
			};
		}

		public bool ReadToObject<TObject>(ReaderState state,
			ILookupNode<char, ReaderCallback<ReaderState, string, char, TObject>> root, ref TObject target)
		{
			if (state.Current == -1)
				return true;

			while (true)
			{
				// Parse field name
				var empty = true;

				// FIXME: handle % encoding in field names
				var node = root;

				while (QueryStringCharacter.IsUnreserved(state.Current))
				{
					empty = false;
					node = node.Follow((char) state.Current);

					state.Pull();
				}

				if (empty)
				{
					state.Error("empty field name");

					return false;
				}

				// Parse field value
				switch (state.Current)
				{
					case '=':
						state.Pull();
						state.Location = QueryStringLocation.ValueBegin;

						if (!(node.HasValue ? node.Value(this, state, ref target) : this.ReadToValue(state, out _)))
							return false;

						break;

					default:
						state.Location = QueryStringLocation.ValueEnd;

						if (node.HasValue && !node.Value(this, state, ref target))
							return false;

						break;
				}

				if (state.Location != QueryStringLocation.ValueEnd)
					throw new InvalidOperationException(
						"internal error, please report an issue on GitHub: https://github.com/r3c/verse/issues");

				// Expect either field separator or end of stream
				if (state.Current == -1)
					return true;

				if (!QueryStringCharacter.IsSeparator(state.Current))
				{
					state.Error("unexpected character");

					return false;
				}

				state.Pull();

				// Check for end of stream (in case of dangling separator e.g. "?k&") and resume loop
				if (state.Current == -1)
					return true;

				state.Location = QueryStringLocation.Sequence;
			}
		}

		public bool ReadToValue(ReaderState state, out string value)
		{
			switch (state.Location)
			{
				case QueryStringLocation.Sequence:
					var dummy = false;

					value = default;

					return this.ReadToObject(state,
						EmptyLookupNode<char, ReaderCallback<ReaderState, string, char, bool>>.Instance, ref dummy);

				case QueryStringLocation.ValueBegin:
					return Reader.ReadValue(state, out value);

				case QueryStringLocation.ValueEnd:
					value = string.Empty;

					return true;

				default:
					value = default;

					return false;
			}
		}

		public ReaderState Start(Stream stream, ErrorEvent error)
		{
			var state = new ReaderState(stream, this.encoding, error);

			if (state.Current == '?')
				state.Pull();

			return state;
		}

		public void Stop(ReaderState context)
		{
		}

		private static bool ReadValue(ReaderState state, out string value)
		{
			var buffer = new byte[state.Encoding.GetMaxByteCount(1)];
			var builder = new StringBuilder(32);

			while (true)
			{
				int current = state.Current;

				if (QueryStringCharacter.IsUnreserved(current))
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
							value = default;

							return false;
						}

						state.Pull();

						if (state.Current == -1)
						{
							value = default;

							return false;
						}

						var hex1 = QueryStringCharacter.HexaToDecimal(state.Current);

						state.Pull();

						if (state.Current == -1)
						{
							value = default;

							return false;
						}

						var hex2 = QueryStringCharacter.HexaToDecimal(state.Current);

						state.Pull();

						if (hex1 < 0 || hex2 < 0)
						{
							value = default;

							return false;
						}

						buffer[count] = (byte)((hex1 << 4) + hex2);
					}

					builder.Append(state.Encoding.GetChars(buffer, 0, count));
				}
				else
				{
					state.Location = QueryStringLocation.ValueEnd;

					value = builder.ToString();

					return true;
				}
			}	
		}
	}
}
