using System;
using System.Collections.Generic;
using System.Globalization;
using TypeUtils.Extensions;

namespace TypeUtils.Services.Impl
{
    public class TypeConverter: ITypeConverter
    {
        /// <summary>
        ///  Custom converter methods
        /// </summary>
        private Dictionary<Tuple<Type, Type>, CustomConverterDelegate> _customConverters 
            = new Dictionary<Tuple<Type, Type>, CustomConverterDelegate>();

        private readonly object _customConvertersLock = new object();

        /// <summary>
        /// Statically created converter
        /// </summary>
        public static ITypeConverter Current {
            get
            {
                lock (_CurrentLock)
                {
                    if (_Current == null)
                        _Current = new TypeConverter();

                    return _Current;
                }
            }
        }

        private static ITypeConverter _Current;
        private static readonly object _CurrentLock = new object();

        /// <summary>
        /// Construction
        /// </summary>
        public TypeConverter()
        {
            // Register some useful converters
            registerCustomConverter<string, Uri>(stringToUri);
            registerCustomConverter<string, Guid>(stringToEnum);
            registerCustomConverter<string, Guid>(stringToGuid);
            registerCustomConverter<double, DateTime>(doubleToDateTime);
        }

        /// <summary>
        /// Registers custom converter for T_Source to T_Target conversion
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="converter"></param>
        public void registerCustomConverter<T_Source, T_Target> (CustomConverterDelegate converter)
        {
            registerCustomConverter(typeof(T_Source), typeof(T_Target), converter);
        }

        /// <summary>
        /// Registers custom converter for sourceType to targetType conversion
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        /// <param name="converter"></param>
        public void registerCustomConverter(Type sourceType, Type targetType, CustomConverterDelegate converter)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));
            if (converter == null)
                throw new ArgumentNullException(nameof(converter));

            lock (_customConvertersLock)
            {
                // Add customer converter. Override previous registered converters with same key.
                _customConverters[Tuple.Create(sourceType, targetType)]
                    = converter;
            }
        }

        /// <summary>
        /// Remove registration
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        public void deregister<T_Source, T_Target>()
        {
            deregister(typeof(T_Source), typeof(T_Target));
        }

        /// <summary>
        /// Remove registration
        /// </summary>
        /// <param name="sourceType"></param>
        /// <param name="targetType"></param>
        public void deregister(Type sourceType, Type targetType)
        {
            if (sourceType == null)
                throw new ArgumentNullException(nameof(sourceType));
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            lock (_customConvertersLock)
            {
                _customConverters.Remove(Tuple.Create(sourceType, targetType));
            }

        }

        /// <summary>
        /// Converts source value into T_Target
        /// </summary>
        /// <typeparam name="T_Target">Target type</typeparam>
        /// <param name="sourceValue"></param>
        /// <returns></returns>
        public T_Target convert<T_Target>(object sourceValue, IFormatProvider format = null)
        {
            return (T_Target)convert(sourceValue, typeof(T_Target), format);
        }

        /// <summary>
        /// Converts source value into target type
        /// </summary>
        /// <param name="sourceValue">Source value</param>
        /// <param name="targetType">Target type</param>
        /// <param name="format">Format provider</param>
        /// <returns>Value of target type</returns>
        public object convert(object sourceValue, Type targetType, IFormatProvider format = null)
        {
            if (targetType == null)
                throw new ArgumentNullException(nameof(targetType));

            format = format ?? CultureInfo.CurrentCulture;

            object targetValue = null;

            // Is target nullable ?
            var underlyingTargetType = Nullable.GetUnderlyingType(targetType);

            // Handle null values
            if (sourceValue == null)
            {
                // Nullable types will be null
                if (underlyingTargetType != null)
                    targetValue = null;

                // Non nullable types will be default 
                else
                    targetValue = targetType.GetDefaultValue();
            }
            else
            {
                var sourceType = sourceValue.GetType();

                var realTargetType = underlyingTargetType ?? targetType;

                var customConverterMethod = findCustomConverterMethod(sourceType, realTargetType);

                if (customConverterMethod != null)
                {
                    // Custom conversion
                    targetValue = customConverterMethod(sourceValue, realTargetType, format);
                }
                else if (realTargetType.IsEnum)
                {
                    // Enum special behavior
                    targetValue = stringToEnum(sourceValue, realTargetType, format);
                }
                else
                {
                    // Default conversion
                    targetValue = Convert.ChangeType(sourceValue, realTargetType, format);
                }
            }

            return targetValue;
        }

        private CustomConverterDelegate findCustomConverterMethod(Type from, Type to)
        {
            lock (_customConvertersLock)
            {
                CustomConverterDelegate converter = null;

                if (_customConverters.TryGetValue(Tuple.Create(from, to), out converter))
                {
                    return converter;
                }
                else
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// String to guid conversion
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private object stringToGuid(object sourceValue, Type targetType, IFormatProvider format)
        {
            return Guid.Parse((string)sourceValue);
        }

        /// <summary>
        /// String to enum conversion
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private object stringToEnum(object sourceValue, Type targetType, IFormatProvider format)
        {
            return Enum.Parse(targetType, sourceValue.ToString());
        }

        /// <summary>
        /// String to uri conversion
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private object stringToUri(object sourceValue, Type targetType, IFormatProvider format)
        {
            Uri uri = null;

            if (!Uri.TryCreate((string)sourceValue, UriKind.Absolute, out uri))
                Uri.TryCreate((string)sourceValue, UriKind.Relative, out uri);

            return uri;
        }

        /// <summary>
        /// Ole date to datetime conversion
        /// </summary>
        /// <param name="sourceValue"></param>
        /// <param name="targetType"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        private object doubleToDateTime(object sourceValue, Type targetType, IFormatProvider format)
        {
            return DateTime.FromOADate((double)sourceValue);
        }
        
    }
}
