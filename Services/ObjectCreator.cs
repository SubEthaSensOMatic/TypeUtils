using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace TypeUtils.Services
{
    public static class ObjectCreator<T_Create> where T_Create : new()
    {
        public delegate T FactoryDelegate<T>();

        public static FactoryDelegate<T_Create> Create = CreateFactory();

        private static FactoryDelegate<T_Create> CreateFactory()
        {
            ConstructorInfo ctor = typeof(T_Create)
                .GetConstructors()
                .Where(c => c.GetParameters().Length == 0)
                .FirstOrDefault();

            if (ctor == null)
                throw new InvalidOperationException(typeof(T_Create).Name + " has no default constructor!");
        
            var lambda = Expression.Lambda(
                typeof(FactoryDelegate<T_Create>), Expression.New(ctor));

            return (FactoryDelegate<T_Create>)lambda.Compile();
        }
    }
}
