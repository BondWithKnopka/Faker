using System;

namespace Faker.EntitiesToFake
{
    class TestConstructors
    {
        public TestConstructors()
        {
        }

        public TestConstructors(int i)
        {
            Console.WriteLine("Error");
        }

        public TestConstructors(int i, int j)
        {
            Console.WriteLine("This is Ok");
        }
    }
}