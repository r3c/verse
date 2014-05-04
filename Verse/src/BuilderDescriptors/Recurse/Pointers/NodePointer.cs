using System;
using System.Collections.Generic;

namespace Verse.BuilderDescriptors.Recurse.Pointers
{
	class NodePointer<T, C, V> : IPointer<T, C, V>
	{
		public EnterCallback<T, C, V>	enter = null;

		public Dictionary<string, EnterCallback<T, C, V>>	fields = new Dictionary<string, EnterCallback<T, C, V>> ();

		public void Enter (T source, IWriter<C, V> writer, C context)
		{
			if (this.enter != null)
			{
				this.enter (source, writer, context);

				return;
			}

			foreach (KeyValuePair<string, EnterCallback<T, C, V>> field in this.fields)
				field.Value (source, writer, context);
		}
	}
}
