using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.DecoderDescriptors.Recurse;
using Verse.DecoderDescriptors.Recurse.Readers;
using Verse.DecoderDescriptors.Recurse.Readers.Pattern;

namespace Verse.Schemas.JSON
{
    class Reader<TEntity> : PatternReader<TEntity, Value, ReaderState>
    {
        #region Constants

        private const ulong MANTISSA_MAX = long.MaxValue / 10;

        #endregion

        #region Attributes / Instance

        private readonly Encoding encoding;

        #endregion

        #region Attributes / Static

        private static readonly Reader<TEntity> unknown = new Reader<TEntity>(null);

        #endregion

        #region Constructors

        public Reader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        #endregion

        #region Methods / Public

        public override IReader<TOther, Value, ReaderState> Create<TOther>()
        {
            return new Reader<TOther>(this.encoding);
        }

        public override IBrowser<TEntity> ReadElements(Func<TEntity> constructor, ReaderState state)
        {
            switch (state.Current)
            {
                case (int)'[':
                    return new Browser<TEntity>(this.ScanArrayAsArray(constructor, state));

                case (int)'{':
                    return new Browser<TEntity>(this.ScanObjectAsArray(constructor, state));

                default:
                    return new Browser<TEntity>((int index, out TEntity current) =>
                    {
                        if (!this.ReadEntity(constructor, state, out current))
                            return BrowserState.Failure;

                        return BrowserState.Success;
                    });
            }
        }

        public override bool ReadEntity(Func<TEntity> constructor, ReaderState state, out TEntity target)
        {
            if (this.HoldArray)
            {
            	target = constructor();

                return this.ProcessArray(ref target, state);
            }

            switch (state.Current)
            {
                case (int)'"':
            		target = constructor();

                    return this.ScanStringAsEntity(ref target, state);

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
                    target = constructor();

                    return this.ScanNumberAsEntity(ref target, state);

                case (int)'f':
                    state.Read();

                    if (!state.PullExpected('a') || !state.PullExpected('l') || !state.PullExpected('s') || !state.PullExpected('e'))
                    {
                    	target = default(TEntity);

                        return false;
                    }

                    if (this.HoldValue)
                    {
                    	target = constructor();

                        this.ProcessValue(ref target, Value.FromBoolean(false));
                    }
                    else
                    	target = default(TEntity);

                    return true;

                case (int)'n':
                    state.Read();

                    if (!state.PullExpected('u') || !state.PullExpected('l') || !state.PullExpected('l'))
                    {
                    	target = default(TEntity);

                        return false;
                    }

                    if (this.HoldValue)
                    {
                    	target = constructor();

                        this.ProcessValue(ref target, Value.Void);
                    }
                    else
                    	target = default(TEntity);

                    return true;

                case (int)'t':
                    state.Read();

                    if (!state.PullExpected('r') || !state.PullExpected('u') || !state.PullExpected('e'))
                    {
                    	target = default(TEntity);

                        return false;
                    }

                    if (this.HoldValue)
                    {
                    	target = constructor();

                        this.ProcessValue(ref target, Value.FromBoolean(true));
                    }
                    else
                    	target = default(TEntity);

                    return true;

                case (int)'[':
                    target = constructor();

                    return this.ScanArrayAsEntity(ref target, state);

                case (int)'{':
                    target = constructor();

                    return this.ScanObjectAsEntity(ref target, state);

                default:
                    state.Error(state.Position, "expected array, object or value");

                    target = default(TEntity);

                    return false;
            }
        }

        public override bool Start(Stream stream, DecodeError error, out ReaderState state)
        {
            state = new ReaderState(stream, this.encoding, error);
            state.PullIgnored();

            if (state.Current < 0)
            {
                state.Error(state.Position, "empty input stream");

                return false;
            }

            return true;
        }

        public override void Stop(ReaderState state)
        {
        }

        #endregion

        #region Methods / Private

        private BrowserMove<TEntity> ScanArrayAsArray(Func<TEntity> constructor, ReaderState state)
        {
            state.Read();

            return (int index, out TEntity current) =>
            {
                state.PullIgnored();

                if (state.Current == (int)']')
                {
                	current = default(TEntity);

                    state.Read();

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
                if (!this.ReadEntity(constructor, state, out current))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };
        }

        private bool ScanArrayAsEntity(ref TEntity target, ReaderState state)
        {
            INode<TEntity, Value, ReaderState> node;

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
                node = this.RootNode;

                if (index > 9)
                {
                    foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
                        node = node.Follow(digit);
                }
                else
                    node = node.Follow((char)('0' + index));

                // Read array value
                if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                    return false;
            }

            state.Read();

            return true;
        }

        private bool ScanNumberAsEntity(ref TEntity target, ReaderState state)
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
                if (this.HoldValue)
                {
                    number =
                        (long)((numberMantissa ^ numberMantissaMask) + numberMantissaPlus) *
                        (decimal)Math.Pow(10, numberPower);

                    this.ProcessValue(ref target, Value.FromNumber(number));
                }
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
                        state.Error(state.Position, "invalid character in object key");

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
                if (!this.ReadEntity(constructor, state, out current))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };
        }

        private bool ScanObjectAsEntity(ref TEntity target, ReaderState state)
        {
            char character;
            INode<TEntity, Value, ReaderState> node;

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
                node = this.RootNode;

                while (state.Current != (int)'"')
                {
                    if (!state.PullCharacter(out character))
                    {
                        state.Error(state.Position, "invalid character in object key");

                        return false;
                    }

                    node = node.Follow(character);
                }

                state.Read();

                // Read object separator
                state.PullIgnored();

                if (!state.PullExpected(':'))
                    return false;

                // Read object value
                state.PullIgnored();

                if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                    return false;
            }

            state.Read();

            return true;
        }

        private bool ScanStringAsEntity(ref TEntity target, ReaderState state)
        {
            StringBuilder buffer;
            char character;

            state.Read();

            // Read and store string in a buffer if its value is needed
            if (this.HoldValue)
            {
                buffer = new StringBuilder(32);

                while (state.Current != (int)'"')
                {
                    if (!state.PullCharacter(out character))
                    {
                        state.Error(state.Position, "invalid character in string value");

                        return false;
                    }

                    buffer.Append(character);
                }

                this.ProcessValue(ref target, Value.FromString(buffer.ToString()));
            }

            // Read and discard string otherwise
            else
            {
                while (state.Current != (int)'"')
                {
                    if (!state.PullCharacter(out character))
                    {
                        state.Error(state.Position, "invalid character in string value");

                        return false;
                    }
                }
            }

            state.Read();

            return true;
        }

        #endregion
    }
}