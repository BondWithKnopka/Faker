namespace Faker.Generator
{
    public interface IGenerator
    {
        object Generate(GeneratorContext context);
    }
}