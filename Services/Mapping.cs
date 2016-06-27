using System;
using System.Collections.Generic;

namespace TypeUtils.Services
{
    /// <summary>
    /// Contains mapping rule from one property to another
    /// </summary>
    public sealed class MappingRule<T_Source, T_Target>
    {
        /// <summary>
        /// Source property name
        /// </summary>
        public string SourceProperty { get; internal set; }

        /// <summary>
        /// Source propery getter
        /// </summary>
        public Func<T_Source, T_Target, object> Getter { get; internal set; }

        /// <summary>
        /// Target property name
        /// </summary>
        public string TargetProperty { get; internal set; }

        /// <summary>
        /// Target property setter 
        /// </summary>
        public Action<T_Source, T_Target, object> Setter { get; internal set; }

        /// <summary>
        /// Format provider 
        /// </summary>
        public IFormatProvider Format { get; internal set; }
    }

    /// <summary>
    /// Defines mapping between two types
    /// </summary>
    /// <typeparam name="T_Source">Source type</typeparam>
    /// <typeparam name="T_Target">Target type</typeparam>
    public class Mapping<T_Source, T_Target>
    {
        /// <summary>
        /// Gets source type
        /// </summary>
        public Type SourceType { get { return typeof(T_Source); } }

        /// <summary>
        /// Gets target type
        /// </summary>
        public Type TargetType { get { return typeof(T_Target); } }

        /// <summary>
        /// Type conversion override
        /// </summary>
        public ITypeConverter Converter { get; set; }

        /// <summary>
        /// Gets number of rules
        /// </summary>
        public int Count => _rules.Count;

        /// <summary>
        /// Gets nth rule
        /// </summary>
        /// <param name="idx"></param>
        /// <returns></returns>
        public MappingRule<T_Source, T_Target> this[int idx] => _rules[idx];

        /// <summary>
        /// Rules
        /// </summary>
        private List<MappingRule<T_Source, T_Target>> _rules = new List<MappingRule<T_Source, T_Target>>();

        /// <summary>
        /// Map one property name to another property name
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="targetProperty"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public Mapping<T_Source, T_Target> map(string sourceProperty, string targetProperty, IFormatProvider format = null)
        {
            _rules.Add(new MappingRule<T_Source, T_Target>()
            {
                SourceProperty = sourceProperty,
                TargetProperty = targetProperty,
                Format = format
            });
            return this;
        }

        /// <summary>
        /// Map one property name to identically property name
        /// </summary>
        /// <param name="property"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public Mapping<T_Source, T_Target> map(string property, IFormatProvider format = null)
        {
            _rules.Add(new MappingRule<T_Source, T_Target>()
            {
                SourceProperty = property,
                TargetProperty = property,
                Format = format
            });
            return this;
        }

        /// <summary>
        /// Map getter function to property name
        /// </summary>
        /// <param name="getter"></param>
        /// <param name="targetProperty"></param>
        /// <param name="format"></param>
        /// <returns></returns>
        public Mapping<T_Source, T_Target> map(Func<T_Source, T_Target, object> getter, string targetProperty, IFormatProvider format = null)
        {
            _rules.Add(new MappingRule<T_Source, T_Target>()
            {
                Getter = getter,
                TargetProperty = targetProperty,
                Format = format
            });
            return this;
        }

        /// <summary>
        /// Map source property name to setter
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        public Mapping<T_Source, T_Target> map(string sourceProperty, Action<T_Source, T_Target, object> setter)
        {
            _rules.Add(new MappingRule<T_Source, T_Target>()
            {
                SourceProperty = sourceProperty,
                Setter = setter
            });
            return this;
        }

        /// <summary>
        /// Map source property name to setter
        /// </summary>
        /// <param name="sourceProperty"></param>
        /// <param name="setter"></param>
        /// <returns></returns>
        public Mapping<T_Source, T_Target> map(Func<T_Source, T_Target, object> getter, Action<T_Source, T_Target, object> setter)
        {
            _rules.Add(new MappingRule<T_Source, T_Target>()
            {
                Getter = getter,
                Setter = setter
            });
            return this;
        }
    }
}
