using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	class Reader : IReader<ReaderContext, Value>
	{
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
			long numberDecimal;
			int numberDecimalPower;
			int numberExponent;
			sbyte numberExponentSign;
			long numberIntegral;
			sbyte numberSign;

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

						container.value (ref target, new Value {String = buffer.ToString (), Type = Content.String});
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
						// Read number sign
						if (context.Current == (int)'-')
						{
							context.Pull ();

							numberSign = -1;
						}
						else
							numberSign = 1;

						// Read integral part
						for (numberIntegral = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
							numberIntegral = numberIntegral * 10 + (context.Current - (int)'0');

						// Read decimal part
						if (context.Current == (int)'.')
						{
							context.Pull ();

							numberDecimalPower = 0;

							for (numberDecimal = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
							{
								numberDecimal = numberDecimal * 10 + (context.Current - (int)'0');

								--numberDecimalPower;
							}
						}
						else
						{
							numberDecimal = 0;
							numberDecimalPower = 0;
						}

						// Read exponent
						if (context.Current == (int)'E' || context.Current == (int)'e')
						{
							context.Pull ();

							switch (context.Current)
							{
								case (int)'+':
									context.Pull ();

									numberExponentSign = 1;

									break;

								case (int)'-':
									context.Pull ();

									numberExponentSign = -1;

									break;

								default:
									numberExponentSign = 1;

									break;
							}

							for (numberExponent = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull ())
								numberExponent = numberExponent * 10 + (context.Current - (int)'0');
						}
						else
						{
							numberExponent = 0;
							numberExponentSign = 0;
						}

						// Compute result number and assign if needed
						if (container.value != null)
						{
							number = numberIntegral * numberSign;

							if (numberDecimal != 0)
								number += numberDecimal * Math.Pow (10, numberDecimalPower) * numberSign;

							if (numberExponent != 0)
								number *= Math.Pow (10, numberExponent * numberExponentSign);

							container.value (ref target, new Value {Number = number, Type = Content.Number});
						}
					}

					return true;

				case (int)'f':
					context.Pull ();

					if (!this.SkipChar (context, 'a') || !this.SkipChar (context, 'l') || !this.SkipChar (context, 's') || !this.SkipChar (context, 'e'))
						return false;

					if (container.value != null)
						container.value (ref target, new Value {Boolean = false, Type = Content.Boolean});

					return true;

				case (int)'n':
					context.Pull ();

					if (!this.SkipChar (context, 'u') || !this.SkipChar (context, 'l') || !this.SkipChar (context, 'l'))
						return false;

					if (container.value != null)
						container.value (ref target, new Value {Type = Content.Void});

					return true;

				case (int)'t':
					context.Pull ();

					if (!this.SkipChar (context, 'r') || !this.SkipChar (context, 'u') || !this.SkipChar (context, 'e'))
						return false;

					if (container.value != null)
						container.value (ref target, new Value {Boolean = true, Type = Content.Boolean});

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
