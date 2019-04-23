Verse TODO list
===============

TODO
----

- [ ] Switch to type-safe descriptor API
  - [ ] Remove parameterless `HasValue` method from descriptors
  - [ ] Require `bool ValueDecoder(TNative, out TValue)` argument to `IDecoderDescriptor.HasValue`
  - [ ] Require `bool ValueEncoder(TValue, out TNative)` argument to `IEncoderDescriptor.HasValue`
  - [ ] Allow decoding converters to return false  
- [ ] Remove Protobuf dependency from RawProtobufSchema
  - [ ] Read Protobuf without need for ProtoReader class nor related types
  - [ ] Write Protobuf without need for ProtoWriter class nor related types
- [ ] Implement "ignore case" option for name-based schemas
  - [ ] Support option in NameLookup
- [ ] Implement support for proto-based Protobuf schema

DONE
----

- [x] Upgrade public API to multi-entity
  - [x] IDecoderStream & IEncoderStream should be IDisposable and flush at dispose
  - [x] Rename IDecoder.TryOpen to IDecoder.Open
  - [x] Rename IEncoder.TryOpen to IEncoder.Open
  - [x] Rename IDecoderStream.Decode to IDecoderStream.TryDecode
  - [x] Return void from IEncoderStream.Encode
- [x] Allow JSON writer customization
- [x] Add JSON option "omit null values in object fields"
- [x] Avoid using inheritance for PatternReader nodes
- [x] Remove duplicated code from Linker
- [x] Change .IsArray method, see comment in RecurseDecoderDescriptor
- [x] Factorize "Ignore" methods in all leaf IReader implementations
