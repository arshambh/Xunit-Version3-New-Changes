using System.Diagnostics.CodeAnalysis;
using Xunit.Sdk;

namespace xunit3;

public record Person(string Name, int Age);

public class PersonSerializer : IXunitSerializer
{
    public PersonSerializer() { }

    public bool IsSerializable(
        Type type,
        object? value,
        [NotNullWhen(false)] out string? failureReason)
    {
        if (type != typeof(Person) || value is not Person)
        {
            failureReason = "Only Person is supported";
            return false;
        }

        failureReason = null;
        return true;
    }

    public string Serialize(object value)
    {
        var person = (Person)value;
        return $"{person.Name}\n{person.Age}";
    }

    public object Deserialize(Type type, string serializedValue)
    {
        var parts = serializedValue.Split('\n');
        return new Person(parts[0], int.Parse(parts[1]));
    }
}

public class PersonSerializerTests
{
    private readonly PersonSerializer _serializer = new();

    [Fact]
    public void IsSerializable_WithPerson_ReturnsTrue()
    {
        var person = new Person("Alice", 30);

        var result = _serializer.IsSerializable(typeof(Person), person, out var failureReason);

        Assert.True(result);
        Assert.Null(failureReason);
    }

    [Fact]
    public void IsSerializable_WithWrongType_ReturnsFalse()
    {
        var result = _serializer.IsSerializable(typeof(string), "hello", out var failureReason);

        Assert.False(result);
        Assert.Equal("Only Person is supported", failureReason);
    }

    [Fact]
    public void Serialize_ReturnsExpectedString()
    {
        var person = new Person("Alice", 30);

        var serialized = _serializer.Serialize(person);

        Assert.Equal("Alice\n30", serialized);
    }

    [Fact]
    public void Deserialize_ReturnsExpectedPerson()
    {
        var serialized = "Bob\n25";

        var result = _serializer.Deserialize(typeof(Person), serialized);

        var person = Assert.IsType<Person>(result);
        Assert.Equal("Bob", person.Name);
        Assert.Equal(25, person.Age);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var original = new Person("Carol", 42);

        var serialized = _serializer.Serialize(original);
        var restored = (Person)_serializer.Deserialize(typeof(Person), serialized);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Age, restored.Age);
    }

    // Theory added here
    public static TheoryData<string, int> People =>
    [
        ("Alice", 30),
        ("Bob", 25),
        ("Carol", 42),
        ("Dave", 18),
    ];

    [Theory]
    [MemberData(nameof(People))]
    public void RoundTrip_WithTheory_PreservesValues(string name, int age)
    {
        var original = new Person(name, age);

        var serialized = _serializer.Serialize(original);
        var restored = (Person)_serializer.Deserialize(typeof(Person), serialized);

        Assert.Equal(name, restored.Name);
        Assert.Equal(age, restored.Age);
    }
}