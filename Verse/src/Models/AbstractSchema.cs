using System;
using System.Collections.Generic;

using Verse.Dynamics;
using Verse.Events;

namespace Verse.Models
{
	public abstract class AbstractSchema : ISchema
	{
		#region Events
		
		public event StreamErrorEvent	OnStreamError;
		
		public event TypeErrorEvent		OnTypeError;

		#endregion
		
		#region Methods / Abstract

		public abstract IDecoder<T>	GetDecoder<T> (Func<T> constructor);

		public abstract IEncoder<T>	GetEncoder<T> ();
		
		#endregion

		#region Methods / Public

		public IDecoder<T>	GetDecoder<T> ()
		{
			return this.GetDecoder (ConstructorGenerator.Generate<T> ());
		}

		#endregion
		
		#region Methods / Protected
		
		protected void	EventStreamError (long position, string message)
		{
			StreamErrorEvent	error;

			error = this.OnStreamError;

			if (error != null)
				error (position, message);
		}

		protected void	EventTypeError (Type type, string value)
		{
			TypeErrorEvent	error;

			error = this.OnTypeError;

			if (error != null)
				error (type, value);
		}
		
		#endregion
	}
}
