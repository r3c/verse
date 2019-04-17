Verse TODO list
===============

TODO
----

- [ ] Upgrade public API to multi-entity & explicit object mode
  - [ ] IDecoderStream & IEncoderStream should be IDisposable and flush at dispose
  - [ ] Rename IDecoder.TryOpen to IDecoder.Open
  - [ ] Rename IEncoder.TryOpen to IEncoder.Open
  - [ ] Rename IDecoderStream.Decode to IDecoderStream.TryDecode
  - [ ] Return void from IEncoderStream.Encode
  - [ ] Allow decoding converters to return false
- [ ] Implement "ignore case" option for name-based schemas
  - [ ] Support option in NameLookup
- [ ] Support configuration options for Protobuf schema [proto-settings]

DONE
----

- [x] Allow JSON writer customization
- [x] Add JSON option "omit null values in object fields"
- [x] Avoid using inheritance for PatternReader nodes
- [x] Remove duplicated code from Linker
- [x] Change .IsArray method, see comment in RecurseDecoderDescriptor
- [x] Factorize "Ignore" methods in all leaf IReader implementations
