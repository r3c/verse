using System;

namespace Verse.Schemas.JSON
{
	public struct Value
	{
		#region Attributes / Instance

		public bool Boolean;

		public double Number;

		public string String;

		public Content Type;

		#endregion

		#region Attributes / Static

		public readonly static Value Void = new Value ();

		#endregion

		#region Methods

		public static Value FromBoolean (bool value)
		{
			return new Value { Boolean = value, Type = Content.Boolean };
		}

		public static Value FromNumber (double value)
		{
			return new Value { Number = value, Type = Content.Number };
		}

		public static Value FromString (string value)
		{
			return new Value { String = value, Type = Content.String };
		}

		#endregion
	}
}
