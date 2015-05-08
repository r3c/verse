using System;
using System.Collections.Generic;

namespace Verse.ParserDescriptors.Recurse
{
    internal interface IBrowser<T> : IEnumerator<T>
    {
        bool Success
        {
            get;
        }
    }
}