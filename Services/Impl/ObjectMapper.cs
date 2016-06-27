using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TypeUtils.Services.Impl
{
    public class ObjectMapper: IObjectMapper
    {
        private class MapperHolder
        {
            public IPropertyMapper Mapper { get; set; }

            public TargetFactoryDelegate TargetFactory { get; set; }
        }

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


        private Dictionary<Tuple<Type, Type>, MapperHolder> _propertyMapperLookup 
            = new Dictionary<Tuple<Type, Type>, MapperHolder>();
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

            // Factory function should be set or T_Target must have default constructor
            if (factory == null)
                factory = () => ObjectCreator<T_Target>.Create();

            lock (_propertyMapperLookupLock)
            {
                var key = Tuple.Create(typeof(T_Source), typeof(T_Target));

                if (_propertyMapperLookup.ContainsKey(key))
                    throw new InvalidOperationException("Mapping already registerd!");

                // Create mapper
                var propertyMapper = _propertyMapperFactory
                    .createPropertyMapper<T_Source, T_Target>(mapping);

                // cache mapper
                _propertyMapperLookup[key] = new MapperHolder()
                {
                    Mapper = propertyMapper,
                    TargetFactory = factory
                };
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
        public IEnumerable<T_Target> map<T_Source, T_Target>(IEnumerable<T_Source> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var result = new List<T_Target>();

            var mapper = getMapper<T_Source, T_Target>();
            foreach (var obj in source)
            {
                var newObj = mapper.TargetFactory();
                mapper.Mapper.map(obj, newObj);
                result.Add((T_Target)newObj);
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
        public IEnumerable<T_Target> mapParallel<T_Source, T_Target>(IEnumerable<T_Source> source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            var sourceLst = source.ToArray();

            var result = new T_Target[sourceLst.Length];

            var mapper = getMapper<T_Source, T_Target>();
            Parallel.For(0, sourceLst.Length, i =>
            {
                var newObj = mapper.TargetFactory();
                mapper.Mapper.map(sourceLst[i], newObj);
                result[i] = (T_Target)newObj;
            });

            return result;
        }

        private MapperHolder getMapper<T_Source, T_Target>()
        {
            MapperHolder mapper = null;

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
