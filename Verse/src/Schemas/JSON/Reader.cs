using System;
using System.Globalization;
using System.Text;
using Verse.DecoderDescriptors.Abstract;
using Verse.DecoderDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Reader<TEntity> : RecurseReader<TEntity, ReaderState, JSONValue>
	{
		#region Constants

		private const ulong MANTISSA_MAX = long.MaxValue / 10;

		#endregion

		#region Attributes

		private static readonly Reader<TEntity> emptyReader = new Reader<TEntity>();

		#endregion

		#region Methods / Public

		public override BrowserMove<TEntity> Browse(Func<TEntity> constructor, ReaderState state)
		{
			switch (state.Current)
			{
				case (int)'[':
					return this.ScanArrayAsArray(constructor, state);

				case (int)'{':
					return this.ScanObjectAsArray(constructor, state);

				default:
					return (int index, out TEntity current) =>
					{
						current = constructor();

						if (!this.Read(ref current, state))
							return BrowserState.Failure;

						return BrowserState.Success;
					};
			}
		}

		public override RecurseReader<TOther, ReaderState, JSONValue> Create<TOther>()
		{
			return new Reader<TOther>();
		}

		public override bool Read(ref TEntity entity, ReaderState state)
		{
			if (this.IsArray)
				return this.ReadArray(ref entity, state);

			switch (state.Current)
			{
				case (int)'"':
					return this.ScanStringAsEntity(ref entity, state);

				case (int)'-':
				case (int)'.':
				case (int)'0':
				case (int)'1':
				case (int)'2':
				case (int)'3':
				case (int)'4':
				case (int)'5':
				case (int)'6':
				case (int)'7':
				case (int)'8':
				case (int)'9':
					return this.ScanNumberAsEntity(ref entity, state);

				case (int)'f':
					state.Read();

					if (!state.PullExpected('a') || !state.PullExpected('l') || !state.PullExpected('s') || !state.PullExpected('e'))
					{
						entity = default(TEntity);

						return false;
					}

					if (this.IsValue)
						entity = this.ConvertValue(JSONValue.FromBoolean(false));
					else
						entity = default(TEntity);

					return true;

				case (int)'n':
					state.Read();

					if (!state.PullExpected('u') || !state.PullExpected('l') || !state.PullExpected('l'))
					{
						entity = default(TEntity);

						return false;
					}

					if (this.IsValue)
						entity = this.ConvertValue(JSONValue.Void);
					else
						entity = default(TEntity);

					return true;

				case (int)'t':
					state.Read();

					if (!state.PullExpected('r') || !state.PullExpected('u') || !state.PullExpected('e'))
					{
						entity = default(TEntity);

						return false;
					}

					if (this.IsValue)
						entity = this.ConvertValue(JSONValue.FromBoolean(true));
					else
						entity = default(TEntity);

					return true;

				case (int)'[':
					return this.ScanArrayAsEntity(ref entity, state);

				case (int)'{':
					return this.ScanObjectAsEntity(ref entity, state);

				default:
					state.Error("expected array, object or value");

					entity = default(TEntity);

					return false;
			}
		}

		#endregion

		#region Methods / Private

		private static bool Ignore(ReaderState state)
		{
			var dummy = default(TEntity);

			return Reader<TEntity>.emptyReader.Read(ref dummy, state);
		}

		private BrowserMove<TEntity> ScanArrayAsArray(Func<TEntity> constructor, ReaderState state)
		{
			state.Read();

			return (int index, out TEntity current) =>
			{
				state.PullIgnored();

				if (state.Current == (int)']')
				{
					state.Read();

					current = default(TEntity);

					return BrowserState.Success;
				}

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
					{
						current = default(TEntity);

						return BrowserState.Failure;
					}

					state.PullIgnored();
				}

				// Read array value
				current = constructor();

				if (!this.Read(ref current, state))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		private bool ScanArrayAsEntity(ref TEntity entity, ReaderState state)
		{
			EntityTree<TEntity, ReaderState> node;

			state.Read();

			for (int index = 0; true; ++index)
			{
				state.PullIgnored();

				if (state.Current == (int)']')
					break;

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
						return false;

					state.PullIgnored();
				}

				// Build and move to array index
				node = this.Root;

				if (index > 9)
				{
					foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
						node = node.Follow(digit);
				}
				else
					node = node.Follow((char)('0' + index));

				// Read array value
				if (!(node.Read != null ? node.Read(ref entity, state) : Reader<TEntity>.Ignore(state)))
					return false;
			}

			state.Read();

			return true;
		}

		private bool ScanNumberAsEntity(ref TEntity entity, ReaderState state)
		{
			decimal number;
			uint numberExponent;
			uint numberExponentMask;
			uint numberExponentPlus;
			ulong numberMantissa;
			ulong numberMantissaMask;
			ulong numberMantissaPlus;
			int numberPower;

			unchecked
			{
				numberMantissa = 0;
				numberPower = 0;

				// Read number sign
				if (state.Current == (int)'-')
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
				for (; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Read())
				{
					if (numberMantissa > MANTISSA_MAX)
					{
						++numberPower;

						continue;
					}

					numberMantissa = numberMantissa*10 + (ulong)(state.Current - (int)'0');
				}

				// Read decimal part if any
				if (state.Current == (int)'.')
				{
					state.Read();

					for (; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Read())
					{
						if (numberMantissa > MANTISSA_MAX)
							continue;

						numberMantissa = numberMantissa*10 + (ulong)(state.Current - (int)'0');

						--numberPower;
					}
				}

				// Read exponent if any
				if (state.Current == (int)'E' || state.Current == (int)'e')
				{
					state.Read();

					switch (state.Current)
					{
						case (int)'+':
							state.Read();

							numberExponentMask = 0;
							numberExponentPlus = 0;

							break;

						case (int)'-':
							state.Read();

							numberExponentMask = ~0U;
							numberExponentPlus = 1;

							break;

						default:
							numberExponentMask = 0;
							numberExponentPlus = 0;

							break;
					}

					for (numberExponent = 0; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Read())
						numberExponent = numberExponent*10 + (uint)(state.Current - (int)'0');

					numberPower += (int)((numberExponent ^ numberExponentMask) + numberExponentPlus);
				}

				// Compute result number and assign if needed
				if (this.IsValue)
				{
					number =
						(long)((numberMantissa ^ numberMantissaMask) + numberMantissaPlus) *
						(decimal)Math.Pow(10, numberPower);

					entity = this.ConvertValue(JSONValue.FromNumber(number));
				}
				else
					entity = default(TEntity);
			}

			return true;
		}

		private BrowserMove<TEntity> ScanObjectAsArray(Func<TEntity> constructor, ReaderState state)
		{
			state.Read();

			return (int index, out TEntity current) =>
			{
				char ignore;

				state.PullIgnored();

				if (state.Current == (int)'}')
				{
					state.Read();

					current = default(TEntity);

					return BrowserState.Success;
				}

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
					{
						current = default(TEntity);

						return BrowserState.Failure;
					}

					state.PullIgnored();
				}

				if (!state.PullExpected('"'))
				{
					current = default(TEntity);

					return BrowserState.Failure;
				}

				// Read and move to object key
				while (state.Current != (int)'"')
				{
					if (!state.PullCharacter(out ignore))
					{
						state.Error("invalid character in object key");

						current = default(TEntity);

						return BrowserState.Failure;
					}
				}

				state.Read();

				// Read object separator
				state.PullIgnored();

				if (!state.PullExpected(':'))
				{
					current = default(TEntity);

					return BrowserState.Failure;
				}

				// Read object value
				state.PullIgnored();

				// Read array value
				current = constructor();

				if (!this.Read(ref current, state))
					return BrowserState.Failure;

				return BrowserState.Continue;
			};
		}

		private bool ScanObjectAsEntity(ref TEntity entity, ReaderState state)
		{
			char character;
			EntityTree<TEntity, ReaderState> node;

			state.Read();

			for (int index = 0; true; ++index)
			{
				state.PullIgnored();

				if (state.Current == (int)'}')
					break;

				// Read comma separator if any
				if (index > 0)
				{
					if (!state.PullExpected(','))
						return false;

					state.PullIgnored();
				}

				if (!state.PullExpected('"'))
					return false;

				// Read and move to object key
				for (node = this.Root; state.Current != (int)'"'; node = node.Follow(character))
				{
					if (!state.PullCharacter(out character))
					{
						state.Error("invalid character in object key");

						return false;
					}
				}

				state.Read();

				// Read object separator
				state.PullIgnored();

				if (!state.PullExpected(':'))
					return false;

				// Read object value
				state.PullIgnored();

				if (!(node.Read != null ? node.Read(ref entity, state) : Reader<TEntity>.Ignore(state)))
					return false;
			}

			state.Read();

			return true;
		}

		private bool ScanStringAsEntity(ref TEntity entity, ReaderState state)
		{
			StringBuilder buffer;
			char character;

			state.Read();

			// Read and store string in a buffer if its value is needed
			if (this.IsValue)
			{
				buffer = new StringBuilder(32);

				while (state.Current != (int)'"')
				{
					if (!state.PullCharacter(out character))
					{
						state.Error("invalid character in string value");

						entity = default(TEntity);

						return false;
					}

					buffer.Append(character);
				}

				entity = this.ConvertValue(JSONValue.FromString(buffer.ToString()));
			}

			// Read and discard string otherwise
			else
			{
				while (state.Current != (int)'"')
				{
					if (!state.PullCharacter(out character))
					{
						state.Error("invalid character in string value");

						entity = default(TEntity);

						return false;
					}
				}

				entity = default(TEntity);
			}

			state.Read();

			return true;
		}

		#endregion
	}
}