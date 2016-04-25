namespace Verse.ParserDescriptors.Recurse
{
    delegate BrowserState BrowserMove<TEntity>(int index, out TEntity current);
}