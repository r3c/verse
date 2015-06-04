using System.Collections.Generic;

namespace Verse.ParserDescriptors.Flat
{
    internal interface IBrowser<out T> : IEnumerator<T>
    {
        bool Success
        {
            get;
        }
    }
}