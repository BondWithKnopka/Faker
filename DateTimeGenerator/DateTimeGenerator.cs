using System;
using Faker.Generator;

namespace DateTimeGenerator
{
    public class DateTimeGenerator : Generator<DateTime>
    {
        protected override DateTime ObjectGeneration(Random random)
        {
            int year = random.Next(1, 2030);
            int month = random.Next(1, 13);
            int day = random.Next(1, 29);
            int hour = random.Next(0, 24);
            int minute = random.Next(0, 60);
            int second = random.Next(0, 60);

            DateTime result = new DateTime(year, month, day, hour, minute, second);
            return result;
        }
    }
}