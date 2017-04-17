namespace Verse.Schemas.Protobuf
{
	public struct Value
	{
		#region Attributes

		public readonly double DoubleContent;

		public readonly float FloatContent;

		public readonly long LongContent;

		public readonly string StringContent;

		public readonly ContentType Type;

		#endregion

		#region Constructor

		public Value(double value)
		{
			this.DoubleContent = value;
			this.FloatContent = 0f;
			this.LongContent = 0;
			this.StringContent = string.Empty;

			this.Type = ContentType.Double;
		}

		public Value(float value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = value;
			this.LongContent = 0;
			this.StringContent = string.Empty;

			this.Type = ContentType.Float;
		}

		public Value(long value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = 0f;
			this.LongContent = value;
			this.StringContent = string.Empty;

			this.Type = ContentType.Long;
		}

		public Value(string value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = 0f;
			this.LongContent = 0;
			this.StringContent = value;

			this.Type = ContentType.String;
		}

		#endregion
	}
}