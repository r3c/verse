namespace Verse.Schemas.Protobuf
{
	public struct ProtobufValue
	{
		#region Attributes

		public readonly double DoubleContent;

		public readonly float FloatContent;

		public readonly long LongContent;

		public readonly string StringContent;

		public readonly ProtobufType Type;

		#endregion

		#region Constructor

		public ProtobufValue(double value)
		{
			this.DoubleContent = value;
			this.FloatContent = 0f;
			this.LongContent = 0;
			this.StringContent = string.Empty;

			this.Type = ProtobufType.Double;
		}

		public ProtobufValue(float value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = value;
			this.LongContent = 0;
			this.StringContent = string.Empty;

			this.Type = ProtobufType.Float;
		}

		public ProtobufValue(long value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = 0f;
			this.LongContent = value;
			this.StringContent = string.Empty;

			this.Type = ProtobufType.Long;
		}

		public ProtobufValue(string value)
		{
			this.DoubleContent = 0.0;
			this.FloatContent = 0f;
			this.LongContent = 0;
			this.StringContent = value;

			this.Type = ProtobufType.String;
		}

		#endregion
	}
}