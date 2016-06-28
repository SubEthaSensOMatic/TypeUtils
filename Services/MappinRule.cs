using System;

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
}
