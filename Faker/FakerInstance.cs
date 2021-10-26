using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Faker.Generator;

namespace Faker
{
    public class FakerInstance : IFaker
    {
        private readonly Dictionary<Type, IGenerator> _generators;
        private readonly Stack<Type> _circleDepend = new Stack<Type>();
        private readonly FakerConfiguration _configuration;

        public FakerInstance(FakerConfiguration configuration)
        {
            _generators = new Dictionary<Type, IGenerator>();

            foreach (Type type in Assembly.GetExecutingAssembly().GetTypes())
            {
                if (IsRequiredType(type, typeof(Generator<>)) && IsValidType(type))
                {
                    _generators.Add(
                        type.BaseType.GetGenericArguments()[0], 
                        (IGenerator)Activator.CreateInstance(type)
                    );
                }
            }

            _generators.Add(typeof(List<>), new ListGenerator());
            ScanPlugins(AppDomain.CurrentDomain.BaseDirectory + "\\Plugins\\");
            _configuration = configuration;
        }

        private bool IsRequiredType(Type GeneratorType, Type RequiredType)
        {
            Type localType = GeneratorType;
            while (localType != null && localType != typeof(object))
            {
                var buf = localType.IsGenericType ? localType.GetGenericTypeDefinition() : localType;
                if (RequiredType == buf)
                {
                    return true;
                }

                localType = localType.BaseType;
            }

            return false;
        }

        private bool IsValidType(Type type)
        {
            return type.BaseType != null &&
                type.BaseType.GetGenericArguments().Any() &&
                type.Namespace == "Faker.Generator";
        }

        public T Create<T>()
        {
            return (T) Create(typeof(T));
        }

        private void ScanPlugins(string directory)
        {
            foreach (var file in Directory.EnumerateFiles(directory, "*.dll", SearchOption.AllDirectories))
            {
                var assembly = Assembly.LoadFile(file);

                foreach (Type type in assembly.GetTypes())
                {
                    if (IsRequiredType(type, typeof(Generator<>)))
                    {
                        if (type.BaseType != null && type.BaseType.GetGenericArguments().Any())
                        {
                            _generators.Add(type.BaseType.GetGenericArguments()[0],
                                (IGenerator) Activator.CreateInstance(type));
                        }
                    }
                }
            }
        }

        public object Create(Type type)
        {
            if (_circleDepend.Count(CircleType => CircleType == type) >= 5)
            {
                Console.WriteLine("Circular Dependency");
                return GetDefaultValue(type);
            }
            _circleDepend.Push(type);

            var fakerInstance = new FakerInstance(_configuration);
            int seed = (int) DateTime.Now.Ticks & 0x0000FFFF;
            GeneratorContext context = new GeneratorContext(new Random(seed), type, fakerInstance);

            IGenerator generator = FindGenerator(type);

            if (generator != null)
            {
                _circleDepend.Pop();
                return generator.Generate(context);
            }

            var obj = CreateObject(type);

            obj = FillObject(obj);
            _circleDepend.Pop();
            return obj;
        }

        private object FillObject(object obj)
        {
            if (obj != null)
            {
                FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
                PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);

                FillFields(obj, fields);
                FillProperties(obj, properties);
            }

            return obj;
        }

        private void FillFields(object obj, FieldInfo[] fields)
        {
            foreach (FieldInfo field in fields)
            {
                if (IsNotValueSet(field, obj))
                {
                    ConfigurationRule configurationRule = GetFieldConfigurationRule(field);

                    if (configurationRule == null)
                    {
                        field.SetValue(obj, Create(field.FieldType));
                    }
                    else
                    {
                        int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                        var fakerInstance = new FakerInstance(_configuration);
                        GeneratorContext context = 
                            new GeneratorContext(new Random(seed), field.FieldType, fakerInstance);
                        
                        field.SetValue(
                            obj,
                            ((IGenerator)Activator.CreateInstance(configurationRule.GeneratorName)).Generate(context)
                        );
                    }
                }
            }
        }

        private void FillProperties(object obj, PropertyInfo[] properties)
        {
            foreach (PropertyInfo property in properties)
            {
                if (property.CanWrite && IsNotValueSet(property, obj))
                {
                    ConfigurationRule configurationRule = GetPropertyConfigurationRule(property);

                    if (configurationRule == null)
                    {
                        property.SetValue(obj, Create(property.PropertyType));
                    }
                    else
                    {
                        int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                        var fakerInstance = new FakerInstance(_configuration);
                        GeneratorContext context = 
                            new GeneratorContext(new Random(seed), property.PropertyType,fakerInstance);
                        
                        property.SetValue(
                            obj,((IGenerator)Activator.CreateInstance(configurationRule.GeneratorName)).Generate(context)
                        );
                    }
                }
            }
        }

        private ConfigurationRule GetPropertyConfigurationRule(PropertyInfo property)
        {
            if (_configuration != null)
            {
                foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                {
                    if (rule.FieldName == property.Name && rule.FieldType == property.PropertyType)
                    {
                        return rule;
                    }
                }
            }

            return null;
        }

        private ConfigurationRule GetFieldConfigurationRule(FieldInfo field)
        {
            if (_configuration != null)
            {
                foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                {
                    if (rule.FieldName == field.Name && rule.FieldType == field.FieldType)
                    {
                        return rule;
                    }
                }
            }

            return null;
        }

        private bool IsNotValueSet(MemberInfo member, object obj)
        {
            if (member is FieldInfo field)
            {
                if (GetDefaultValue(field.FieldType) == null) return true;
                if (field.GetValue(obj).Equals(GetDefaultValue(field.FieldType))) return true;
            }

            if (member is PropertyInfo property)
            {
                if (GetDefaultValue(property.PropertyType) == null) return true;
                if (property.GetValue(obj).Equals(GetDefaultValue(property.PropertyType))) return true;
            }

            return false;
        }

        private object GetDefaultValue(Type t)
        {
            return t.IsValueType ? Activator.CreateInstance(t) : null;
        }

        private object CreateObject(Type type)
        {
            ConstructorInfo[] bufConstructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public);

            IEnumerable<ConstructorInfo> constructors =
                bufConstructors.OrderByDescending(Constructor => Constructor.GetParameters().Length);

            object obj = null;

            foreach (ConstructorInfo constructor in constructors)
            {
                ParameterInfo[] parametersInfo = constructor.GetParameters();
                object[] parameters = new object[parametersInfo.Length];

                FillParameters(type, parametersInfo, parameters);

                try
                {
                    obj = constructor.Invoke(parameters);
                    break;
                }
                catch
                {
                    // ignored
                }
            }

            if (obj == null && type.IsValueType)
            {
                obj = Activator.CreateInstance(type);
            }

            return obj;
        }

        private void FillParameters(Type type, ParameterInfo[] parametersInfo, object[] parameters)
        {
            for (int i = 0; i < parameters.Length; i++)
            {
                ConfigurationRule configurationRule = null;
                if (_configuration != null)
                {
                    foreach (ConfigurationRule rule in _configuration.ConfigurationRules)
                    {
                        if (rule.FieldName == parametersInfo[i].Name &&
                            rule.FieldType == parametersInfo[i].ParameterType)
                        {
                            configurationRule = rule;
                        }
                    }
                }

                if (configurationRule == null)
                {
                    parameters[i] = Create(parametersInfo[i].ParameterType);
                }
                else
                {
                    int seed = (int)DateTime.Now.Ticks & 0x0000FFFF;
                    var fakerInstance = new FakerInstance(_configuration);
                    GeneratorContext context = new GeneratorContext(new Random(seed), type, fakerInstance);
                    parameters[i] = ((IGenerator)Activator.CreateInstance(configurationRule.GeneratorName))
                        .Generate(context);
                }
            }
        }

        private IGenerator FindGenerator(Type type)
        {
            if (type.IsGenericType)
            {
                type = type.GetGenericTypeDefinition();
            }

            return _generators.ContainsKey(type) ? _generators[type] : null;
        }
    }
}