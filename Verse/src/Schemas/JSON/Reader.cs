using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;
using Verse.ParserDescriptors.Recurse.Readers;
using Verse.ParserDescriptors.Recurse.Readers.String;

namespace Verse.Schemas.JSON
{
    class Reader<TEntity> : StringReader<TEntity, Value, ReaderState>
    {
        #region Constants

        private const ulong MANTISSA_MAX = long.MaxValue / 10;

        #endregion

        #region Attributes / Instance

        private readonly Encoding encoding;

        #endregion

        #region Attributes / Static

        private static readonly Reader<TEntity> unknown = new Reader<TEntity>(Encoding.Default);

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

        public override IBrowser<TEntity> ReadArray(Func<TEntity> constructor, ReaderState state)
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
                        current = default (TEntity);

                        if (!this.ReadValue(ref current, state))
                            return BrowserState.Failure;

                        return BrowserState.Success;
                    });
            }
        }

        public override bool ReadValue(ref TEntity target, ReaderState state)
        {
            if (this.HoldArray)
                return this.ProcessArray(ref target, state);

            switch (state.Current)
            {
                case (int)'"':
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
                    return this.ScanNumberAsEntity(ref target, state);

                case (int)'f':
                    state.Pull();

                    if (!this.PullExpected(state, 'a') || !this.PullExpected(state, 'l') || !this.PullExpected(state, 's') || !this.PullExpected(state, 'e'))
                        return false;

                    if (this.HoldValue)
                        this.ProcessValue(ref target, Value.FromBoolean(false));

                    return true;

                case (int)'n':
                    state.Pull();

                    if (!this.PullExpected(state, 'u') || !this.PullExpected(state, 'l') || !this.PullExpected(state, 'l'))
                        return false;

                    if (this.HoldValue)
                        this.ProcessValue(ref target, Value.Void);

                    return true;

                case (int)'t':
                    state.Pull();

                    if (!this.PullExpected(state, 'r') || !this.PullExpected(state, 'u') || !this.PullExpected(state, 'e'))
                        return false;

                    if (this.HoldValue)
                        this.ProcessValue(ref target, Value.FromBoolean(true));

                    return true;

                case (int)'[':
                    return this.ScanArrayAsEntity(ref target, state);

                case (int)'{':
                    return this.ScanObjectAsEntity(ref target, state);

                default:
                    state.OnError(state.Position, "expected array, object or value");

                    return false;
            }
        }

        public override bool Start(Stream stream, ParserError onError, out ReaderState state)
        {
            state = new ReaderState(stream, this.encoding, onError);

            this.PullIgnored(state);

            if (state.Current < 0)
            {
                state.OnError(state.Position, "empty input stream");

                return false;
            }

            return true;
        }

        public override void Stop(ReaderState state)
        {
        }

        #endregion

        #region Methods / Private

        private bool PullCharacter(ReaderState state, out char character)
        {
            int nibble;
            int previous;
            int value;

            previous = state.Current;

            state.Pull();

            if (previous < 0)
            {
                character = default (char);

                return false;
            }

            if (previous != (int)'\\')
            {
                character = (char)previous;

                return true;
            }

            previous = state.Current;

            state.Pull();

            switch (previous)
            {
                case -1:
                    character = default (char);

                    return false;

                case (int)'"':
                    character = '"';

                    return true;

                case (int)'\\':
                    character = '\\';

                    return true;

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
                        previous = state.Current;

                        state.Pull();

                        if (previous >= (int)'0' && previous <= (int)'9')
                            nibble = previous - (int)'0';
                        else if (previous >= (int)'A' && previous <= (int)'F')
                            nibble = previous - (int)'A' + 10;
                        else if (previous >= (int)'a' && previous <= (int)'f')
                            nibble = previous - (int)'a' + 10;
                        else
                        {
                            state.OnError(state.Position, "unknown character in unicode escape sequence");

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

        private bool PullExpected(ReaderState state, char expected)
        {
            if (state.Current != (int)expected)
            {
                state.OnError(state.Position, string.Format(CultureInfo.InvariantCulture, "expected '{0}'", expected));

                return false;
            }

            state.Pull();

            return true;
        }

        private void PullIgnored(ReaderState state)
        {
            int current;

            while (true)
            {
                current = state.Current;

                if (current < 0 || current > (int)' ')
                    return;

                state.Pull();
            }
        }

        private BrowserMove<TEntity> ScanArrayAsArray(Func<TEntity> constructor, ReaderState state)
        {
            state.Pull();

            return (int index, out TEntity current) =>
            {
                current = constructor();

                this.PullIgnored(state);

                if (state.Current == (int)']')
                {
                    state.Pull();

                    return BrowserState.Success;
                }

                // Read comma separator if any
                if (index > 0)
                {
                    if (!this.PullExpected(state, ','))
                        return BrowserState.Failure;

                    this.PullIgnored(state);
                }

                // Read array value
                if (!this.ReadValue(ref current, state))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };
        }

        private bool ScanArrayAsEntity(ref TEntity target, ReaderState state)
        {
            INode<TEntity, Value, ReaderState> node;

            state.Pull();

            for (int index = 0; true; ++index)
            {
                this.PullIgnored(state);

                if (state.Current == (int)']')
                    break;

                // Read comma separator if any
                if (index > 0)
                {
                    if (!this.PullExpected(state, ','))
                        return false;

                    this.PullIgnored(state);
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

            state.Pull();

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
                    state.Pull();

                    numberMantissaMask = ~0UL;
                    numberMantissaPlus = 1;
                }
                else
                {
                    numberMantissaMask = 0;
                    numberMantissaPlus = 0;
                }

                // Read integral part
                for (; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Pull())
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
                    state.Pull();

                    for (; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Pull())
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
                    state.Pull();

                    switch (state.Current)
                    {
                        case (int)'+':
                            state.Pull();

                            numberExponentMask = 0;
                            numberExponentPlus = 0;

                            break;

                        case (int)'-':
                            state.Pull();

                            numberExponentMask = ~0U;
                            numberExponentPlus = 1;

                            break;

                        default:
                            numberExponentMask = 0;
                            numberExponentPlus = 0;

                            break;
                    }

                    for (numberExponent = 0; state.Current >= (int)'0' && state.Current <= (int)'9'; state.Pull())
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
            state.Pull();

            return (int index, out TEntity current) =>
            {
                char ignore;

                current = constructor();

                this.PullIgnored(state);

                if (state.Current == (int)'}')
                {
                    state.Pull();

                    return BrowserState.Success;
                }

                // Read comma separator if any
                if (index > 0)
                {
                    if (!this.PullExpected(state, ','))
                        return BrowserState.Failure;

                    this.PullIgnored(state);
                }

                if (!this.PullExpected(state, '"'))
                    return BrowserState.Failure;

                // Read and move to object key
                while (state.Current != (int)'"')
                {
                    if (!this.PullCharacter(state, out ignore))
                    {
                        state.OnError(state.Position, "invalid character in object key");

                        return BrowserState.Failure;
                    }
                }

                state.Pull();

                // Read object separator
                this.PullIgnored(state);

                if (!this.PullExpected(state, ':'))
                    return BrowserState.Failure;

                // Read object value
                this.PullIgnored(state);

                // Read array value
                if (!this.ReadValue(ref current, state))
                    return BrowserState.Failure;

                return BrowserState.Continue;
            };
        }

        private bool ScanObjectAsEntity(ref TEntity target, ReaderState state)
        {
            char character;
            INode<TEntity, Value, ReaderState> node;

            state.Pull();

            for (int index = 0; true; ++index)
            {
                this.PullIgnored(state);

                if (state.Current == (int)'}')
                    break;

                // Read comma separator if any
                if (index > 0)
                {
                    if (!this.PullExpected(state, ','))
                        return false;

                    this.PullIgnored(state);
                }

                if (!this.PullExpected(state, '"'))
                    return false;

                // Read and move to object key
                node = this.RootNode;

                while (state.Current != (int)'"')
                {
                    if (!this.PullCharacter(state, out character))
                    {
                        state.OnError(state.Position, "invalid character in object key");

                        return false;
                    }

                    node = node.Follow(character);
                }

                state.Pull();

                // Read object separator
                this.PullIgnored(state);

                if (!this.PullExpected(state, ':'))
                    return false;

                // Read object value
                this.PullIgnored(state);

                if (!node.Enter(ref target, Reader<TEntity>.unknown, state))
                    return false;
            }

            state.Pull();

            return true;
        }

        private bool ScanStringAsEntity(ref TEntity target, ReaderState state)
        {
            StringBuilder buffer;
            char character;

            state.Pull();

            // Read and store string in a buffer if its value is needed
            if (this.HoldValue)
            {
                buffer = new StringBuilder(32);

                while (state.Current != (int)'"')
                {
                    if (!this.PullCharacter(state, out character))
                    {
                        state.OnError(state.Position, "invalid character in string value");

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
                    if (!this.PullCharacter(state, out character))
                    {
                        state.OnError(state.Position, "invalid character in string value");

                        return false;
                    }
                }
            }

            state.Pull();

            return true;
        }

        #endregion
    }
}