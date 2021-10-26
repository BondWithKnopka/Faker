using System;
using System.Collections.Generic;
using System.Reflection;
using Faker.EntitiesToFake;
using Faker.Generator;

namespace Test
{
    public class NewIntGenerator : Generator<int>
    {
        protected override int ObjectGeneration(Random random)
        {
            return 322;
        }
    }

    public class NewStringGenerator : Generator<string>
    {
        protected override string ObjectGeneration(Random random)
        {
            string[] songWords =
            {
                "Never gonna give you up", "Never gonna let you down", "Never gonna run around and desert you",
                "Never gonna make you cry", "Never gonna say goodbye", "Never gonna tell a lie and hurt you"
            };
            return songWords[random.Next(0, songWords.Length - 1)];
        }
    }
}

namespace Faker
{
    using Test;

    class Program
    {
        class A
        {
            public int i;
            public B B;
        }

        class B
        {
            public C C;
            public double d;
        }

        class C
        {
            public A A;
        }

        class TestClass
        {
            public int Int;
            public DateTime DateTime;
            public bool Bool;
        }

        class TestClassInField
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

            PrintObjectValue(faker.Create<A>(), " ");
            PrintDelimiter();

            PrintObjectValue(faker.Create<TestClassInField>(), " ");
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
            configuration2.Add<TestConfiguration, string, NewStringGenerator>(Config => Config.ConfigString);
            configuration2.Add<TestConfiguration, int, NewIntGenerator>(Config => Config.ConfigInt);
            configuration2.Add<TestConfiguration, int, NewIntGenerator>(Config => Config.PropIntConfig);

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