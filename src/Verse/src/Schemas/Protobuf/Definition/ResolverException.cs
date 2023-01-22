using System;
using System.Globalization;

namespace Verse.Schemas.Protobuf.Definition
{
    public sealed class ResolverException : Exception
    {
        public ResolverException(string format, params object[] args) :
            base(string.Format(CultureInfo.InvariantCulture, "proto resolver error: " + format, args))
        {
        }
    }
}
