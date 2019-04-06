namespace Verse.DecoderDescriptors.Tree
{
	delegate BrowserState BrowserMove<TEntity>(int index, out TEntity current);
}