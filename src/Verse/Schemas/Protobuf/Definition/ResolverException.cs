using System;
using System.Globalization;

namespace Verse.Schemas.Protobuf.Definition;

public sealed class ResolverException(string format, params object[] args)
    : Exception(string.Format(CultureInfo.InvariantCulture, "proto resolver error: " + format, args));