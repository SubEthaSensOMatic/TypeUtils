using System;
using System.Collections.Generic;
using System.Text;

namespace TypeUtils.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// All integer types
        /// </summary>
        private static readonly HashSet<Type> _IntegerTypes = new HashSet<Type>() {
            typeof(byte),
            typeof(byte?),
            typeof(sbyte),
            typeof(sbyte?),
            typeof(short),
            typeof(short?),
            typeof(ushort),
            typeof(ushort?),
            typeof(int),
            typeof(int?),
            typeof(uint),
            typeof(uint?),
            typeof(long),
            typeof(long?),
            typeof(ulong),
            typeof(ulong?)};

        /// <summary>
        /// All numeric types
        /// </summary>
        private static readonly HashSet<Type> _NumericTypes = new HashSet<Type>() {
            typeof(float),
            typeof(float?),
            typeof(double),
            typeof(double?),
            typeof(decimal),
            typeof(decimal?) };

        /// <summary>
        /// All text types
        /// </summary>
        private static readonly HashSet<Type> _TextTypes = new HashSet<Type>() {
            typeof(char),
            typeof(char?),
            typeof(string),
            typeof(StringBuilder) };

        /// <summary>
        /// Test if type is integer type
        /// </summary>
        /// <param name="type">Type to test</param>
        /// <returns></returns>
        public static bool IsIntegerType(this Type type)
        {
            return _IntegerTypes.Contains(type);
        }

        /// <summary>
        /// Test if type is numeric type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsNumericType(this Type type)
        {
            return _NumericTypes.Contains(type);
        }

        /// <summary>
        /// Test if type is texz type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static bool IsTextType(this Type type)
        {
            return _TextTypes.Contains(type);
        }

        /// <summary>
        /// Get default value for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static object GetDefaultValue(this Type type)
        {
            return type.IsValueType ? Activator.CreateInstance(type) : null;
        }
    }
}
