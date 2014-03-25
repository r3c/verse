
namespace Verse
{
    public interface ISchema<T> : IDescriptor<T>
    {
        #region Methods

        bool Generate(out IParser<T> parser);

        #endregion
    }
}
