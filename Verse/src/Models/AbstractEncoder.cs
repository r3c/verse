using System;
using System.IO;

using Verse.Events;

namespace Verse.Models
{
	abstract class	AbstractEncoder<T> : IEncoder<T>
	{
		#region Events
		
		public event StreamErrorEvent	OnStreamError;

		public event TypeErrorEvent		OnTypeError;
		
		#endregion

		#region Methods / Abstract

		public abstract void		Bind (Func<T> builder);

		public abstract void		Bind ();

		public abstract bool		Encode (Stream stream, T instance);

		public abstract IEncoder<U>	HasField<U> (string name, EncoderValueGetter<T, U> getter);

		public abstract IEncoder<U>	HasItems<U> (EncoderArrayGetter<T, U> getter);

		public abstract IEncoder<U>	HasPairs<U> (EncoderMapGetter<T, U> getter);

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
