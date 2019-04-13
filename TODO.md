Verse TODO list
===============

TODO
----

- Support "ignore case" option in schemas using PatternReader
- Change .IsArray method, see comment in RecurseDecoderDescriptor
- Factorize "Ignore" methods in all leaf IReader implementations
- Support configuration options for Protobuf schema [proto-settings]
- IDecoderStream & IEncoderStream should be IDisposable and flush at dispose
- Rename IDecoder.TryOpen to IDecoder.Open
- Rename IEncoder.TryOpen to IEncoder.Open
- Rename IDecoderStream.Decode to IDecoderStream.TryDecode
- Return true from IEncoderStream.Encode
- Allow decoding converters to return false

DONE
----

- Allow JSON writer customization
- Add JSON option "omit null values in object fields"
- Avoid using inheritance for PatternReader nodes
- Remove duplicated code from Linker
