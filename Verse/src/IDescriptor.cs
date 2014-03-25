namespace Verse
{
    public interface IDescriptor<T>
    {
        #region Methods

        IDescriptor<U>	ForChildren<U>(DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create);

        IDescriptor<U>	ForChildren<U>(DescriptorAssign<T, U> assign);

        IDescriptor<T>  ForChildren();

        IDescriptor<U>	ForField<U>(string name, DescriptorAssign<T, U> assign, DescriptorCreate<T, U> create);

        IDescriptor<U>	ForField<U>(string name, DescriptorAssign<T, U> assign);

        IDescriptor<T>  ForField(string name);

        void            LetValue<U>(DescriptorAssign<T, U> assign);

        #endregion
    }
}

