
#if false
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

using Verse.Models.JSON;
using Verse.Test.Helpers;

namespace Verse.UTest.Models
{
	[TestFixture]
	class RecursiveSchemaTester
	{
		private ISchema	GetSchema ()
		{
			JSONSchema	schema;

			schema = new JSONSchema (0);
			schema.OnStreamError += (position, message) => Console.Error.WriteLine ("Stream error at position {0}: {1}", position, message);
			schema.OnTypeError += (type, value) => Console.Error.WriteLine ("Type error: could not convert \"{1}\" to {0}", type, value);

			return schema;
		}

		private class	MyNestedArray
		{
			public MyNestedArray[]	children;
			public string			value;
		}

		[Test]
		public void	NestedArray ()
		{
			IDecoder<MyNestedArray>	decoder;
			IEncoder<MyNestedArray>	encoder;
			ISchema					schema;

			schema = this.GetSchema ();

			decoder = schema.GetDecoder<MyNestedArray> ();
			decoder.Link ();

			encoder = schema.GetEncoder<MyNestedArray> ();
			encoder.Link ();

			SchemaValidator.Validate (decoder, encoder, new MyNestedArray
			{
				children = new MyNestedArray[]
				{
					new MyNestedArray
					{
						children = null,
						value = "a"
					},
					new MyNestedArray
					{
						children = new MyNestedArray[]
						{
							new MyNestedArray
							{
								children = null,
								value = "b"
							},
							new MyNestedArray
							{
								children = null,
								value = "c"
							}
						},
						value = "d"
					},
					new MyNestedArray
					{
						children = new MyNestedArray[0],
						value = "e"
					}
				},
				value = "f"
			});
		}

		private class	MyNestedValue
		{
			public MyNestedValue	child;
			public int				value;
		}

		[Test]
		public void	NestedField ()
		{
			IDecoder<MyNestedValue>	decoder;
			IEncoder<MyNestedValue>	encoder;
			ISchema					schema;

			schema = this.GetSchema ();

			decoder = schema.GetDecoder<MyNestedValue> ();
			decoder.Link ();

			encoder = schema.GetEncoder<MyNestedValue> ();
			encoder.Link ();

			SchemaValidator.Validate (decoder, encoder, new MyNestedValue
			{
				child	= new MyNestedValue
				{
					child	= new MyNestedValue
					{
						child	= null,
						value	= 64
					},
					value	= 42
				},
				value	= 17
			});
		}
	}
}
#endif