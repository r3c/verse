using Verse.Linkers;

namespace Verse;

public static class Linker
{
    /// <summary>
    /// Create a linker relying on reflection to automatically describe an entity when creating a decoder or encoder
    /// descriptor.
    /// </summary>
    /// <typeparam name="TNative">Type of native values for current serialization format</typeparam>
    /// <returns>Reflection linker instance</returns>
    public static ILinker<TNative> CreateReflection<TNative>()
    {
        return new ReflectionLinker<TNative>();
    }
}