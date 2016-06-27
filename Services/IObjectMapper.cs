using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services
{
    /// <summary>
    /// Target object factory
    /// </summary>
    /// <returns></returns>
    public delegate object TargetFactoryDelegate();

    public interface IObjectMapper
    {
        /// <summary>
        /// Add mapping
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="factory">Target object factory</param>
        void registerMapping<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping, TargetFactoryDelegate factory = null);

        /// <summary>
        /// Remove mapping
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="mapping"></param>
        void deregisterMapping<T_Source, T_Target>();

        /// <summary>
        /// Map enumeration of sourc objects to target objects
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        IEnumerable<T_Target> map<T_Source, T_Target>(ICollection<T_Source> source);

        /// <summary>
        /// Parallely map enumeration of sourc objects to target objects. Target objects
        /// ordering will be same as source objects.
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        IEnumerable<T_Target> mapParallel<T_Source, T_Target>(IList<T_Source> source);
    }
}
