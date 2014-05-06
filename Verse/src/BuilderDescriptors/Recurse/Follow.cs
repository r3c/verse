
namespace Verse.BuilderDescriptors.Recurse
{
	public delegate void Follow<T, C, V> (T source, IWriter<C, V> writer, C context);
}
