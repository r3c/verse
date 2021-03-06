﻿using System;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Tree;
using Verse.LookupNodes;

namespace Verse.Schemas.JSON
{
	internal class Reader : IReader<ReaderState, JSONValue, int>
	{
		private readonly bool readObjectValuesAsArray;
		private readonly bool readScalarAsOneElementArray;
		private readonly Encoding encoding;

		public Reader(Encoding encoding, bool readObjectValuesAsArray, bool readScalarAsOneElementArray)
		{
			this.readObjectValuesAsArray = readObjectValuesAsArray;
			this.readScalarAsOneElementArray = readScalarAsOneElementArray;
			this.encoding = encoding;
		}

		public ReaderStatus ReadToArray<TElement>(ReaderState state, Func<TElement> constructor,
			ReaderCallback<ReaderState, JSONValue, int, TElement> callback, out BrowserMove<TElement> browserMove)
		{
			state.PullIgnored();

			switch (state.Current)
			{
				case '[':
					browserMove = this.ReadToArrayFromArray(state, constructor, callback);

					return ReaderStatus.Succeeded;

				case '{':
					if (this.readObjectValuesAsArray)
					{
						browserMove = this.ReadToArrayFromObjectValues(state, constructor, callback);

						return ReaderStatus.Succeeded;
					}

					goto default;

				case 'n':
					browserMove = default;

					return Reader.ExpectKeywordNull(state) ? ReaderStatus.Ignored : ReaderStatus.Failed;

				default:
					// Accept any scalar value as an array of one element
					if (this.readScalarAsOneElementArray)
					{
						browserMove = (int index, out TElement current) =>
						{
							if (index > 0)
							{
								current = default;

								return BrowserState.Success;
							}

							current = constructor();

							return callback(this, state, ref current) != ReaderStatus.Failed
								? BrowserState.Continue
								: BrowserState.Failure;
						};

						return ReaderStatus.Succeeded;
					}

					// Ignore array when not supported by current descriptor
					else
					{
						browserMove = (int index, out TElement current) =>
						{
							current = default;

							return BrowserState.Success;
						};

						return this.Skip(state) ? ReaderStatus.Succeeded : ReaderStatus.Failed;
					}
			}
		}

		public ReaderStatus ReadToObject<TObject>(ReaderState state,
			ILookupNode<int, ReaderCallback<ReaderState, JSONValue, int, TObject>> root, ref TObject target)
		{
			state.PullIgnored();

			switch (state.Current)
			{
				case '[':
					return this.ReadToObjectFromArray(state, root, ref target);

				case '{':
					return this.ReadToObjectFromObject(state, root, ref target);

				default:
					return this.Skip(state) ? ReaderStatus.Ignored : ReaderStatus.Failed;
			}
		}

		public ReaderStatus ReadToValue(ReaderState state, out JSONValue value)
		{
			state.PullIgnored();

			switch (state.Current)
			{
				case '"':
					return Reader.ReadToValueFromString(state, out value);

				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return Reader.ReadToValueFromNumber(state, out value);

				case 'f':
					if (!Reader.ExpectKeywordFalse(state))
					{
						value = default;

						return ReaderStatus.Failed;
					}

					value = JSONValue.FromBoolean(false);

					return ReaderStatus.Succeeded;

				case 'n':
					if (!Reader.ExpectKeywordNull(state))
					{
						value = default;

						return ReaderStatus.Failed;
					}

					value = JSONValue.Void;

					return ReaderStatus.Ignored;

				case 't':
					if (!Reader.ExpectKeywordTrue(state))
					{
						value = default;

						return ReaderStatus.Failed;
					}

					value = JSONValue.FromBoolean(true);

					return ReaderStatus.Succeeded;

				case '[':
					value = default;

					return this.Skip(state) ? ReaderStatus.Succeeded : ReaderStatus.Failed;

				case '{':
					value = default;

					return this.Skip(state) ? ReaderStatus.Succeeded : ReaderStatus.Failed;

				default:
					state.Error("expected array, object or value");

					value = default;

					return ReaderStatus.Failed;
			}
		}

		public ReaderState Start(Stream stream, ErrorEvent error)
		{
			return new ReaderState(stream, this.encoding, error);
		}

		public void Stop(ReaderState state)
		{
			state.Dispose();
		}

		private BrowserMove<TElement> ReadToArrayFromArray<TElement>(ReaderState state, Func<TElement> constructor,
			ReaderCallback<ReaderState, JSONValue, int, TElement> callback)
		{
			state.Read();

			return (int index, out TElement current) =>
			{
				state.PullIgnored();

				if (state.Current == ']')
				{
					state.Read();

					current = default;

					return BrowserState.Success;
				}

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
					{
						current = default;

						return BrowserState.Failure;
					}

					state.PullIgnored();
				}

				// Read array value
				current = constructor();

				return callback(this, state, ref current) != ReaderStatus.Failed ? BrowserState.Continue : BrowserState.Failure;
			};
		}

		private BrowserMove<TElement> ReadToArrayFromObjectValues<TElement>(ReaderState state,
			Func<TElement> constructor, ReaderCallback<ReaderState, JSONValue, int, TElement> callback)
		{
			state.Read();

			return (int index, out TElement current) =>
			{
				state.PullIgnored();

				if (state.Current == '}')
				{
					state.Read();

					current = default;

					return BrowserState.Success;
				}

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
					{
						current = default;

						return BrowserState.Failure;
					}

					state.PullIgnored();
				}

				if (!state.PullExpected('"'))
				{
					current = default;

					return BrowserState.Failure;
				}

				// Read and move to object key
				while (state.Current != '"')
				{
					if (!state.PullCharacter(out _))
					{
						state.Error("invalid character in object key");

						current = default;

						return BrowserState.Failure;
					}
				}

				state.Read();

				// Read object separator
				state.PullIgnored();

				if (!state.PullExpected(':'))
				{
					current = default;

					return BrowserState.Failure;
				}

				// Read object value
				state.PullIgnored();

				// Read array value
				current = constructor();

				return callback(this, state, ref current) != ReaderStatus.Failed
					? BrowserState.Continue
					: BrowserState.Failure;
			};
		}

		private ReaderStatus ReadToObjectFromArray<TObject>(ReaderState state,
			ILookupNode<int, ReaderCallback<ReaderState, JSONValue, int, TObject>> root, ref TObject target)
		{
			state.Read();

			for (var index = 0;; ++index)
			{
				state.PullIgnored();

				if (state.Current == ']')
					break;

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
						return ReaderStatus.Failed;

					state.PullIgnored();
				}

				// Build and move to array index
				var node = root.Follow(index);

				// Read array value
				if (!(node.HasValue ? node.Value(this, state, ref target) != ReaderStatus.Failed : this.Skip(state)))
					return ReaderStatus.Failed;
			}

			state.Read();

			return ReaderStatus.Succeeded;
		}

		private ReaderStatus ReadToObjectFromObject<TObject>(ReaderState state,
			ILookupNode<int, ReaderCallback<ReaderState, JSONValue, int, TObject>> root, ref TObject target)
		{
			state.Read();

			for (var index = 0;; ++index)
			{
				state.PullIgnored();

				if (state.Current == '}')
				{
					break;
				}

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
						return ReaderStatus.Failed;

					state.PullIgnored();
				}

				if (!state.PullExpected('"'))
					return ReaderStatus.Failed;

				// Read and move to object key
				var node = root;

				while (state.Current != '"')
				{
					if (!state.PullCharacter(out var character))
					{
						state.Error("invalid character in object key");

						return ReaderStatus.Failed;
					}

					node = node.Follow(character);
				}

				state.Read();

				// Read object separator
				state.PullIgnored();

				if (!state.PullExpected(':'))
					return ReaderStatus.Failed;

				// Read object value
				state.PullIgnored();

				if (node.HasValue)
				{
					if (node.Value(this, state, ref target) == ReaderStatus.Failed)
						return ReaderStatus.Failed;
				}
				else
				{
					if (!this.Skip(state))
						return ReaderStatus.Failed;
				}
			}

			state.Read();

			return ReaderStatus.Succeeded;
		}

		private static bool ExpectKeywordFalse(ReaderState state)
		{
			return state.PullExpected('f') && state.PullExpected('a') && state.PullExpected('l') &&
			       state.PullExpected('s') && state.PullExpected('e');
		}

		private static bool ExpectKeywordNull(ReaderState state)
		{
			return state.PullExpected('n') && state.PullExpected('u') && state.PullExpected('l') &&
				   state.PullExpected('l');
		}

		private static bool ExpectKeywordTrue(ReaderState state)
		{
			return state.PullExpected('t') && state.PullExpected('r') && state.PullExpected('u') &&
				   state.PullExpected('e');
		}

		private static ReaderStatus ReadToValueFromNumber(ReaderState state, out JSONValue value)
		{
			unchecked
			{
				const ulong mantissaMax = long.MaxValue / 10;

				var numberMantissa = 0UL;
				var numberPower = 0;

				// Read number sign
				ulong numberMantissaMask;
				ulong numberMantissaPlus;

				if (state.Current == '-')
				{
					state.Read();

					numberMantissaMask = ~0UL;
					numberMantissaPlus = 1;
				}
				else
				{
					numberMantissaMask = 0;
					numberMantissaPlus = 0;
				}

				// Read integral part
				for (; state.Current >= (int) '0' && state.Current <= (int) '9'; state.Read())
				{
					if (numberMantissa > mantissaMax)
					{
						++numberPower;

						continue;
					}

					numberMantissa = numberMantissa * 10 + (ulong) (state.Current - '0');
				}

				// Read decimal part if any
				if (state.Current == '.')
				{
					state.Read();

					for (; state.Current >= (int) '0' && state.Current <= (int) '9'; state.Read())
					{
						if (numberMantissa > mantissaMax)
							continue;

						numberMantissa = numberMantissa * 10 + (ulong) (state.Current - '0');

						--numberPower;
					}
				}

				// Read exponent if any
				if (state.Current == 'E' || state.Current == 'e')
				{
					uint numberExponentMask;
					uint numberExponentPlus;

					state.Read();

					switch (state.Current)
					{
						case '+':
							state.Read();

							numberExponentMask = 0;
							numberExponentPlus = 0;

							break;

						case '-':
							state.Read();

							numberExponentMask = ~0U;
							numberExponentPlus = 1;

							break;

						default:
							numberExponentMask = 0;
							numberExponentPlus = 0;

							break;
					}

					uint numberExponent;

					for (numberExponent = 0; state.Current >= (int) '0' && state.Current <= (int) '9'; state.Read())
						numberExponent = numberExponent * 10 + (uint) (state.Current - '0');

					numberPower += (int) ((numberExponent ^ numberExponentMask) + numberExponentPlus);
				}

				value = JSONValue.FromNumber((long)((numberMantissa ^ numberMantissaMask) + numberMantissaPlus), numberPower);

				return ReaderStatus.Succeeded;
			}
		}

		private static ReaderStatus ReadToValueFromString(ReaderState state, out JSONValue value)
		{
			state.Read();

			var buffer = new StringBuilder(32);

			while (state.Current != '"')
			{
				if (!state.PullCharacter(out var character))
				{
					state.Error("invalid character in string value");

					value = default;

					return ReaderStatus.Failed;
				}

				buffer.Append(character);
			}

			state.Read();

			value = JSONValue.FromString(buffer.ToString());

			return ReaderStatus.Succeeded;
		}

		private bool Skip(ReaderState state)
		{
			var empty = false;

			switch (state.Current)
			{
				case '"':
					state.Read();

					while (state.Current != '"')
					{
						if (state.PullCharacter(out _))
							continue;

						state.Error("invalid character in string value");

						return false;
					}

					state.Read();

					return true;

				case '-':
				case '.':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return Reader.ReadToValueFromNumber(state, out _) != ReaderStatus.Failed;

				case 'f':
					return Reader.ExpectKeywordFalse(state);

				case 'n':
					return Reader.ExpectKeywordNull(state);

				case 't':
					return Reader.ExpectKeywordTrue(state);

				case '[':
					return this.ReadToObjectFromArray(state,
						       EmptyLookupNode<int, ReaderCallback<ReaderState, JSONValue, int, bool>>.Instance,
						       ref empty) != ReaderStatus.Failed;

				case '{':
					return this.ReadToObjectFromObject(state,
						       EmptyLookupNode<int, ReaderCallback<ReaderState, JSONValue, int, bool>>.Instance,
						       ref empty) != ReaderStatus.Failed;

				default:
					state.Error("expected array, object or value");

					return false;
			}
		}
	}
}
