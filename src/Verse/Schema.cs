using System.Text;
using Verse.Formats.Json;
using Verse.Formats.RawProtobuf;
using Verse.Schemas;
using Verse.Schemas.Json;
using Verse.Schemas.RawProtobuf;

namespace Verse;

/// <summary>
/// Main library entry point.
/// </summary>
public static class Schema
{
    /// <summary>
    /// Create new JSON schema using given settings.
    /// </summary>
    public static ISchema<JsonValue, TEntity> CreateJson<TEntity>(JsonConfiguration configuration)
    {
        return new JsonSchema<TEntity>(configuration);
    }

    /// <summary>
    /// Create new JSON schema using default UTF8 encoding.
    /// </summary>
    public static ISchema<JsonValue, TEntity> CreateJson<TEntity>()
    {
        return CreateJson<TEntity>(default);
    }

    /// <summary>
    /// Create new query string schema with given encoding.
    /// </summary>
    public static ISchema<string, TEntity> CreateQueryString<TEntity>(Encoding encoding)
    {
        return new QueryStringSchema<TEntity>(encoding);
    }

    /// <summary>
    /// Create new query string schema with default UTF8 encoding.
    /// </summary>
    public static ISchema<string, TEntity> CreateQueryString<TEntity>()
    {
        return CreateQueryString<TEntity>(new UTF8Encoding(false));
    }

    /// <summary>
    /// Create new raw Protocol Buffers schema using given settings.
    /// </summary>
    public static ISchema<RawProtobufValue, TEntity> CreateRawProtobuf<TEntity>(RawProtobufConfiguration configuration)
    {
        return new RawProtobufSchema<TEntity>(configuration);
    }

    /// <summary>
    /// Create new raw Protocol Buffers schema using default settings.
    /// </summary>
    public static ISchema<RawProtobufValue, TEntity> CreateRawProtobuf<TEntity>()
    {
        return CreateRawProtobuf<TEntity>(default);
    }
}