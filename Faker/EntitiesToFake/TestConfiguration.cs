namespace Faker.EntitiesToFake
{
    class TestConfiguration
    {
        public string ConfigString;
        public int ConfigInt;
        public string String;
        public int Int;
        private int PropInt { get; }
        public int PropIntConfig { get; }

        public TestConfiguration(int PropInt, int PropIntConfig)
        {
            this.PropInt = PropInt;
            this.PropIntConfig = PropIntConfig;
        }
    }
}