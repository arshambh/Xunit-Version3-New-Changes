using Xunit.Sdk;

namespace xunit3;


public class PersonMessageJson : IJsonSerializable, IJsonDeserializable
{
    // Required for deserialization (Activator.CreateInstance)
    public PersonMessageJson() { }

    public PersonMessageJson(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public string Name { get; set; } = "";
    public int Age { get; set; }

    // IJsonSerializable
    public string? ToJson()
    {
        // Minimal JSON — only for illustration.
        // Real message types usually inherit MessageSinkMessage and override Serialize(JsonObjectSerializer).
        return $$"""{"Name":"{{Name}}","Age":{{Age}}}""";
    }

    // IJsonDeserializable
    public void FromJson(IReadOnlyDictionary<string, object?> root)
    {
        if (root.TryGetValue("Name", out var nameObj) && nameObj is string name)
            Name = name;

        if (root.TryGetValue("Age", out var ageObj) && ageObj is int age)
            Age = age;
        else if (ageObj is long ageLong) // JSON numbers often come back as long
            Age = (int)ageLong;
    }
}

public class PersonMessageJsonTests
{
    [Fact]
    public void ToJson_ReturnsExpectedJson()
    {
        var message = new PersonMessageJson("Alice", 30);

        var json = message.ToJson();

        Assert.Equal("""{"Name":"Alice","Age":30}""", json);
    }

    [Fact]
    public void FromJson_PopulatesProperties()
    {
        var message = new PersonMessageJson();
        var root = new Dictionary<string, object?>
        {
            ["Name"] = "Bob",
            ["Age"] = 25
        };

        message.FromJson(root);

        Assert.Equal("Bob", message.Name);
        Assert.Equal(25, message.Age);
    }

    [Fact]
    public void RoundTrip_PreservesValues()
    {
        var original = new PersonMessageJson("Carol", 42);

        // Simulate serialize → deserialize via the dictionary shape xUnit uses
        var root = new Dictionary<string, object?>
        {
            ["Name"] = original.Name,
            ["Age"] = original.Age
        };

        var restored = new PersonMessageJson();
        restored.FromJson(root);

        Assert.Equal(original.Name, restored.Name);
        Assert.Equal(original.Age, restored.Age);
    }

    // Theory — same pattern as the PersonSerializer tests
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
        var original = new PersonMessageJson(name, age);

        var root = new Dictionary<string, object?>
        {
            ["Name"] = original.Name,
            ["Age"] = original.Age
        };

        var restored = new PersonMessageJson();
        restored.FromJson(root);

        Assert.Equal(name, restored.Name);
        Assert.Equal(age, restored.Age);
    }
}