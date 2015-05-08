using System;
using System.Globalization;
using System.IO;
using System.Text;
using Verse.ParserDescriptors.Recurse;

namespace Verse.Schemas.JSON
{
    internal class Reader : IReader<ReaderContext, Value>
    {
        #region Constants

        private const ulong MANTISSA_MAX = long.MaxValue/10;

        #endregion

        #region Events

        public event ParserError Error;

        #endregion

        #region Attributes

        private readonly Encoding encoding;

        #endregion

        #region Constructors

        public Reader(Encoding encoding)
        {
            this.encoding = encoding;
        }

        #endregion

        #region Methods / Public

        public IBrowser<TEntity> ReadArray<TEntity>(Func<TEntity> constructor, Container<TEntity, ReaderContext, Value> container, ReaderContext context)
        {
            char ignore;
            BrowserMove<TEntity> move;

            switch (context.Current)
            {
                case (int)'[':
                    context.Pull();

                    move = (int index, out TEntity current) =>
                    {
                        current = constructor();

                        this.PullIgnored(context);

                        if (context.Current == (int)']')
                        {
                            context.Pull();

                            return BrowserState.Success;
                        }

                        // Read comma separator if any
                        if (index > 0)
                        {
                            if (!this.PullExpected(context, ','))
                                return BrowserState.Failure;

                            this.PullIgnored(context);
                        }

                        // Read array value
                        if (!this.ReadValue(ref current, container, context))
                            return BrowserState.Failure;

                        return BrowserState.Continue;
                    };

                    break;

                case (int)'{':
                    context.Pull();

                    move = (int index, out TEntity current) =>
                    {
                        current = constructor();

                        this.PullIgnored(context);

                        if (context.Current == (int)'}')
                        {
                            context.Pull();

                            return BrowserState.Success;
                        }

                        // Read comma separator if any
                        if (index > 0)
                        {
                            if (!this.PullExpected(context, ','))
                                return BrowserState.Failure;

                            this.PullIgnored(context);
                        }

                        if (!this.PullExpected(context, '"'))
                            return BrowserState.Failure;

                        // Read and move to object key
                        while (context.Current != (int)'"')
                        {
                            if (!this.PullCharacter(context, out ignore))
                            {
                                this.OnError(context.Position, "invalid character in object key");

                                return BrowserState.Failure;
                            }
                        }

                        context.Pull();

                        // Read object separator
                        this.PullIgnored(context);

                        if (!this.PullExpected(context, ':'))
                            return BrowserState.Failure;

                        // Read object value
                        this.PullIgnored(context);

                        // Read array value
                        if (!this.ReadValue(ref current, container, context))
                            return BrowserState.Failure;

                        return BrowserState.Continue;
                    };

                    break;

                default:
                    move = (int index, out TEntity current) =>
                    {
                        current = default (TEntity);

                        if (!this.ReadValue(ref current, container, context))
                            return BrowserState.Failure;

                        return BrowserState.Success;
                    };

                    break;
            }

            return new Browser<TEntity>(move);
        }

        public bool ReadValue<TEntity>(ref TEntity target, Container<TEntity, ReaderContext, Value> container, ReaderContext context)
        {
            StringBuilder buffer;
            char current;
            INode<TEntity, ReaderContext, Value> node;
            double number;
            uint numberExponent;
            uint numberExponentMask;
            uint numberExponentPlus;
            ulong numberMantissa;
            ulong numberMantissaMask;
            ulong numberMantissaPlus;
            int numberPower;

            if (container.items != null)
                return container.items(ref target, this, context);

            switch (context.Current)
            {
                case (int)'"':
                    context.Pull();

                    // Read and store string in a buffer if its value is needed
                    if (container.value != null)
                    {
                        buffer = new StringBuilder(32);

                        while (context.Current != (int)'"')
                        {
                            if (!this.PullCharacter(context, out current))
                            {
                                this.OnError(context.Position, "invalid character in string value");

                                return false;
                            }

                            buffer.Append(current);
                        }

                        container.value(ref target, Value.FromString(buffer.ToString()));
                    }

                        // Read and discard string otherwise
                    else
                    {
                        while (context.Current != (int)'"')
                        {
                            if (!this.PullCharacter(context, out current))
                            {
                                this.OnError(context.Position, "invalid character in string value");

                                return false;
                            }
                        }
                    }

                    context.Pull();

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
                            context.Pull();

                            numberMantissaMask = ~0UL;
                            numberMantissaPlus = 1;
                        }
                        else
                        {
                            numberMantissaMask = 0;
                            numberMantissaPlus = 0;
                        }

                        // Read integral part
                        for (; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull())
                        {
                            if (numberMantissa > Reader.MANTISSA_MAX)
                            {
                                ++numberPower;

                                continue;
                            }

                            numberMantissa = numberMantissa*10 + (ulong)(context.Current - (int)'0');
                        }

                        // Read decimal part if any
                        if (context.Current == (int)'.')
                        {
                            context.Pull();

                            for (; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull())
                            {
                                if (numberMantissa > Reader.MANTISSA_MAX)
                                    continue;

                                numberMantissa = numberMantissa*10 + (ulong)(context.Current - (int)'0');

                                --numberPower;
                            }
                        }

                        // Read exponent if any
                        if (context.Current == (int)'E' || context.Current == (int)'e')
                        {
                            context.Pull();

                            switch (context.Current)
                            {
                                case (int)'+':
                                    context.Pull();

                                    numberExponentMask = 0;
                                    numberExponentPlus = 0;

                                    break;

                                case (int)'-':
                                    context.Pull();

                                    numberExponentMask = ~0U;
                                    numberExponentPlus = 1;

                                    break;

                                default:
                                    numberExponentMask = 0;
                                    numberExponentPlus = 0;

                                    break;
                            }

                            for (numberExponent = 0; context.Current >= (int)'0' && context.Current <= (int)'9'; context.Pull())
                                numberExponent = numberExponent*10 + (uint)(context.Current - (int)'0');

                            numberPower += (int)((numberExponent ^ numberExponentMask) + numberExponentPlus);
                        }

                        // Compute result number and assign if needed
                        if (container.value != null)
                        {
                            number = (long)((numberMantissa ^ numberMantissaMask) + numberMantissaPlus)*Math.Pow(10, numberPower);

                            container.value(ref target, Value.FromNumber(number));
                        }
                    }

                    return true;

                case (int)'f':
                    context.Pull();

                    if (!this.PullExpected(context, 'a') || !this.PullExpected(context, 'l') || !this.PullExpected(context, 's') || !this.PullExpected(context, 'e'))
                        return false;

                    if (container.value != null)
                        container.value(ref target, Value.FromBoolean(false));

                    return true;

                case (int)'n':
                    context.Pull();

                    if (!this.PullExpected(context, 'u') || !this.PullExpected(context, 'l') || !this.PullExpected(context, 'l'))
                        return false;

                    if (container.value != null)
                        container.value(ref target, Value.Void);

                    return true;

                case (int)'t':
                    context.Pull();

                    if (!this.PullExpected(context, 'r') || !this.PullExpected(context, 'u') || !this.PullExpected(context, 'e'))
                        return false;

                    if (container.value != null)
                        container.value(ref target, Value.FromBoolean(true));

                    return true;

                case (int)'[':
                    context.Pull();

                    for (int index = 0; true; ++index)
                    {
                        this.PullIgnored(context);

                        if (context.Current == (int)']')
                            break;

                        // Read comma separator if any
                        if (index > 0)
                        {
                            if (!this.PullExpected(context, ','))
                                return false;

                            this.PullIgnored(context);
                        }

                        // Build and move to array index
                        node = container.fields;

                        if (index > 9)
                        {
                            foreach (char digit in index.ToString(CultureInfo.InvariantCulture))
                                node = node.Follow(digit);
                        }
                        else
                            node = node.Follow((char)('0' + index));

                        // Read array value
                        if (!node.Enter(ref target, this, context))
                            return false;
                    }

                    context.Pull();

                    return true;

                case (int)'{':
                    context.Pull();

                    for (int index = 0; true; ++index)
                    {
                        this.PullIgnored(context);

                        if (context.Current == (int)'}')
                            break;

                        // Read comma separator if any
                        if (index > 0)
                        {
                            if (!this.PullExpected(context, ','))
                                return false;

                            this.PullIgnored(context);
                        }

                        if (!this.PullExpected(context, '"'))
                            return false;

                        // Read and move to object key
                        node = container.fields;

                        while (context.Current != (int)'"')
                        {
                            if (!this.PullCharacter(context, out current))
                            {
                                this.OnError(context.Position, "invalid character in object key");

                                return false;
                            }

                            node = node.Follow(current);
                        }

                        context.Pull();

                        // Read object separator
                        this.PullIgnored(context);

                        if (!this.PullExpected(context, ':'))
                            return false;

                        // Read object value
                        this.PullIgnored(context);

                        if (!node.Enter(ref target, this, context))
                            return false;
                    }

                    context.Pull();

                    return true;

                default:
                    this.OnError(context.Position, "expected array, object or value");

                    return false;
            }
        }

        public bool Start(Stream stream, out ReaderContext context)
        {
            context = new ReaderContext(stream, this.encoding);

            this.PullIgnored(context);

            if (context.Current < 0)
            {
                this.OnError(context.Position, "empty input stream");

                return false;
            }

            return true;
        }

        public void Stop(ReaderContext context)
        {
        }

        #endregion

        #region Methods / Private

        private void OnError(int position, string message)
        {
            ParserError error;

            error = this.Error;

            if (error != null)
                error(position, message);
        }

        private bool PullCharacter(ReaderContext context, out char character)
        {
            int nibble;
            int previous;
            int value;

            previous = context.Current;

            context.Pull();

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

            previous = context.Current;

            context.Pull();

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
                        previous = context.Current;

                        context.Pull();

                        if (previous >= (int)'0' && previous <= (int)'9')
                            nibble = previous - (int)'0';
                        else if (previous >= (int)'A' && previous <= (int)'F')
                            nibble = previous - (int)'A' + 10;
                        else if (previous >= (int)'a' && previous <= (int)'f')
                            nibble = previous - (int)'a' + 10;
                        else
                        {
                            this.OnError(context.Position, "unknown character in unicode escape sequence");

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

        private bool PullExpected(ReaderContext context, char expected)
        {
            if (context.Current != (int)expected)
            {
                this.OnError(context.Position, string.Format(CultureInfo.InvariantCulture, "expected '{0}'", expected));

                return false;
            }

            context.Pull();

            return true;
        }

        private void PullIgnored(ReaderContext context)
        {
            int current;

            while (true)
            {
                current = context.Current;

                if (current < 0 || current > (int)' ')
                    return;

                context.Pull();
            }
        }

        #endregion
    }
}