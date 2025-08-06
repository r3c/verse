using System.IO;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test.Schemas;

[TestFixture]
internal class ProtobufSchemaTester
{
    [Test]
    //[TestCase("res/Protobuf/Example2.proto", "outer")]
    [TestCase("res/Protobuf/Example3.proto", "outer")]
    [TestCase("res/Protobuf/Person.proto", "Person")]
    public void Decode(string path, string messageName)
    {
        var proto = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, path));
        var schema = new ProtobufSchema<int>(new StringReader(proto), messageName);

        Assert.That(schema, Is.Not.Null);
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
        var proto = File.ReadAllText(
            Path.Combine(TestContext.CurrentContext.TestDirectory, "res/Protobuf/Person.proto"));
        var schema = new ProtobufSchema<Person>(new StringReader(proto), "Person");
        var root = schema.DecoderDescriptor.IsObject(() => new Person());

        root.HasField("email", SetterHelper.Mutation((Person p, string v) => p.Email = v))
            .IsValue(Format.Protobuf.To.String);
        root.HasField("id", SetterHelper.Mutation((Person p, int v) => p.Id = v))
            .IsValue(Format.Protobuf.To.Integer32S);
        root.HasField("name", SetterHelper.Mutation((Person p, string v) => p.Name = v))
            .IsValue(Format.Protobuf.To.String);

        var decoder = schema.CreateDecoder();

        using var stream = new MemoryStream([16, 17, 0, 0, 0]);
        using var decoderStream = decoder.Open(stream);

        Assert.That(decoderStream.TryDecode(out var entity), Is.True);
        Assert.That(17, Is.EqualTo(entity!.Id));
    }
}