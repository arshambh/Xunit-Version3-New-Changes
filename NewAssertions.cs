using System.Collections.Immutable;

// Samples for new assertion library features in xUnit v3:
// https://xunit.net/docs/getting-started/v3/whats-new#new-assertions-for-everybody
// https://xunit.net/docs/getting-started/v3/whats-new#support-for-immutable-collections
// https://xunit.net/docs/getting-started/v3/whats-new#support-for-partial-collections
// https://xunit.net/docs/getting-started/v3/whats-new#dynamically-skippable-tests
// https://xunit.net/docs/getting-started/v3/whats-new#explicit-tests

namespace xunit3;

public class NewAssertionsTests
{
    // ---------------------------------------------------------------------
    // New assertions for everybody: dynamically skipping a test at runtime.
    // ---------------------------------------------------------------------

    [Fact]
    public void AssertSkip_AlwaysSkipsTheTest()
    {
        // Assert.Skip immediately marks the test as skipped.
        Assert.Skip("This test is not implemented yet.");
    }

    [Fact]
    public void AssertSkipUnless_SkipsWhenConditionIsFalse()
    {
        var isWindows = OperatingSystem.IsWindows();

        // Skips the test unless the condition is true.
        Assert.SkipUnless(isWindows, "This test only runs on Windows.");

        Assert.True(isWindows);
    }

    [Fact]
    public void AssertSkipWhen_SkipsWhenConditionIsTrue()
    {
        var isDebug = false;

        // Skips the test when the condition is true.
        Assert.SkipWhen(isDebug, "This test does not run in debug mode.");

        Assert.False(isDebug);
    }

    // ---------------------------------------------------------------------
    // Support for immutable collections: new Assert.Contains /
    // Assert.DoesNotContain overloads for ImmutableDictionary<TKey, TValue>,
    // ImmutableHashSet<T>, and ImmutableSortedSet<T>.
    // ---------------------------------------------------------------------

    [Fact]
    public void ImmutableDictionary_ContainsAndDoesNotContain()
    {
        var dictionary = new Dictionary<string, int>
        {
            ["one"] = 1,
            ["two"] = 2,
        }.ToImmutableDictionary();

        Assert.Contains("one", dictionary);
        Assert.DoesNotContain("three", dictionary);
    }

    [Fact]
    public void ImmutableHashSet_ContainsAndDoesNotContain()
    {
        var hashSet = new[] { 1, 2, 3 }.ToImmutableHashSet();

        Assert.Contains(2, hashSet);
        Assert.DoesNotContain(42, hashSet);
    }

    [Fact]
    public void ImmutableSortedSet_ContainsAndDoesNotContain()
    {
        var sortedSet = new[] { "apple", "banana" }.ToImmutableSortedSet();

        Assert.Contains("apple", sortedSet);
        Assert.DoesNotContain("cherry", sortedSet);
    }

    // ---------------------------------------------------------------------
    // Support for partial collections: new overloads for Span<T>,
    // ReadOnlySpan<T>, Memory<T>, and ReadOnlyMemory<T> (Contains,
    // DoesNotContain, Equal), plus string-like overloads for char-based
    // spans (StartsWith, EndsWith, Equal, Contains, DoesNotContain).
    // ---------------------------------------------------------------------

    [Fact]
    public void ReadOnlySpan_Equal()
    {
        Span<int> span = [1, 2, 3];
        Span<int> other = [1, 2, 3];

        // Equal works for spans of any element type.
        Assert.Equal(other.ToArray(), span.ToArray());
    }

    [Fact]
    public void Memory_ContainsAndDoesNotContain()
    {
        // Char-based spans are treated like strings for Contains/DoesNotContain.
        ReadOnlySpan<char> span = "Hello, World!".AsSpan();

        Assert.Contains("World", span);
        Assert.DoesNotContain("goodbye", span);

        // Memory works with Equal.
        ReadOnlyMemory<char> memory = "Hello, World!".AsMemory();
        Assert.Equal("Hello, World!".AsSpan(), memory.Span);
    }

    [Fact]
    public void ReadOnlyMemory_Equal()
    {
        ReadOnlyMemory<int> actual = new[] { 1, 2, 3 };
        ReadOnlyMemory<int> expected = new[] { 1, 2, 3 };

        Assert.Equal(expected.ToArray(), actual.ToArray());
    }

    [Fact]
    public void CharSpans_AreTreatedLikeStrings()
    {
        ReadOnlySpan<char> value = "Hello, World!".AsSpan();
        ReadOnlySpan<char> memoryValue = "Hello, World!".AsMemory().Span;

        Assert.StartsWith("Hello", value);
        Assert.EndsWith("World!", value);
        Assert.Contains("lo, Wo", value);
        Assert.DoesNotContain("goodbye", value);
        Assert.Equal("Hello, World!", value);
        Assert.Equal("Hello, World!", memoryValue);
    }

    // ---------------------------------------------------------------------
    // Updated assertions: Assert.Equivalent now supports comparing two
    // Uri values by comparing their OriginalString values.
    // ---------------------------------------------------------------------

    [Fact]
    public void AssertEquivalent_SupportsUriComparison()
    {
        var uri1 = new Uri("https://xunit.net/docs/getting-started/v3/whats-new");
        var uri2 = new Uri("https://xunit.net/docs/getting-started/v3/whats-new");

        Assert.Equivalent(uri1, uri2);
    }

    // ---------------------------------------------------------------------
    // Dynamically skippable tests: [Fact(Skip = "...")] skips the test at
    // discovery time (before it runs), complementing the runtime Assert.Skip
    // family shown above.
    // https://xunit.net/docs/getting-started/v3/whats-new#dynamically-skippable-tests
    // ---------------------------------------------------------------------

    [Fact(Skip = "Skipped at discovery time until the feature is implemented.")]
    public void SkippedViaAttribute_Demo()
    {
        // This body never executes while the Skip reason is present.
        Assert.True(true);
    }

    // ---------------------------------------------------------------------
    // Explicit tests: [Fact(Explicit = true)] means the test only runs when
    // explicitly requested (e.g. filtering by name in the runner); it shows
    // as "not run" during normal test executions.
    // https://xunit.net/docs/getting-started/v3/whats-new#explicit-tests
    // ---------------------------------------------------------------------

    [Fact(Explicit = true)]
    public void ExplicitTest_Demo()
    {
        Assert.True(true);
    }
}
