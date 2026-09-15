// ---------------------------------------------------------------------
// Theory data rows and metadata
//
// In v2, theory data providers had to return something compatible with
// IEnumerable<object[]>. In v3 the contract was expanded to allow three
// legal data row representations:
//
//   * object[]
//   * named or unnamed tuples (anything implementing ITuple)
//   * ITheoryDataRow
//
// MemberData methods may also be async (returning Task<> or ValueTask<>),
// so data retrieval can be asynchronous.
//
// ITheoryDataRow lets you decorate an individual row of data with
// metadata: explicit, skipped, custom display name, timeout, and traits.
// TheoryDataRow is the untyped base class and TheoryDataRow<T1..T10> are
// the strongly typed versions, each of which supports both a
// property-setting pattern and a fluent (WithXxx) construction pattern.
// https://xunit.net/docs/getting-started/v3/whats-new#theory-data-rows-and-metadata
// ---------------------------------------------------------------------

namespace xunit3;

public class NewTheoryDataRowTests
{
    // =================================================================
    // The three legal data row representations.
    // =================================================================

    // 1) object[] - the shape that was also legal in v2.
    public static IEnumerable<object[]> ObjectArrayData =>
    [
        [2, 4],
        [3, 9],
    ];

    [Theory]
    [MemberData(nameof(ObjectArrayData))]
    public void DataRow_AsObjectArray(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // 2) Named tuples (anything implementing ITuple).
    public static IEnumerable<(int Value, int Squared)> NamedTupleData =>
    [
        (4, 16),
        (5, 25),
    ];

    [Theory]
    [MemberData(nameof(NamedTupleData))]
    public void DataRow_AsNamedTuple(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // 2) ...and unnamed tuples.
    public static IEnumerable<(int, int)> UnnamedTupleData =>
    [
        (6, 36),
    ];

    [Theory]
    [MemberData(nameof(UnnamedTupleData))]
    public void DataRow_AsUnnamedTuple(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // 3) ITheoryDataRow - TheoryData<...> rows, or TheoryDataRow<...> rows.
    public static TheoryData<int, int> TheoryDataRows => new()
    {
        { 6, 36 },                          // implicitly wrapped in a TheoryDataRow<int, int>
        new TheoryDataRow<int, int>(7, 49), // an explicitly constructed row
    };

    [Theory]
    [MemberData(nameof(TheoryDataRows))]
    public void DataRow_AsTheoryDataRow(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Row metadata: marked as explicit. Explicit rows only run when they
    // are explicitly requested (for example, by asking to run one test in
    // Test Explorer); otherwise the runner reports them as "not run".
    // =================================================================

    public static TheoryData<int, int> ExplicitRows => new()
    {
        { 8, 64 }, // normal row: always runs
        new TheoryDataRow<int, int>(9, 81) { Explicit = true },
    };

    [Theory]
    [MemberData(nameof(ExplicitRows))]
    public void Metadata_Explicit(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Row metadata: marked as skipped. The row is reported as skipped and
    // the test body never runs for it, while the other rows do run.
    // =================================================================

    public static TheoryData<int, int> SkippedRows => new()
    {
        { 10, 100 }, // normal row: always runs
        new TheoryDataRow<int, int>(11, 121) { Skip = "This row isn't ready to run yet." },
    };

    [Theory]
    [MemberData(nameof(SkippedRows))]
    public void Metadata_Skip(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Row metadata: custom display name. The row shows up in test results
    // using this name instead of the default "(value: 12, squared: 144)".
    // =================================================================

    public static TheoryData<int, int> TestDisplayNameRows => new()
    {
        new TheoryDataRow<int, int>(12, 144) { TestDisplayName = "twelve squared is one forty-four" },
    };

    [Theory]
    [MemberData(nameof(TestDisplayNameRows))]
    public void Metadata_TestDisplayName(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Row metadata: timeout, expressed in seconds (null means no timeout).
    // A timeout may be set per row, so slow rows can be given more time
    // than fast ones rather than applying one timeout to the whole theory.
    // =================================================================

    public static TheoryData<int, int> TimeoutRows => new()
    {
        new TheoryDataRow<int, int>(13, 169) { Timeout = 30 },
    };

    [Theory]
    [MemberData(nameof(TimeoutRows))]
    public void Metadata_Timeout(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Row metadata: traits. Traits attached to a row are merged with the
    // traits applied to the test method/class, so a theory can carry
    // per-row categorization.
    // =================================================================

    public static TheoryData<int, int> TraitRows => new()
    {
        new TheoryDataRow<int, int>(14, 196)
        {
            Traits = new Dictionary<string, HashSet<string>>
            {
                ["Category"] = new HashSet<string> { "row-metadata" },
            },
        },
    };

    [Theory]
    [MemberData(nameof(TraitRows))]
    public void Metadata_Traits(int value, int squared)
    {
        // Row traits are merged with the method/class traits when the row is
        // resolved, so they are visible at runtime through the test context
        // (they are also what filtering and reporting tools use).
        var traits = TestContext.Current.Test!.Traits;

        Assert.Contains("Category", traits.Keys);
        Assert.Contains("row-metadata", traits["Category"]);

        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // The two ways to construct a row with metadata: the property-setting
    // pattern and the fluent pattern.
    // =================================================================

    public static IEnumerable<TheoryDataRow<int, int>> PropertyAndFluentRows =>
    [
        // Property-setting pattern: set the metadata through an object
        // initializer on the type-argument generic TheoryDataRow<...>.
        new TheoryDataRow<int, int>(15, 225)
        {
            TestDisplayName = "property-setting pattern",
            Timeout = 30,
            Traits = new Dictionary<string, HashSet<string>>
            {
                ["Category"] = new HashSet<string> { "property-pattern" },
            },
        },

        // Fluent pattern: chain the WithXxx methods. WithSkip(null) and
        // WithExplicit(null) mean "do not skip" / "use the default flag".
        new TheoryDataRow<int, int>(16, 256)
            .WithTestDisplayName("fluent pattern")
            .WithTimeout(30)
            .WithTrait("Category", "fluent-pattern")
            .WithExplicit(false)
            .WithSkip(null),
    ];

    [Theory]
    [MemberData(nameof(PropertyAndFluentRows))]
    public void DataRow_PropertySettingAndFluentPatterns(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    // =================================================================
    // Async MemberData: data methods may return Task<> or ValueTask<> of
    // any of the legal row shapes, so data can be retrieved asynchronously.
    // =================================================================

    public static async Task<IEnumerable<object[]>> AsyncObjectArrayDataAsync()
    {
        await Task.Yield();

        return new[]
        {
            new object[] { 17, 289 },
            new object[] { 18, 324 },
        };
    }

    [Theory]
    [MemberData(nameof(AsyncObjectArrayDataAsync))]
    public void AsyncMemberData_TaskOfObjectArrays(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }

    public static async ValueTask<IEnumerable<TheoryDataRow<int, int>>> AsyncTheoryDataRowsAsync()
    {
        await Task.Yield();

        return
        [
            new TheoryDataRow<int, int>(19, 361) { TestDisplayName = "async row 1" },
            new TheoryDataRow<int, int>(20, 400) { TestDisplayName = "async row 2" },
        ];
    }

    [Theory]
    [MemberData(nameof(AsyncTheoryDataRowsAsync))]
    public void AsyncMemberData_ValueTaskOfTheoryDataRows(int value, int squared)
    {
        Assert.Equal(squared, value * value);
    }
}
