using System;
using Faker.Generator;

namespace StringGenerator
{
    public class StringGenerator : Generator<string>
    {
        protected override string ObjectGeneration(Random random)
        {
            char[] chars = new Char[random.Next(10, 20)];
            for (var i = 0; i < chars.Length; i++)
            {
                chars[i] = (char) random.Next(90, 255);
            }

            return new string(chars);
        }
    }
}