using System;
using System.Collections.Generic;

namespace TypeUtils.Services
{
    public sealed class MappingRule
    {
        public string SourceProperty { get; private set; }

        public string TargetProperty { get; private set; }

        public IFormatProvider Format { get; private set; }

        public MappingRule(string sourceProperty, string targetProperty, IFormatProvider format = null)
        {
            SourceProperty = sourceProperty;
            TargetProperty = targetProperty;
            Format = format;
        }
    }

    public class Mapping<T_Source, T_Target>
    {
        public Type SourceType { get { return typeof(T_Source); } }

        public Type TargetType { get { return typeof(T_Target); } }

        public int Count => _rules.Count;

        public MappingRule this[int idx] => _rules[idx];

        private List<MappingRule> _rules = new List<MappingRule>();

        public Mapping<T_Source, T_Target> map(string sourceProperty, string targetProperty, IFormatProvider format = null)
        {
            _rules.Add(new MappingRule(sourceProperty, targetProperty, format));
            return this;
        }

        public Mapping<T_Source, T_Target> map(string property, IFormatProvider format = null)
        {
            _rules.Add(new MappingRule(property, property, format));
            return this;
        }
    }
}
