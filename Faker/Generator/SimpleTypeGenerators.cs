using System;

namespace Faker.Generator
{
    public class IntGenerator : Generator<int>
    {
        protected override int ObjectGeneration(Random random)
        {
            return random.Next();
        }
    }

    public class FloatGenerator : Generator<float>
    {
        protected override float ObjectGeneration(Random random)
        {
            return (float) (random.NextDouble() * random.Next());
        }
    }

    public class LongGenerator : Generator<long>
    {
        protected override long ObjectGeneration(Random random)
        {
            return ((long) random.Next() << 32) + random.Next();
        }
    }

    public class DoubleGenerator : Generator<double>
    {
        protected override double ObjectGeneration(Random random)
        {
            return random.NextDouble() * random.Next();
        }
    }

    public class CharGenerator : Generator<char>
    {
        protected override char ObjectGeneration(Random random)
        {
            return (char) random.Next(0, 255);
        }
    }

    public class BoolGenerator : Generator<bool>
    {
        protected override bool ObjectGeneration(Random random)
        {
            return Convert.ToBoolean(random.Next(0, 2));
        }
    }
}