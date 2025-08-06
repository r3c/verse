using System;
using NUnit.Framework;

namespace Verse.Test;

internal static class ResourceResolver
{
    public static string Resolve<TOrigin>(string relativePath)
    {
        var originNamespace = typeof(TOrigin).Namespace ?? string.Empty;
        var rootNamespace = typeof(ResourceResolver).Namespace ?? string.Empty;

        if (!originNamespace.StartsWith(rootNamespace))
            throw new InvalidOperationException("Generic argument located outside from root namespace");

        var originPath = originNamespace[rootNamespace.Length..].Replace('.', '/');

        return $"{TestContext.CurrentContext.TestDirectory}{originPath}/{relativePath}";
    }
}