using System;
using System.Collections.Generic;
using System.Reflection;

namespace Verse.Linkers.Reflection;

internal record struct EncodeContext<TNative>(IEncodeLinker<TNative> Automatic, BindingFlags BindingFlags,
    IEncoderAdapter<TNative> Adapter, TNative DefaultValue, IDictionary<Type, object> Parents);