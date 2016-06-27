using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services
{
    public interface IPropertyMapperFactory
    {
        /// <summary>
        /// Create mapper for mapping definition. Mapper should be cached. Creating a mapper
        /// is an expensive operation!
        /// </summary>
        /// <typeparam name="T_Source">Source type</typeparam>
        /// <typeparam name="T_Target">target type</typeparam>
        /// <param name="mapping">Mapping definition</param>
        /// <returns>Interface to perperty mapper</returns>
        IPropertyMapper<T_Source, T_Target> createPropertyMapper<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping);
    }
}
