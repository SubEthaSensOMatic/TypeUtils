namespace TypeUtils.Services
{
    public interface IPropertyMapper
    {
        void map(object source, object target);
    }

    public interface IPropertyMapper<T_Source, T_Target>: IPropertyMapper
    {
        void map(T_Source source, T_Target target);
    }
}
