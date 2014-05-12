
namespace Verse.ParserDescriptors.Recurse
{
	delegate bool Follow<T, C, V> (ref T target, IReader<C, V> reader, C context);
}
