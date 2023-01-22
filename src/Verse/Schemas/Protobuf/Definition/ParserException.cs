using System;
using System.Globalization;

namespace Verse.Schemas.Protobuf.Definition
{
    public sealed class ParserException : Exception
    {
        public ParserException(int position, string format, params object[] args) :
            base(string.Format(CultureInfo.InvariantCulture, "parse error at character " + position.ToString(CultureInfo.InvariantCulture) + ": " + format, args))
        {
        }
    }
}
