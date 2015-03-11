using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Reader : IReader<ReaderContext, Value>
	{
		#region Constants

		private const ulong MANTISSA_MAX = long.MaxValue / 10;

		#endregion

		#region Events

		public event ParserError Error;

		#endregion

		#region Attributes

		private readonly Encoding encoding;

		#endregion

		#region Constructors

		public Reader (Encoding encoding)
		{
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public IBrowser<T> ReadArray<T> (Func<T> constructor, Container<T, ReaderContext, Value> container, ReaderContext context)
		{
			char ignore;
			BrowserMove<T> move;

			switch (context.Current)
			{
				case (int)'[':
					context.Pull ();

					move = (int index, out T current) =>
					{
						current = constructor ();

						this.SkipBlank (context);

						if (context.Current == (int)']')
						{
							context.Pull ();

							return BrowserState.Success;
						}

						// Read comma separator if any
						if (index > 0)
						{
							if (!this.SkipChar (context, ','))
								return BrowserState.Failure;

							this.SkipBlank (context);
						}

						// Read array value
						if (!this.ReadValue (ref current, container, context))
							return BrowserState.Failure;

						return BrowserState.Continue;
					};

					break;

				case (int)'{':
					context.Pull ();

					move = (int index, out T current) =>
					{
						current = constructor ();

						this.SkipBlank (context);

						if (context.Current == (int)'}')
						{
							context.Pull ();

							return BrowserState.Success;
						}

						// Read comma separator if any
						if (index > 0)
						{
							if (!this.SkipChar (context, ','))
								return BrowserState.Failure;

							this.SkipBlank (context);
						}

						if (!this.SkipChar (context, '"'))
							return BrowserState.Failure;

						// Read and move to object key
						while (context.Current != (int)'"')
						{
							if (!context.ReadCharacter (out ignore))
							{
								this.OnError (context.Position, "invalid character in object key");

								return BrowserState.Failure;
							}
						}

						context.Pull ();

						// Read object separator
						this.SkipBlank (context);

						if (!this.SkipChar (context, ':'))
							return BrowserState.Failure;

						// Read object value
						this.SkipBlank (context);

						// Read array value
						if (!this.ReadValue (ref current, container, context))
							return BrowserState.Failure;

						return BrowserState.Continue;
					};

					break;

				default:
					move = (int index, out T current) =>
					{
						current = default (T);

						if (!this.ReadValue (ref current, container, context))
							return BrowserState.Failure;

						return BrowserState.Success;
					};

					break;
			}

			return new Browser<T> (move);
		}

		public bool ReadValue<T> (ref T target, Container<T, ReaderContext, Value> container, ReaderContext context)
		{
			StringBuilder buffer;
			char current;
			INode<T, ReaderContext, Value> node;
			double number;
			uint numberExponent;
			uint numberExponentMask;
			uint numberExponentPlus;
			ulong numberMantissa;
			ulong numberMantissaMask;
			ulong numberMantissaPlus;
			int numberPower;

			if (container.items != null)
				return container.items (ref target, this, context);

			switch (context.Current)
			{
				case (int)'"':
					context.Pull ();

					// Read and store string in a buffer if its value is needed
					if (container.value != null)
					{
						buffer = new StringBuilder (32);

						while (context.Current != (int)'"')
						{
							if (!context.ReadCharacter (out current))
							{
								this.OnError (context.Position, "invalid character in string value");

								return false;
							}

							buffer.Append (current);
						}

						container.value (ref target, Value.FromString (buffer.ToString ()));
					}

					// Read and discard string otherwise
					else
					{
						while (context.Current != (int)'"')
						{
							if (!context.ReadCharacter (out current))
							{
								this.OnError (context.Position, "invalid character in string value");

								return false;
							}
						}
					}

					context.Pull ();

					return true;

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
					unchecked
					{
						numberMantissa = 0;
						numberPower = 0;

						// Read number sign
						if (context.Current == (int)'-')
						{
							context.Pull ();

							numberMantissaMask = ~0UL;
							numberMantissaPlus = 1;
						}
						else
						{
							numberMantissaMask = 0;
							numberMantissaPlus = 0;
						}

						// Read integral part
						for (; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
							numberMantissa = numberMantissa * 10 + (ulong)(context.Current - (int)'0');

						// Read decimal part if any
						if (context.Current == (int)'.')
						{
							context.Pull ();

							for (; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
							{
								if (numberMantissa > Reader.MANTISSA_MAX)
									continue;

								numberMantissa = numberMantissa * 10 + (ulong)(context.Current - (int)'0');

								--numberPower;
							}
						}

						// Read exponent if any
						if (context.Current == (int)'E' || context.Current == (int)'e')
						{
							context.Pull ();

							switch (context.Current)
							{
								case (int)'+':
									context.Pull ();

									numberExponentMask = 0;
									numberExponentPlus = 0;

									break;

								case (int)'-':
									context.Pull ();

									numberExponentMask = ~0U;
									numberExponentPlus = 1;

									break;

								default:
									numberExponentMask = 0;
									numberExponentPlus = 0;

									break;
							}

							for (numberExponent = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
								numberExponent = numberExponent * 10 + (uint)(context.Current - (int)'0');

							numberPower += (int)((numberExponent ^ numberExponentMask) + numberExponentPlus);
						}

						// Compute result number and assign if needed
						if (container.value != null)
						{
							number = (long)((numberMantissa ^ numberMantissaMask) + numberMantissaPlus) * Math.Pow (10, numberPower);

							container.value (ref target, Value.FromNumber (number));
						}
					}

					return true;

				case (int)'f':
					context.Pull ();

					if (!this.SkipChar (context, 'a') || !this.SkipChar (context, 'l') || !this.SkipChar (context, 's') || !this.SkipChar (context, 'e'))
						return false;

					if (container.value != null)
						container.value (ref target, Value.FromBoolean (false));

					return true;

				case (int)'n':
					context.Pull ();

					if (!this.SkipChar (context, 'u') || !this.SkipChar (context, 'l') || !this.SkipChar (context, 'l'))
						return false;

					if (container.value != null)
						container.value (ref target, Value.Void);

					return true;

				case (int)'t':
					context.Pull ();

					if (!this.SkipChar (context, 'r') || !this.SkipChar (context, 'u') || !this.SkipChar (context, 'e'))
						return false;

					if (container.value != null)
						container.value (ref target, Value.FromBoolean (true));

					return true;

				case (int)'[':
					context.Pull ();

					for (int index = 0; true; ++index)
					{
						this.SkipBlank (context);

						if (context.Current == (int)']')
							break;

						// Read comma separator if any
						if (index > 0)
						{
							if (!this.SkipChar (context, ','))
								return false;

							this.SkipBlank (context);
						}

						// Build and move to array index
						node = container.fields;

						if (index > 9)
						{
							foreach (char digit in index.ToString (CultureInfo.InvariantCulture))
								node = node.Follow (digit);
						}
						else
							node = node.Follow ((char)('0' + index));

						// Read array value
						if (!node.Enter (ref target, this, context))
							return false;
					}

					context.Pull ();

					return true;

				case (int)'{':
					context.Pull ();

					for (int index = 0; true; ++index)
					{
						this.SkipBlank (context);

						if (context.Current == (int)'}')
							break;

						// Read comma separator if any
						if (index > 0)
						{
							if (!this.SkipChar (context, ','))
								return false;

							this.SkipBlank (context);
						}

						if (!this.SkipChar (context, '"'))
							return false;

						// Read and move to object key
						node = container.fields;

						while (context.Current != (int)'"')
						{
							if (!context.ReadCharacter (out current))
							{
								this.OnError (context.Position, "invalid character in object key");

								return false;
							}

							node = node.Follow (current);
						}

						context.Pull ();

						// Read object separator
						this.SkipBlank (context);

						if (!this.SkipChar (context, ':'))
							return false;

						// Read object value
						this.SkipBlank (context);

						if (!node.Enter (ref target, this, context))
							return false;
					}

					context.Pull ();

					return true;

				default:
					this.OnError (context.Position, "expected array, object or value");

					return false;
			}
		}

		public bool Start (Stream stream, out ReaderContext context)
		{
			context = new ReaderContext (stream, this.encoding);

			this.SkipBlank (context);

			if (context.Current < 0)
			{
				this.OnError (context.Position, "empty input stream");

				return false;
			}

			return true;
		}

		public void Stop (ReaderContext context)
		{
		}

		#endregion

		#region Methods / Private

		private void OnError (int position, string message)
		{
			ParserError	error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		private void SkipBlank (ReaderContext source)
		{
			int current;

			while (true)
			{
				current = source.Current;

				if (current < 0 || current > (int)' ')
					return;

				source.Pull ();
			}
		}

		private bool SkipChar (ReaderContext context, char expected)
		{
			if (context.Current != (int)expected)
			{
				this.OnError (context.Position, string.Format (CultureInfo.InvariantCulture, "expected '{0}'", expected));

				return false;
			}

			context.Pull ();

			return true;
		}

		#endregion
	}
}
