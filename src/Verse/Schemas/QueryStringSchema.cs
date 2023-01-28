using System;
using System.Text;
using Verse.DecoderDescriptors;
using Verse.Schemas.QueryString;

namespace Verse.Schemas;

/// <inheritdoc />
/// <summary>
/// URI query string serialization implementation following RFC-3986. This implementation has no support for
/// encoding yet.
/// See: https://tools.ietf.org/html/rfc3986#section-3.4
/// </summary>
/// <typeparam name="TEntity">Entity type</typeparam>
internal class QueryStringSchema<TEntity> : ISchema<string, TEntity>
{
    /// <inheritdoc/>
    public IDecoderDescriptor<string, TEntity> DecoderDescriptor => _decoderDescriptor;

    /// <inheritdoc/>
    public IEncoderDescriptor<string, TEntity> EncoderDescriptor => throw new NotImplementedException("encoding not implemented");

    /// <inheritdoc/>
    public string DefaultValue => string.Empty;

    /// <inheritdoc/>
    public IEncoderAdapter<string> NativeFrom => throw new NotImplementedException("encoding not implemented");

    /// <inheritdoc/>
    public IDecoderAdapter<string> NativeTo => _decoderAdapter;

    private readonly QueryStringDecoderAdapter _decoderAdapter;

    private readonly TreeDecoderDescriptor<ReaderState, string, char, TEntity> _decoderDescriptor;

    private readonly Encoding _encoding;

    public QueryStringSchema(Encoding encoding)
    {
        var readerDefinition = new ReaderDefinition<TEntity>();

        _decoderAdapter = new QueryStringDecoderAdapter();
        _decoderDescriptor = new TreeDecoderDescriptor<ReaderState, string, char, TEntity>(readerDefinition);
        _encoding = encoding;
    }

    /// <inheritdoc/>
    public IDecoder<TEntity> CreateDecoder()
    {
        return _decoderDescriptor.CreateDecoder(new Reader(_encoding));
    }

    /// <inheritdoc/>
    public IEncoder<TEntity> CreateEncoder()
    {
        throw new NotImplementedException("encoding not implemented");
    }
}