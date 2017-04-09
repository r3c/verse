using System.Collections.Generic;

namespace Verse.DecoderDescriptors.Flat
{
    internal interface IBrowser<out T> : IEnumerator<T>
    {
        bool Success
        {
            get;
        }
    }
}