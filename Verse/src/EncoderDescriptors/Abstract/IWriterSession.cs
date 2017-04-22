﻿using System;
using System.IO;

namespace Verse.EncoderDescriptors.Abstract
{
	interface IWriterSession<TState>
	{
		bool Start(Stream stream, EncodeError error, out TState state);

		void Stop(TState state);
	}
}