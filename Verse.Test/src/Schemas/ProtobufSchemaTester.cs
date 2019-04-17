using System.IO;
using NUnit.Framework;
using Verse.Schemas;

namespace Verse.Test.Schemas
{
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

			Assert.NotNull(schema);
		}

		private class Person
		{
			public string Email;
			public int Id;
			public string Name;
		}

		[Test]
		[Ignore("Proto messages are not supported yet")]
		public void DecodeAssign()
		{
			var proto = File.ReadAllText(Path.Combine(TestContext.CurrentContext.TestDirectory, "res/Protobuf/Person.proto"));
			var schema = new ProtobufSchema<Person>(new StringReader(proto), "Person");

			var person = schema.DecoderDescriptor.IsObject(() => new Person());
			person.HasField("email", (ref Person p, string v) => p.Email = v).IsValue();
			person.HasField("id", (ref Person p, int v) => p.Id = v).IsValue();
			person.HasField("name", (ref Person p, string v) => p.Name = v).IsValue();

			var decoder = schema.CreateDecoder();

			using (var stream = new MemoryStream(new byte[] { 16, 17, 0, 0, 0 }))
			{
				Assert.True(decoder.TryOpen(stream, out var decoderStream));
				Assert.True(decoderStream.Decode(out var entity));

				Assert.AreEqual(17, entity.Id);
			}
		}	
	}
}