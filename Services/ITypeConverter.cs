using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services
{
    /// <summary>
    /// Interface to type conversion
    /// </summary>
    public interface ITypeConverter
    {
        /// <summary>
        /// Converts source value into target type
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="format">Format provider</param>
        /// <returns>Value of target type</returns>
        object convert(object sourceValue, Type targetType, IFormatProvider format = null);

        /// <summary>
        /// Converts source value into T_Target
        /// </summary>
        /// <typeparam name="T_Target">Target type</typeparam>
        /// <param name="sourceValue">Source value</param>
        /// <returns></returns>
        T_Target convert<T_Target>(object sourceValue, IFormatProvider format = null);
    }
}
