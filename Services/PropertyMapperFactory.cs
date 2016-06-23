using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeUtils.Services
{
    public class PropertyMapperFactory
    {

        public IPropertyMapper createPropertyMapper<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var type = createPropertyMapperType<T_Source, T_Target>(mapping);

            return (IPropertyMapper)Activator
                .CreateInstance(type, new object[] { mapping });
        }

        private Type createPropertyMapperType<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var typeName = "PropertyMapper_" + Guid.NewGuid().ToString("N");

            var mapper = createMapperType<T_Source, T_Target>(typeName);
            mapper.Members.Add(createMapMethod(mapping));

            var libs = new HashSet<string>();
            addLibs4Type(GetType(), libs);
            addLibs4Type(typeof(T_Source), libs);
            addLibs4Type(typeof(T_Target), libs);

            return compileMapper(mapper, typeName, libs.ToArray());
        }

        private Type compileMapper(CodeTypeDeclaration mapper, string mapperTypeName, string[] libs)
        {
            var codeNamespace = new CodeNamespace("TypeUtils.Services");
            codeNamespace.Types.Add(mapper);

            var compileUnit = new CodeCompileUnit();
            compileUnit.Namespaces.Add(codeNamespace);

            var compilerParameter = new CompilerParameters(libs.ToArray())
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                IncludeDebugInformation = false
            };

            var results = (new CSharpCodeProvider())
                .CompileAssemblyFromDom(compilerParameter, compileUnit);

            if (results.Errors.HasErrors)
            {
                var sb = new StringBuilder();

                foreach (CompilerError error in results.Errors)
                    sb.AppendLine(String.Format("Error ({0}): {1}", error.ErrorNumber, error.ErrorText));

                throw new InvalidOperationException(sb.ToString());
            }

            return results.CompiledAssembly.GetType("TypeUtils.Services." + mapperTypeName);
        }

        private CodeTypeDeclaration createMapperType<T_Source, T_Target>(string typeName)
        {
            var mapper = new CodeTypeDeclaration(typeName);
            mapper.Attributes = MemberAttributes.Public;
            mapper.IsClass = true;
            mapper.BaseTypes.Add(new CodeTypeReference(typeof(IPropertyMapper)));

            mapper.Members.Add(new CodeMemberField(typeof(Mapping<T_Source, T_Target>), "_mapping")
            {
                Attributes = MemberAttributes.Private
            });

            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(Mapping<T_Source, T_Target>), "mapping"));
            constructor.Statements.Add(new CodeSnippetStatement("_mapping = mapping;"));

            mapper.Members.Add(constructor);

            return mapper;
        }

        private CodeMemberMethod createMapMethod<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var sourceType = typeof(T_Source);
            var targetType = typeof(T_Target);

            var mapMethod = new CodeMemberMethod()
            {
                Name = "map",
                Attributes = MemberAttributes.Public
            };

            mapMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "source"));
            mapMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(object), "target"));

            var snippedStatement = new CodeSnippetStatement(string.Format(
                "var target_ = ({0})target;", targetType.FullName));
            mapMethod.Statements.Add(snippedStatement);

            snippedStatement = new CodeSnippetStatement(string.Format(
                "var source_ = ({0})source;", sourceType.FullName));
            mapMethod.Statements.Add(snippedStatement);

            for (int ruleIdx = 0; ruleIdx < mapping.Count; ruleIdx++)
            {
                var sourceProperty = sourceType
                    .GetProperty(mapping[ruleIdx].SourceProperty);

                if (sourceProperty == null)
                    continue;

                var targetProperty = targetType
                    .GetProperty(mapping[ruleIdx].TargetProperty);

                if (targetProperty == null)
                    continue;

                if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
                {
                    // No conversion needed
                    snippedStatement = new CodeSnippetStatement(string.Format(
                        @"target_.{0} = source_.{1};",
                        targetProperty.Name,
                        sourceProperty.Name));

                    mapMethod.Statements.Add(snippedStatement);
                }
                else
                {
                    // Conversion needed
                    snippedStatement = new CodeSnippetStatement(string.Format(
                        @"target_.{0} = {1}.Current.convert<{3}>(source_.{2}, _mapping[{4}].Format);",
                        targetProperty.Name,
                        typeof(TypeConverter).FullName,
                        sourceProperty.Name,
                        targetProperty.PropertyType.FullName,
                        ruleIdx));

                    mapMethod.Statements.Add(snippedStatement);
                }
            }

            return mapMethod;
        }

        private void addLibs4Type(Type t, HashSet<string> libs)
        {
            if (t.Assembly != null)
                libs.Add(t.Assembly.Location);

            addLibs4Interfaces(t, libs);

            var genericTypeArguments = t.GenericTypeArguments;
            if (genericTypeArguments != null && genericTypeArguments.Length > 0)
                foreach (var ta in genericTypeArguments)
                    addLibs4Type(ta, libs);

            if (t.BaseType != null)
                addLibs4Type(t.BaseType, libs);

        }

        private void addLibs4Interfaces(Type t, HashSet<string> libs)
        {
            var ifs = t.GetInterfaces();
            if (ifs != null && ifs.Length > 0)
                foreach (var i in ifs)
                    if (i.Assembly != null)
                        libs.Add(i.Assembly.Location);
        }
    }
}
