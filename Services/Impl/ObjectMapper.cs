using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services.Impl
{
    public class ObjectMapper: IObjectMapper
    {
        /// <summary>
        /// Statically created mapper
        /// </summary>
        public static IObjectMapper Current
        {
            get
            {
                lock (_CurrentLock)
                {
                    if (_Current == null)
                        _Current = new ObjectMapper(new PropertyMapperFactory());

                    return _Current;
                }
            }
        }

        private static IObjectMapper _Current;
        private static readonly object _CurrentLock = new object();


        private Dictionary<Tuple<Type, Type>, IPropertyMapper> _propertyMapperLookup 
            = new Dictionary<Tuple<Type, Type>, IPropertyMapper>();
        private readonly object _propertyMapperLookupLock = new object();

        private IPropertyMapperFactory _propertyMapperFactory;

        /// <summary>
        /// Construct mapper
        /// </summary>
        /// <param name="propertyMapperFactory"></param>
        public ObjectMapper(IPropertyMapperFactory propertyMapperFactory)
        {
            if (propertyMapperFactory == null)
                throw new ArgumentNullException(nameof(propertyMapperFactory));

            _propertyMapperFactory = propertyMapperFactory;
        }

        /// <summary>
        /// Add mapping
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="mapping"></param>
        /// <param name="factory">Target object factory</param>
        public void registerMapping<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping, TargetFactoryDelegate factory = null)
        {
            if (mapping == null)
                throw new ArgumentNullException(nameof(mapping));

            lock (_propertyMapperLookupLock)
            {
                var key = Tuple.Create(typeof(T_Source), typeof(T_Target));

                if (_propertyMapperLookup.ContainsKey(key))
                    throw new InvalidOperationException("Mapping already registerd!");

                // Create mapper
                var propertyMapper = _propertyMapperFactory
                    .createPropertyMapper(mapping);

                // cache mapper
                _propertyMapperLookup[key] = propertyMapper;
            }
        }

        /// <summary>
        /// Remove mapping
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="mapping"></param>
        public void deregisterMapping<T_Source, T_Target>()
        {
            lock (_propertyMapperLookupLock)
            {
                _propertyMapperLookup.Remove(
                    Tuple.Create(typeof(T_Source), typeof(T_Target)));
            }
        }

        /// <summary>
        /// Map enumeration of sourc objects to target objects
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>
        public IEnumerable<T_Target> map<T_Source, T_Target>(ICollection<T_Source> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<T_Target> (source.Count);

            var mapper = getMapper<T_Source, T_Target>();
            object target = null;

            foreach (var obj in source)
            {
                target = mapper.createTarget();
                mapper.map(obj, target);
                result.Add((T_Target)target);
            }

            return result;
        }

        /// <summary>
        /// Parallely map enumeration of sourc objects to target objects. Target objects
        /// ordering will be same as source objects.
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="source"></param>
        /// <returns></returns>        
        public IEnumerable<T_Target> mapParallel<T_Source, T_Target>(IList<T_Source> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new T_Target[source.Count];

            var mapper = getMapper<T_Source, T_Target>();
            object target = null;

            Parallel.For(0, source.Count, i =>
            {
                target = mapper.createTarget();
                mapper.map(source[i], target);
                result[i] = (T_Target)target;
            });

            return result;
        }

        private IPropertyMapper getMapper<T_Source, T_Target>()
        {
            IPropertyMapper mapper = null;

            lock (_propertyMapperLookupLock)
            {
                var key = Tuple.Create(typeof(T_Source), typeof(T_Target));

                if (!_propertyMapperLookup.TryGetValue(key, out mapper))
                    throw new InvalidOperationException("Mapping not registered!");
            }

            return mapper;
        }
    }
}
