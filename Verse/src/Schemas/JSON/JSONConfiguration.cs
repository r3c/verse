using System.Text;

namespace Verse.Schemas.JSON
{
	public struct JSONConfiguration
	{
		/// <summary>
		/// Accept object value as a valid input for descriptors declared as array, producing an array with unordered
		/// object values as array elements. Enabling this option takes priority over <see cref="AcceptValueAsArray"/>,
		/// meaning parsed objects (e.g. {"a": "x", "b": "y"}) will generate arrays using their values (e.g. ["x", "y"])
		/// instead of producing a 1-element array where the element is the object itself (e.g. [{"a": "x", "b": "y"}]).
		/// </summary>
		public bool AcceptObjectAsArray;

		/// <summary>
		/// Accept scalar value as valid input for descriptors declared as array, producing a 1-element array.
		/// </summary>
		public bool AcceptValueAsArray;

		/// <summary>
		/// Encoding used to read/write JSON text from/to binary stream.
		/// </summary>
		public Encoding Encoding;

		/// <summary>
		/// Do not write fields when their value is null.
		/// </summary>
		public bool OmitNull;
	}
}
