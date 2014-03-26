namespace Verse
{
    public interface IParserDescriptor<T>
    {
        #region Methods

        IParserDescriptor<U>	ForChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        IParserDescriptor<U>	ForChildren<U> (DescriptorSet<T, U> store);

        IParserDescriptor<T>	ForChildren (IParserDescriptor<T> descriptor);

        IParserDescriptor<T>	ForChildren ();

        IParserDescriptor<U>	ForField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        IParserDescriptor<U>	ForField<U> (string name, DescriptorSet<T, U> store);

        IParserDescriptor<T>	ForField (string name, IParserDescriptor<T> descriptor);

        IParserDescriptor<T>	ForField (string name);

        void					ForValue<U> (DescriptorSet<T, U> store);

        #endregion
    }
}
