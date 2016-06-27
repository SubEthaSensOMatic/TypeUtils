using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TypeUtils.Services.Impl
{
    public class PropertyMapperFactory: IPropertyMapperFactory
    {
        /// <summary>
        /// Create mapper for mapping definition. Mapper should be cached. Creating a mapper
        /// is an expensive operation!
        /// </summary>
        /// <typeparam name="T_Source">Source type</typeparam>
        /// <typeparam name="T_Target">target type</typeparam>
        /// <param name="mapping">Mapping definition</param>
        /// <returns>Interface to perperty mapper</returns>
        public IPropertyMapper<T_Source, T_Target> createPropertyMapper<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var type = createPropertyMapperType<T_Source, T_Target>(mapping);

            return (IPropertyMapper<T_Source, T_Target>)Activator
                .CreateInstance(type, new object[] { mapping });
        }

        /// <summary>
        /// Create dynamic compiled mapper type
        /// </summary>
        /// <typeparam name="T_Source"></typeparam>
        /// <typeparam name="T_Target"></typeparam>
        /// <param name="mapping"></param>
        /// <returns></returns>
        private Type createPropertyMapperType<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var typeName = "PropertyMapper_" + Guid.NewGuid().ToString("N");

            var mapper = createMapperType(typeName, mapping);

            if (typeof(T_Source) != typeof(object) || typeof(T_Target) != typeof(object))
                mapper.Members.Add(createMapMethod(mapping));

            mapper.Members.Add(createMapMethodTypeSafe(mapping));

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
                GenerateInMemory = true, //false,
                IncludeDebugInformation = false,
                //TempFiles = new TempFileCollection(Environment.GetEnvironmentVariable("TEMP"), true)
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

        private CodeTypeDeclaration createMapperType<T_Source, T_Target>(string typeName, Mapping<T_Source, T_Target> mapping)
        {
            var mapper = new CodeTypeDeclaration(typeName);
            mapper.Attributes = MemberAttributes.Public;
            mapper.IsClass = true;
            mapper.BaseTypes.Add(new CodeTypeReference(typeof(IPropertyMapper<T_Source, T_Target>)));

            mapper.Members.Add(new CodeMemberField(typeof(Mapping<T_Source, T_Target>), "_mapping")
            {
                Attributes = MemberAttributes.Private
            });

            mapper.Members.Add(new CodeMemberField(typeof(ITypeConverter), "_converter")
            {
                Attributes = MemberAttributes.Private
            });

            var constructor = new CodeConstructor();
            constructor.Attributes = MemberAttributes.Public | MemberAttributes.Final;
            constructor.Parameters.Add(new CodeParameterDeclarationExpression(
                typeof(Mapping<T_Source, T_Target>), "mapping"));

            constructor.Statements.Add(new CodeSnippetStatement("_mapping = mapping;"));

            constructor.Statements.Add(new CodeSnippetStatement(string.Format(
                "_converter = mapping.Converter ?? {0}.Current;", typeof(TypeConverter).FullName)));

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

            var callMap = new CodeMethodInvokeExpression()
            {
                Method = new CodeMethodReferenceExpression(new CodeThisReferenceExpression(), "map"),
            };
            callMap.Parameters.Add(new CodeCastExpression()
            {
                TargetType = new CodeTypeReference(typeof(T_Source)),
                Expression = new CodeArgumentReferenceExpression("source")
            });
            callMap.Parameters.Add(new CodeCastExpression()
            {
                TargetType = new CodeTypeReference(typeof(T_Target)),
                Expression = new CodeArgumentReferenceExpression("target")
            });

            mapMethod.Statements.Add(callMap);
            
            return mapMethod;
        }

        private CodeMemberMethod createMapMethodTypeSafe<T_Source, T_Target>(Mapping<T_Source, T_Target> mapping)
        {
            var sourceType = typeof(T_Source);
            var targetType = typeof(T_Target);

            var mapMethod = new CodeMemberMethod()
            {
                Name = "map",
                Attributes = MemberAttributes.Public
            };

            mapMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T_Source), "source"));
            mapMethod.Parameters.Add(new CodeParameterDeclarationExpression(typeof(T_Target), "target"));

            for (int ruleIdx = 0; ruleIdx < mapping.Count; ruleIdx++)
            {
                var rule = mapping[ruleIdx];

                CodeStatement stmt = null;

                if (!string.IsNullOrWhiteSpace(rule.SourceProperty) && !string.IsNullOrWhiteSpace(rule.TargetProperty))
                {
                    // Property name to property name
                    stmt = createPropertyToPropertyStatement(ruleIdx, rule);
                }
                else if (!string.IsNullOrWhiteSpace(rule.SourceProperty) && rule.Setter != null)
                {
                    // Property name to setter
                    stmt = createPropertyToSetterStatement(ruleIdx, rule);
                }
                else if (rule.Getter!= null && !string.IsNullOrWhiteSpace(rule.TargetProperty))
                {
                    // Getter to property name
                    stmt = createGetterToPropertyStatement(ruleIdx, rule);
                }
                else if (rule.Getter != null && rule.Setter != null)
                {
                    // Getter to setter
                    stmt = createGetterToSetterStatement(ruleIdx, rule);
                }

                if (stmt != null)
                    mapMethod.Statements.Add(stmt);
                
            }

            return mapMethod;
        }

        private CodeStatement createPropertyToPropertyStatement<T_Source, T_Target>(int ruleIdx, MappingRule<T_Source, T_Target> rule)
        {
            var sourceProperty = typeof(T_Source)
                .GetProperty(rule.SourceProperty);

            if (sourceProperty == null || sourceProperty.GetGetMethod(false) == null)
                return null;

            var targetProperty = typeof(T_Target)
                .GetProperty(rule.TargetProperty);

            if (targetProperty == null || targetProperty.GetSetMethod(false) == null)
                return null;

            CodeStatement result = null;

            if (targetProperty.PropertyType.IsAssignableFrom(sourceProperty.PropertyType))
            {
                // No conversion needed
                result = new CodeSnippetStatement(string.Format(
                    @"target.{0} = source.{1};",
                    targetProperty.Name,
                    sourceProperty.Name));
            }
            else
            {
                // Conversion needed
                result = new CodeSnippetStatement(string.Format(
                    @"target.{0} = _converter.convert<{1}>(source.{2}, _mapping[{3}].Format);",
                    targetProperty.Name,
                    targetProperty.PropertyType.FullName,
                    sourceProperty.Name,
                    ruleIdx));
            }

            return result;
        }

        private CodeStatement createPropertyToSetterStatement<T_Source, T_Target>(int ruleIdx, MappingRule<T_Source, T_Target> rule)
        {
            var sourceProperty = typeof(T_Source)
                .GetProperty(rule.SourceProperty);

            if (sourceProperty == null || sourceProperty.GetGetMethod(false) == null)
                return null;

            // No conversion needed
            return new CodeSnippetStatement(string.Format(
                @"_mapping[{0}].Setter(source, target, source.{1});",
                ruleIdx,
                sourceProperty.Name));
        }

        private CodeStatement createGetterToPropertyStatement<T_Source, T_Target>(int ruleIdx, MappingRule<T_Source, T_Target> rule)
        {
            var targetProperty = typeof(T_Target)
                .GetProperty(rule.TargetProperty);

            if (targetProperty == null || targetProperty.GetSetMethod(false) == null)
                return null;

            // No conversion needed
            return new CodeSnippetStatement(string.Format(
                @"target.{0} = _converter.convert<{1}>(_mapping[{2}].Getter(source, target));",
                targetProperty.Name,
                targetProperty.PropertyType.FullName,
                ruleIdx));
        }

        private CodeStatement createGetterToSetterStatement<T_Source, T_Target>(int ruleIdx, MappingRule<T_Source, T_Target> rule)
        {
            // No conversion needed
            return new CodeSnippetStatement(string.Format(
                @"_mapping[{0}].Setter(source, target, _mapping[{1}].Getter(source, target));",
                ruleIdx, ruleIdx));
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
