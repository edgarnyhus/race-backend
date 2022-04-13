using Domain.Models;

namespace Infrastructure.Data.Repositories.Helpers
{
    public class GenericPropery<T>
    {
        private T _value;

        public T Value
        {
            get
            {
                // insert desired logic here
                return _value;
            }
            set
            {
                // insert desired logic here
                _value = value;
            }
        }

        public static implicit operator T(GenericPropery<T> value)
        {
            return value.Value;
        }

        public static implicit operator GenericPropery<T>(T value)
        {
            return new GenericPropery<T> { Value = value };
        }
    }
    
    class SomeClass
    {
        public GenericPropery<SignType> SomeProperty { get; set; }
    }
    
    public class GProperty<T>
    {
        public T Property { get; set; }
    }

}