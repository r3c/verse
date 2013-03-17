using System;
using System.Runtime.Serialization;

namespace Verse.Exceptions
{
	public class BindTypeException : Exception, ISerializable
	{
		#region Properties

		public Type	Type
		{
			get
			{
				return this.type;
			}
		}
		
		#endregion
		
		#region Attributes

		private Type	type;
		
		#endregion
		
		#region Constructors / Public

		public BindTypeException (Type type, string message, Exception innerException) :
			base (message, innerException)
		{
			this.type = type;
		}

		public BindTypeException (Type type, string message) :
			base (message)
		{
			this.type = type;
		}
		
		#endregion
		
		#region Constructors / Protected

		protected	BindTypeException (SerializationInfo info, StreamingContext context) :
			base (info, context)
		{
		}
		
		#endregion
	}
}
