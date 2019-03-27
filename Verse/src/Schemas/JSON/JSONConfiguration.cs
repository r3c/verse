using System.Text;

namespace Verse.Schemas.JSON
{
	public struct JSONConfiguration
	{
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
