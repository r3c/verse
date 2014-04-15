namespace Verse
{
    public interface IParserDescriptor<T>
    {
        #region Methods

        IParserDescriptor<U>	HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse);

        IParserDescriptor<U>	HasChildren<U> (DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        IParserDescriptor<U>	HasChildren<U> (DescriptorSet<T, U> store);

        IParserDescriptor<T>	HasChildren ();

        IParserDescriptor<U>	HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create, IParserDescriptor<U> recurse);

        IParserDescriptor<U>	HasField<U> (string name, DescriptorSet<T, U> store, DescriptorGet<T, U> create);

        IParserDescriptor<U>	HasField<U> (string name, DescriptorSet<T, U> store);

        IParserDescriptor<T>	HasField (string name);

		void					IsValue<U> (DescriptorSet<T, U> store);

        void					IsValue ();

        #endregion
    }
}
