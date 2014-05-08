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

		public event ParseError	Error;

		#endregion

		#region Attributes

		private readonly Encoding	encoding;

		#endregion

		#region Constructors

		public Reader (Encoding encoding)
		{
			this.encoding = encoding;
		}

		#endregion

		#region Methods / Public

		public bool Read<U> (ref U target, IPointer<U, ReaderContext, Value> pointer, ReaderContext context)
		{
			StringBuilder	buffer;
			char			current;
			long			numberDecimal;
			int				numberDecimalPower;
			int				numberExponent;
			sbyte			numberExponentSign;
			long			numberIntegral;
			sbyte			numberSign;
			Value			value;

			switch (context.Current)
			{
				case (int)'"':
					context.Pull ();

					if (pointer.CanAssign)
					{
						buffer = new StringBuilder (32);

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							buffer.Append (current);
						}

						context.Pull ();

						pointer.Assign (ref target, new Value {String = buffer.ToString (), Type = Content.String});
					}
					else
					{
						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;
						}

						context.Pull ();
					}

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
						// Read sign
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
								numberDecimalPower -= 1;
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

							numberExponent *= numberExponentSign;
						}
						else
							numberExponent = 0;

						if (numberDecimal != 0 || numberExponent < 0) // Decimal value with either decimal part or negative exponent
							value = new Value {Number = numberSign * (numberIntegral + numberDecimal * Math.Pow (10, numberDecimalPower)) * Math.Pow (10, numberExponent), Type = Content.Number};
						else if (numberExponent > 0) // Integer value with positive exponent
							value = new Value {Number = numberSign * numberIntegral * (long)Math.Pow (10, numberExponent), Type = Content.Number};
						else // Simple integer value
							value = new Value {Number = numberSign * numberIntegral, Type = Content.Number};
					}

					pointer.Assign (ref target, value);

					return true;

				case (int)'f':
					context.Pull ();

					if (!this.ReadExpected (context, 'a') || !this.ReadExpected (context, 'l') || !this.ReadExpected (context, 's') || !this.ReadExpected (context, 'e'))
						return false;

					pointer.Assign (ref target, new Value {Boolean = false, Type = Content.Boolean});

					return true;

				case (int)'n':
					context.Pull ();

					if (!this.ReadExpected (context, 'u') || !this.ReadExpected (context, 'l') || !this.ReadExpected (context, 'l'))
						return false;

					pointer.Assign (ref target, new Value {Type = Content.Void});

					return true;

				case (int)'t':
					context.Pull ();

					if (!this.ReadExpected (context, 'r') || !this.ReadExpected (context, 'u') || !this.ReadExpected (context, 'e'))
						return false;

					pointer.Assign (ref target, new Value {Boolean = true, Type = Content.Boolean});

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
							if (!this.ReadExpected (context, ','))
								return false;

							this.SkipBlank (context);
						}

						// Build and move to array index
						var field = pointer;

						if (index > 9)
						{
							foreach (char digit in index.ToString (CultureInfo.InvariantCulture))
								field = field.Follow (digit);
						}
						else
							field = field.Follow ((char)('0' + index));

						// Read array value
						if (!field.Enter (ref target, this, context))
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
							if (!this.ReadExpected (context, ','))
								return false;

							this.SkipBlank (context);
						}

						if (!this.ReadExpected (context, '"'))
							return false;

						// Read and move to object key
						var field = pointer;

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							field = field.Follow (current);
						}

						context.Pull ();

						// Read object separator
						this.SkipBlank (context);

						if (!this.ReadExpected (context, ':'))
							return false;

						// Read object value
						this.SkipBlank (context);

						if (!field.Enter (ref target, this, context))
							return false;
					}

					context.Pull ();

					return true;

				default:
					this.OnError (context.Position, "expected value");

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
			ParseError	error;

			error = this.Error;

			if (error != null)
				error (position, message);
		}

		private bool ReadCharacter (ReaderContext context, out char character)
		{
			int	nibble;
			int previous;
			int	value;

			previous = context.Current;

			context.Pull ();

			if (previous < 0)
			{
				this.OnError (context.Position, "expected character");

				character = default (char);

				return false;
			}

			if (previous != (int)'\\')
			{
				character = (char)previous;

				return true;
			}

			previous = context.Current;

			context.Pull ();

			switch (previous)
			{
				case -1:
					this.OnError (context.Position, "expected escaped character");

					character = default (char);

					return false;

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
						previous = context.Current;

						context.Pull ();

						if (previous >= (int)'0' && previous <= (int)'9')
							nibble = previous - (int)'0';
						else if (previous >= (int)'A' && previous <= (int)'F')
							nibble = previous - (int)'A' + 10;
						else if (previous >= (int)'a' && previous <= (int)'f')
							nibble = previous - (int)'a' + 10;
						else
						{
							this.OnError (context.Position, "expected unicode code point");

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

		private bool ReadExpected (ReaderContext context, char expected)
		{
			if (context.Current != (int)expected)
			{
				this.OnError (context.Position, string.Format (CultureInfo.InvariantCulture, "expected '{0}'", expected));

				return false;
			}

			context.Pull ();

			return true;
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

		#endregion
	}
}
