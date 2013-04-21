using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

using NUnit.Framework;

namespace Verse.Test.Models
{
	[TestFixture]
	class RecursiveSchemaTester : SchemaTester
	{
		private class	NestedArray
		{
			public NestedArray[]	children;
			public string			value;
		}

		[Test]
		public void	WithNestedArray (ISchema schema)
		{
			IDecoder<NestedArray>	decoder;
			IEncoder<NestedArray>	encoder;

			decoder = schema.GetDecoder<NestedArray> ();
			decoder.Link ();

			encoder = schema.GetEncoder<NestedArray> ();
			encoder.Link ();

			this.Validate (decoder, encoder, new NestedArray
			{
				children = new NestedArray[]
				{
					new NestedArray
					{
						children = null,
						value = "a"
					},
					new NestedArray
					{
						children = new NestedArray[]
						{
							new NestedArray
							{
								children = null,
								value = "b"
							},
							new NestedArray
							{
								children = null,
								value = "c"
							}
						},
						value = "d"
					},
					new NestedArray
					{
						children = new NestedArray[0],
						value = "e"
					}
				},
				value = "f"
			});
		}

		private class	NestedValue
		{
			public NestedValue	child;
			public int			value;
		}

		[Test]
		public void	WithNestedValue (ISchema schema)
		{
			IDecoder<NestedValue>	decoder;
			IEncoder<NestedValue>	encoder;

			decoder = schema.GetDecoder<NestedValue> ();
			decoder.Link ();

			encoder = schema.GetEncoder<NestedValue> ();
			encoder.Link ();

			this.Validate (decoder, encoder, new NestedValue
			{
				child	= new NestedValue
				{
					child	= new NestedValue
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
