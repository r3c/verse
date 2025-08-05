using System.Reflection;
using Verse.Linkers;

namespace Verse;

public static class Linker
{
    /// <summary>
    /// Create a linker relying on reflection to automatically describe an entity when creating a decoder or encoder
    /// descriptor.
    /// </summary>
    /// <param name="bindingFlags">Binding flags used for resolution of linked entities</param>
    /// <typeparam name="TNative">Type of native values for current serialization format</typeparam>
    /// <returns>Reflection linker instance</returns>
    public static ILinker<TNative> CreateReflection<TNative>(BindingFlags bindingFlags)
    {
        return new ReflectionLinker<TNative>(bindingFlags);
    }
}