using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services
{
    /// <summary>
    /// Custom conversion method
    /// </summary>
    /// <param name="sourceValue"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public delegate object CustomConverterDelegate(object sourceValue, Type targetType, IFormatProvider format);    
    
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

        /// <summary>
        /// Registers custom converter for T_Source to T_Target conversion
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="converter"></param>
        void registerCustomConverter<T_Source, T_Target>(CustomConverterDelegate converter);

        /// <summary>
        /// Registers custom converter for sourceType to targetType conversion
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="converter"></param>
        void registerCustomConverter(Type sourceType, Type targetType, CustomConverterDelegate converter);

        /// <summary>
        /// Remove registration
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        void deregister<T_Source, T_Target>();

        /// <summary>
        /// Remove registration
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        void deregister(Type sourceType, Type targetType);
    }
}
