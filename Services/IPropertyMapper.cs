namespace TypeUtils.Services
{
    /// <summary>
    /// None generic access to property mapper
    /// </summary>
    public interface IPropertyMapper
    {
        /// <summary>
        /// Mapping between source and target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>Target object</returns>
        object map(object source, object target);
    }

    /// <summary>
    /// Provides access to property mapper 
    /// </summary>
    /// <typeparam name="T_Source"></typeparam>
    /// <typeparam name="T_Target"></typeparam>
    public interface IPropertyMapper<T_Source, T_Target>: IPropertyMapper
    {
        /// <summary>
        /// Create new target object
        /// </summary>
        /// <returns>New target object</returns>
        T_Target createTarget();

        /// <summary>
        /// Mapping between source and target
        /// </summary>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns>Target object</returns>
        T_Target map(T_Source source, T_Target target);
    }
}
