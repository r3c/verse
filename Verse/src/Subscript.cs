using System;

namespace Verse
{
	public struct Subscript : IEquatable<Subscript>
	{
		#region Properties
		
		public int			AsInteger
		{
			get
			{
				return this.asInteger;
			}
		}

		public string		AsString
		{
			get
			{
				return this.asString;
			}
		}
		
		public ContentType	Type
		{
			get
			{
				return this.type;
			}
		}
		
		#endregion

		#region Attributes
		
		private int			asInteger;

		private string		asString;

		private ContentType	type;
		
		#endregion
		
		#region Constructors
		
		public	Subscript (int value)
		{
			this.asInteger = value;
			this.asString = string.Empty;
			this.type = ContentType.Integer;
		}

		public	Subscript (string value)
		{
			this.asInteger = 0;
			this.asString = value;
			this.type = ContentType.String;
		}
		
		#endregion
		
		#region Methods
		
		public override bool	Equals (object obj)
		{
			return obj is Subscript && this.Equals ((Subscript)obj);
		}
		
		public bool	Equals (Subscript other)
		{
			if (this.type != other.type)
				return false;

			switch (this.type)
			{
				case ContentType.Integer:
					return this.asInteger == other.asInteger;

				case ContentType.String:
					return this.asString == other.asString;

				default:
					return false;
			}
		}
		
		public override int	GetHashCode ()
		{
			switch (this.type)
			{
				case ContentType.Integer:
					return (0x00000000 & 0x60000000) | (this.asInteger.GetHashCode() & 0x1FFFFFFF);

				case ContentType.String:
					return (0x20000000 & 0x60000000) | (this.asString.GetHashCode() & 0x1FFFFFFF);

				default:
					return 0;
			}
		}
		
		public static bool	operator == (Subscript left, Subscript right)
		{
			return left.Equals(right);
		}
		
		public static bool	operator != (Subscript left, Subscript right)
		{
			return !left.Equals(right);
		}

		#endregion
		
		#region Types
		
		public enum	ContentType
		{
			Integer,
			String
		}
		
		#endregion
	}
}
