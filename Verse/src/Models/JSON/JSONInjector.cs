using System;

namespace Verse.Models.JSON
{
	delegate bool JSONInjector<T> (JSONPrinter printer, T value);
}
