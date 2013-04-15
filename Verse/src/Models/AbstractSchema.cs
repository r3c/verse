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

		protected abstract AbstractDecoder<T>	CreateDecoder<T> (Func<T> constructor);

		protected abstract AbstractEncoder<T>	CreateEncoder<T> ();
		
		#endregion

		#region Methods / Public

		public IDecoder<T>	GetDecoder<T> (Func<T> constructor)
		{
			AbstractDecoder<T>	decoder;

			if (constructor == null)
				throw new ArgumentNullException ("constructor");

			decoder = this.CreateDecoder (constructor);
			decoder.OnStreamError += this.EventStreamError;
			decoder.OnTypeError += this.EventTypeError;

			return decoder;
		}

		public IDecoder<T>	GetDecoder<T> ()
		{
			return this.GetDecoder (Generator.Constructor<T> ());
		}

		public IEncoder<T>	GetEncoder<T> ()
		{
			AbstractEncoder<T>	encoder;

			encoder = this.CreateEncoder<T> ();
			encoder.OnStreamError += this.EventStreamError;
			encoder.OnTypeError += this.EventTypeError;

			return encoder;
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
