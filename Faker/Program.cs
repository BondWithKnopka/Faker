using System;
using System.Collections.Generic;
using System.Reflection;
using Faker.EntitiesToFake;
using Faker.Generator;

namespace Test
{
    public class CustomIntGenerator : Generator<int>
    {
        protected override int ObjectGeneration(Random random)
        {
            return 2021;
        }
    }

    public class CustomStringGenerator : Generator<string>
    {
        protected override string ObjectGeneration(Random random)
        {
            string[] words =
            {
                "Somebody", "Once", "Told", "Me", "The", "World", 
                "Is", "Gonna", "Roll", "Me"
            };
            return words[random.Next(0, words.Length - 1)];
        }
    }
}

namespace Faker
{
    using Test;

    class Program
    {
        class ClassOne
        {
            public int Int;
            public ClassTwo refToTwo;
        }

        class ClassTwo
        {
            public ClassThree refToThree;
            public double Double;
        }

        class ClassThree
        {
            public ClassOne refToOne;
        }

        class TestClass
        {
            public int Int;
            public DateTime DateTime;
            public bool Bool;
        }

        class TestWithClassInField
        {
            public string String;
            public double Double;
            public TestClass TestClass;
        }

        static void Main(string[] args)
        {
            var faker = new FakerInstance(null);

            PrintObjectValue(faker.Create<AllConvertionTypes>(), "");
            PrintDelimiter();

            PrintObjectValue(faker.Create<TestConstructors>(), " ");
            PrintDelimiter();

            PrintObjectValue(faker.Create<ClassOne>(), " ");
            PrintDelimiter();

            PrintObjectValue(faker.Create<TestWithClassInField>(), " ");
            PrintDelimiter();

            foreach (TestClass testClass in faker.Create<List<TestClass>>())
            {
                PrintObjectValue(testClass, " ");
            }
            PrintDelimiter();

            foreach (List<TestClass> listTestClass in faker.Create<List<List<TestClass>>>())
            {
                foreach (TestClass testClass in listTestClass)
                {
                    PrintObjectValue(testClass, " ");
                }
                Console.WriteLine();
            }
            PrintDelimiter();

            FakerConfiguration configuration2 = new FakerConfiguration();
            configuration2.Add<TestConfiguration, string, CustomStringGenerator>(Config => Config.ConfigString);
            configuration2.Add<TestConfiguration, int, CustomIntGenerator>(Config => Config.ConfigInt);
            configuration2.Add<TestConfiguration, int, CustomIntGenerator>(Config => Config.PropIntConfig);

            var faker2 = new FakerInstance(configuration2);
            PrintObjectValue(faker2.Create<TestConfiguration>(), " ");
            
            Console.ReadLine();
        }

        private static void PrintDelimiter()
        {
            Console.WriteLine("....................................................................");
        }
        private static void PrintObjectValue(object obj, string offset)
        {
            if (obj != null)
            {
                Type classType = obj.GetType();
                Console.WriteLine(offset + classType.Name);
                
                FieldInfo[] fieldInfo = classType.GetFields();
                PropertyInfo[] propertyInfo = classType.GetProperties();

                PrintFieldInfo(obj, fieldInfo, offset);
                PrintPropertyInfo(obj, propertyInfo, offset);
            }
        }

        private static void PrintFieldInfo(object obj, FieldInfo[] fieldInfo, string offset)
        {
            foreach (var field in fieldInfo)
            {
                Type type = Type.GetType(field.FieldType.ToString());
                if (type != null && type.IsClass && type.Name != "String")
                {
                    offset += " ";
                    PrintObjectValue(field.GetValue(obj), offset);
                    offset = offset.Remove(offset.Length - 1, 1);
                }
                else
                {
                    Console.WriteLine(offset + "Name: " + field.Name + " Field Type: " + field.FieldType +
                                      " Value: " + field.GetValue(obj));
                }
            }
        }

        private static void PrintPropertyInfo(object obj, PropertyInfo[] propertyInfo, string offset)
        {

            foreach (var property in propertyInfo)
            {
                Type type2 = Type.GetType(property.PropertyType.ToString());
                if (type2 != null && type2.IsClass && type2.Name != "String")
                {
                    offset += " ";
                    PrintObjectValue(property.GetValue(obj), offset);
                    offset = offset.Remove(offset.Length - 1, 1);
                }
                else
                {
                    Console.WriteLine(offset + "Name: " + property.Name + " Field Type: " + property.PropertyType +
                                      " Value: " + property.GetValue(obj));
                }
            }
        }
    }
}