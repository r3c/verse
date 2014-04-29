using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
	sealed class Reader : IReader<Context, Value>
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

		public bool Read<U> (ref U target, IPointer<U, Context, Value> builder, Context context)
		{
			StringBuilder				buffer;
			char						current;
			IPointer<U, Context, Value>	field;
			long						numberDecimal;
			int							numberDecimalPower;
			int							numberExponent;
			sbyte						numberExponentSign;
			long						numberIntegral;
			sbyte						numberSign;
			Value						value;

			switch (context.Current)
			{
				case (int)'"':
					context.Next ();

					if (builder.CanAssign)
					{
						buffer = new StringBuilder (32);

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							buffer.Append (current);
						}

						context.Next ();

						builder.Assign (ref target, new Value {String = buffer.ToString (), Type = Content.String});
					}
					else
					{
						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;
						}

						context.Next ();
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
							context.Next ();

							numberSign = -1;
						}
						else
							numberSign = 1;

						// Read integral part
						for (numberIntegral = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
							numberIntegral = numberIntegral * 10 + (context.Current - (int)'0');

						// Read decimal part
						if (context.Current == (int)'.')
						{
							context.Next ();

							numberDecimalPower = 0;

							for (numberDecimal = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
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
							context.Next ();

							switch (context.Current)
							{
								case (int)'+':
									context.Next ();

									numberExponentSign = 1;

									break;

								case (int)'-':
									context.Next ();

									numberExponentSign = -1;

									break;

								default:
									numberExponentSign = 1;

									break;
							}

							for (numberExponent = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Next ())
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

					builder.Assign(ref target, value);

					return true;

				case (int)'f':
					context.Next ();

					if (!this.ReadExpected (context, 'a') || !this.ReadExpected (context, 'l') || !this.ReadExpected (context, 's') || !this.ReadExpected (context, 'e'))
						return false;

					builder.Assign (ref target, new Value {Boolean = false, Type = Content.Boolean});

					return true;

				case (int)'n':
					context.Next ();

					if (!this.ReadExpected (context, 'u') || !this.ReadExpected (context, 'l') || !this.ReadExpected (context, 'l'))
						return false;

					builder.Assign (ref target, new Value {Type = Content.Void});

					return true;

				case (int)'t':
					context.Next ();

					if (!this.ReadExpected (context, 'r') || !this.ReadExpected (context, 'u') || !this.ReadExpected (context, 'e'))
						return false;

					builder.Assign (ref target, new Value {Boolean = true, Type = Content.Boolean});

					return true;

				case (int)'[':
					context.Next ();

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
						field = builder;

						if (index > 9)
						{
							foreach (char digit in index.ToString (CultureInfo.InvariantCulture))
								field = field.Next (digit);
						}
						else
							field = field.Next ((char)('0' + index));

						// Read array value
						if (!field.Enter (ref target, this, context))
							return false;
					}

					context.Next();

					return true;

				case (int)'{':
					context.Next ();

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
						field = builder;

						while (context.Current != (int)'"')
						{
							if (!this.ReadCharacter (context, out current))
								return false;

							field = field.Next (current);
						}

						context.Next ();

						// Read object separator
						this.SkipBlank (context);

						if (!this.ReadExpected (context, ':'))
							return false;

						// Read object value
						this.SkipBlank (context);

						if (!field.Enter (ref target, this, context))
							return false;
					}

					context.Next ();

					return true;

				default:
					this.OnError (context.Position, "unexpected character");

					return false;
			}
		}

		public bool Start (Stream stream, out Context context)
		{
			context = new Context (new StreamReader (stream, this.encoding));

			this.SkipBlank (context);

			if (context.Current < 0)
			{
				this.OnError (context.Position, "empty input stream");

				return false;
			}

			return true;
		}

		public void Stop (Context context)
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

		private bool ReadCharacter (Context context, out char character)
		{
			int	nibble;
			int previous;
			int	value;

			previous = context.Current;

			context.Next ();

			if (previous < 0)
			{
				this.OnError (context.Position, "expected character, found end of stream");

				character = default (char);

				return false;
			}

			if (previous != (int)'\\')
			{
				character = (char)previous;

				return true;
			}

			previous = context.Current;

			context.Next ();

			switch (previous)
			{
				case -1:
					this.OnError (context.Position, "expected escaped character, found end of stream");

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

						context.Next ();

						if (previous >= (int)'0' && previous <= (int)'9')
							nibble = previous - (int)'0';
						else if (previous >= (int)'A' && previous <= (int)'F')
							nibble = previous - (int)'A' + 10;
						else if (previous >= (int)'a' && previous <= (int)'f')
							nibble = previous - (int)'a' + 10;
						else
						{
							this.OnError (context.Position, "expected unicode character");

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

		private bool ReadExpected (Context context, char expected)
		{
			if (context.Current != (int)expected)
			{
				this.OnError (context.Position, "unexpected character");

				return false;
			}

			context.Next ();

			return true;
		}

		private void SkipBlank (Context source)
		{
			int current;

			while (true)
			{
				current = source.Current;

				if (current < 0 || current > (int)' ')
					return;

				source.Next ();
			}
		}

		#endregion
	}
}
