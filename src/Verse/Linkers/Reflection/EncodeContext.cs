using System;
using System.Collections.Generic;
using System.Reflection;

namespace Verse.Linkers.Reflection;

internal record struct EncodeContext<TNative>(
    IEncodeLinker<TNative> Automatic,
    BindingFlags BindingFlags,
    IFormat<TNative> Format,
    IDictionary<Type, object> Parents);