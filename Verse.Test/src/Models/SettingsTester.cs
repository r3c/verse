#if false
using System;
using System.Collections.Generic;
using System.IO;

using NUnit.Framework;

using Verse.Models.JSON;
using Verse.Test.Helpers;

namespace Verse.Test.Models
{
	[TestFixture]
	class SettingsTester
	{
		private ISchema	GetSchema (JSONSettings settings)
		{
			JSONSchema	schema;

			schema = new JSONSchema (settings);
			schema.OnStreamError += (position, message) => Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			return schema;
		}

		private struct	EncoderTest
		{
			public string						attribute;
			public string[]						elements;
			public Dictionary<string, string>	pairs;
		}

		[Test]
		public void EncoderNoNullAttribute ()
		{
			IEncoder<EncoderTest>	encoder;
			EncoderTest				instance;
			ISchema					schema;

			instance = new EncoderTest {attribute = null, elements = new string[] {"a", null, "b"}, pairs = new Dictionary<string, string> {{"x", "1"}, {"y", null}}};
			schema = this.GetSchema (JSONSettings.NoNullAttribute);

			encoder = schema.GetEncoder<EncoderTest> ();
			encoder.Link ();

			EncoderValidator.Validate (encoder, instance, "{\"elements\":[\"a\",null,\"b\"],\"pairs\":{\"x\":\"1\",\"y\":null}}");
		}

		[Test]
		public void EncoderNoNullAttributeNoNullValue ()
		{
			IEncoder<EncoderTest>	encoder;
			EncoderTest				instance;
			ISchema					schema;

			instance = new EncoderTest {attribute = null, elements = new string[] {"a", null, "b"}, pairs = new Dictionary<string, string> {{"x", "1"}, {"y", null}}};
			schema = this.GetSchema (JSONSettings.NoNullAttribute | JSONSettings.NoNullValue);

			encoder = schema.GetEncoder<EncoderTest> ();
			encoder.Link ();

			EncoderValidator.Validate (encoder, instance, "{\"elements\":[\"a\",\"b\"],\"pairs\":{\"x\":\"1\"}}");
		}

		[Test]
		public void EncoderNoNullValue ()
		{
			IEncoder<EncoderTest>	encoder;
			EncoderTest				instance;
			ISchema					schema;

			instance = new EncoderTest {attribute = null, elements = new string[] {"a", null, "b"}, pairs = new Dictionary<string, string> {{"x", "1"}, {"y", null}}};
			schema = this.GetSchema (JSONSettings.NoNullValue);

			encoder = schema.GetEncoder<EncoderTest> ();
			encoder.Link ();

			EncoderValidator.Validate (encoder, instance, "{\"attribute\":null,\"elements\":[\"a\",\"b\"],\"pairs\":{\"x\":\"1\"}}");
		}
	}
}
#endif