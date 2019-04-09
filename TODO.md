Verse TODO list
===============

TODO
----

- Support "ignore case" option in schemas using PatternReader
- Change .IsArray method, see comment in RecurseDecoderDescriptor
- Factorize "Ignore" methods in all leaf IReader implementations
- Support configuration options for Protobuf schema [proto-settings]
- IDecoderStream & IEncoderStream should be IDisposable

DONE
----

- Allow JSON writer customization
- Add JSON option "omit null values in object fields"
- Avoid using inheritance for PatternReader nodes
- Remove duplicated code from Linker
