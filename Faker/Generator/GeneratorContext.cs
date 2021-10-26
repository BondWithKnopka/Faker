using System;

namespace Faker.Generator
{
    public class GeneratorContext
    {
        public Random Random { get; }

        public Type TargetType { get; }

        public IFaker Faker { get; }

        public GeneratorContext(Random random, Type targetType, IFaker faker)
        {
            Random = random;
            TargetType = targetType;
            Faker = faker;
        }
    }
}