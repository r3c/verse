using System;
using System.IO;
using NUnit.Framework;

namespace Verse.Test;

internal static class ResourceResolver
{
    public static string ReadAsString<TOrigin>(string relativePath)
    {
        var path = Resolve<TOrigin>(relativePath);

        return File.ReadAllText(path);
    }

    private static string Resolve<TOrigin>(string relativePath)
    {
        var originNamespace = typeof(TOrigin).Namespace ?? string.Empty;
        var rootNamespace = typeof(ResourceResolver).Namespace ?? string.Empty;

        if (!originNamespace.StartsWith(rootNamespace))
            throw new InvalidOperationException("Generic argument located outside from root namespace");

        var originPath = originNamespace[rootNamespace.Length..].Replace('.', '/');

        return $"{TestContext.CurrentContext.TestDirectory}{originPath}/{relativePath}";
    }
}