using System;
using System.Collections.Generic;
using System.Reflection;

namespace Verse.Linkers.Reflection;

internal record struct DecodeContext<TNative>(IDecodeLinker<TNative> Automatic, BindingFlags BindingFlags,
    IFormat<TNative> Format, IDictionary<Type, object> Parents);