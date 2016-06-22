using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services
{
    public class MappingRule
    {
        public string Source { get; set; }

        public string Target { get; set; }

        public IFormatProvider Format { get; set; }
    }

    internal delegate void SetterDelegate(object target, object value);
    internal delegate object GetterDelegate(object target);

    public class ObjectMapper
    {
        private class PropertyAccessors
        {
            public string PropertyName { get; set; }
            public Type PropertyType { get; set; }
            public SetterDelegate Setter { get; set; }
            public GetterDelegate Getter { get; set; }
        }

        private Dictionary<Type, Dictionary<string, PropertyAccessors>> _propertyLookup 
            = new Dictionary<Type, Dictionary<string, PropertyAccessors>>();

        private readonly object _propertyLookupLock = new object();

        /// <summary>
        /// Statically created mapper
        /// </summary>
        public static ObjectMapper Current
        {
            get
            {
                lock (_CurrentLock)
                {
                    if (_Current == null)
                        _Current = new ObjectMapper();

                    return _Current;
                }
            }
        }

        private static ObjectMapper _Current;
        private static readonly object _CurrentLock = new object();


        /// <summary>
        /// Tries to map all public properties from target with values from source
        /// </summary>
        /// <typeparam name="T_Source">Source type</typeparam>
        /// <typeparam name="T_Target">Target tape</typeparam>
        /// <param name="source"></param>
        /// <param name="target"></param>
        public void map<T_Source, T_Target>(T_Source source, T_Target target)
        {
            var sourceLookup = getPropertyLookup(typeof(T_Source));
            var targetLookup = getPropertyLookup(typeof(T_Target));

            PropertyAccessors sourceAccessor = null;

            foreach (var targetAccessor in targetLookup.Values)
            {
                if (!sourceLookup.TryGetValue(targetAccessor.PropertyName, out sourceAccessor))
                    continue;

                mapValue(sourceAccessor, source, targetAccessor, target);
            }
        }

        /// <summary>
        /// Maps single value with type conversion
        /// </summary>
        /// <param name="sourceAccessor"></param>
        /// <param name="source"></param>
        /// <param name="targetAccessor"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        private bool mapValue(PropertyAccessors sourceAccessor, object source, PropertyAccessors targetAccessor, object target)
        {
            if (targetAccessor.Setter == null)
                return false;

            if (sourceAccessor.Getter == null)
                return false;

            if (targetAccessor.PropertyType.IsAssignableFrom(sourceAccessor.PropertyType))
            {
                targetAccessor.Setter(target, sourceAccessor.Getter(source));
            }
            else
            {
                var sourceValue = sourceAccessor.Getter(source);

                var targetValue = TypeConverter.Current.convert(
                    sourceValue, targetAccessor.PropertyType);

                targetAccessor.Setter(target, targetValue);
            }

            return true;
        }

        /// <summary>
        /// Retrieve cached property accessors for type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private Dictionary<string, PropertyAccessors> getPropertyLookup(Type type)
        {
            Dictionary<string, PropertyAccessors> result = null;

            lock (_propertyLookupLock)
            {
                if (!_propertyLookup.TryGetValue(type, out result))
                {
                    result = new Dictionary<string, PropertyAccessors>();

                    foreach (var propertyInfo in type.GetProperties())
                    {

                        var setter = getSetter(propertyInfo);
                        var getter = getGetter(propertyInfo);

                        if (getter == null && setter == null)
                            continue;

                        var accessors = new PropertyAccessors()
                        {
                            Getter = getter,
                            Setter = setter,
                            PropertyName = propertyInfo.Name,
                            PropertyType = propertyInfo.PropertyType
                        };

                        result.Add(propertyInfo.Name, accessors);
                    }

                    _propertyLookup.Add(type, result);
                }
            }
            return result;
        }

        private SetterDelegate getSetter(PropertyInfo property)
        {
            var setMethodInfo = property.GetSetMethod(false);
            if (setMethodInfo == null)
                return null;

            var method = new DynamicMethod("set", null, 
                new[] { typeof(object), typeof(object) }, true);

            var gen = method.GetILGenerator();

            // Load target object to stack
            gen.Emit(OpCodes.Ldarg_0); 

            // Cast target object 
            gen.Emit(OpCodes.Castclass, property.DeclaringType);

            // Load value to stack
            gen.Emit(OpCodes.Ldarg_1);

            // Unboxing
            gen.Emit(OpCodes.Unbox_Any, property.PropertyType);

            // Call setter of target property
            gen.Emit(OpCodes.Callvirt, setMethodInfo);

            // Exit
            gen.Emit(OpCodes.Ret);

            return (SetterDelegate)method.CreateDelegate(typeof(SetterDelegate));
        }

        private GetterDelegate getGetter(PropertyInfo property)
        {
            var getMethodInfo = property.GetGetMethod(false);
            if (getMethodInfo == null)
                return null;

            var method = new DynamicMethod("get", typeof(object),
                new[] { typeof(object) }, true);

            var gen = method.GetILGenerator();

            // Load target object to stack
            gen.Emit(OpCodes.Ldarg_0);

            // Cast target object 
            gen.Emit(OpCodes.Castclass, property.DeclaringType);

            // Call setter of target property
            gen.Emit(OpCodes.Callvirt, getMethodInfo);

            // Unboxing
            gen.Emit(OpCodes.Box, property.PropertyType);

            // Exit
            gen.Emit(OpCodes.Ret);

            return (GetterDelegate)method.CreateDelegate(typeof(GetterDelegate));
        }

    }
}
