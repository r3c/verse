using System;
using System.IO;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test.Schemas;

[TestFixture]
internal class ProtobufSchemaTester
{
    [Test]
    [TestCase("Protobuf/Edition2023.proto", "Foo")]
    [TestCase("Protobuf/Example2.proto", "outer")]
    [TestCase("Protobuf/Example3.proto", "outer")]
    [TestCase("Protobuf/Person.proto", "Person")]
    [TestCase("Protobuf/Proto2.proto", "Foo")]
    public void Decode(string path, string messageName)
    {
        var proto = ResourceResolver.ReadAsString<ProtobufSchemaTester>(path);
        var schema = new ProtobufSchema<int>(new StringReader(proto), messageName);

        Assert.That(schema, Is.Not.Null);
    }

    [Test]
    [TestCase("edition = \"nan\";", "edition \"nan\" is not a valid number")]
    [TestCase("message Foo {} edition = \"0\";", "keyword \"edition\" must be in first position")]
    [TestCase("syntax = \"proto1\";", "syntax \"proto1\" is not a valid identifier")]
    [TestCase("message Foo {} syntax = \"proto2\";", "keyword \"syntax\" must be in first position")]
    public void Parse_ShouldThrowOnInvalidProto(string proto, string expectedMessage)
    {
        Assert.That(() => new ProtobufSchema<int>(new StringReader(proto), string.Empty),
            Throws.InstanceOf<InvalidOperationException>().With.Message.EqualTo(expectedMessage));
    }

    private class Person
    {
        public string Email = string.Empty;
        public int Id;
        public string Name = string.Empty;
    }

    [Test]
    [Ignore("Proto messages are not supported yet")]
    public void DecodeAssign()
    {
        var proto = ResourceResolver.ReadAsString<ProtobufSchemaTester>("Protobuf/Person.proto");
        var schema = new ProtobufSchema<Person>(new StringReader(proto), "Person");
        var person = schema.DecoderDescriptor.IsObject(() => new Person());

        person
            .HasField("email", SetterHelper.Mutation((Person p, string v) => p.Email = v))
            .IsValue(Format.Protobuf.To.String);
        person
            .HasField("id", SetterHelper.Mutation((Person p, int v) => p.Id = v))
            .IsValue(Format.Protobuf.To.Integer32S);
        person
            .HasField("name", SetterHelper.Mutation((Person p, string v) => p.Name = v))
            .IsValue(Format.Protobuf.To.String);

        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream([16, 17, 0, 0, 0]);
        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out var entity), Is.True);
        Assert.That(17, Is.EqualTo(entity!.Id));
    }
}