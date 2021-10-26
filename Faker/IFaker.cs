using System;
using Faker.Generator;

namespace Faker
{
    public interface IFaker
    {
        object Create(Type type);
    }
}