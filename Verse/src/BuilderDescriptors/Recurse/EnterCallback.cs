
namespace Verse.BuilderDescriptors.Recurse
{
	public delegate void EnterCallback<T, C, V> (T source, IWriter<C, V> writer, C context);
}
