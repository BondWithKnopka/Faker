using System;

namespace Faker.Generator
{
    public abstract class Generator<T> : IGenerator
    {
        protected abstract T ObjectGeneration(Random random);

        public object Generate(GeneratorContext generatorContext)
        {
            return ObjectGeneration(generatorContext.Random);
        }
    }

    public class ListGenerator : IGenerator
    {
        public object Generate(GeneratorContext generatorContext)
        {
            var list = (System.Collections.IList) Activator.CreateInstance(generatorContext.TargetType);

            for (var i = 0; i <= generatorContext.Random.Next(1, 10); i++)
            {
                list.Add(generatorContext.Faker.Create(generatorContext.TargetType.GetGenericArguments()[0]));
            }

            return list;
        }
    }
}