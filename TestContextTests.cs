using System.Text;

// ---------------------------------------------------------------------
// TRX report generation (requires the Microsoft.Testing.Extensions.TrxReport package):
//
//   dotnet test -- --report-trx
//       → Generates a TRX report with the default name in the TestResults folder
//         (e.g. TestResults\xunit3_net8.0_x64.trx)
//         Note: the "--" separator is required so the argument is passed to MTP (Microsoft.Testing.Platform), not MSBuild.
//
//   dotnet test -- --report-trx --report-trx-filename TestContext
//       → Custom report file name:
//         TestResults\TestContext_net8.0_x64.trx
//
//   dotnet test -- --report-trx --report-trx-directory <path>
//       → Custom output folder for the TRX report, e.g.:
//         dotnet test -- --report-trx --report-trx-directory C:\logs\trx
//
//    Samples for new assertion library features in xUnit v3:
//        https://xunit.net/docs/getting-started/v3/whats-new#test-context
// ---------------------------------------------------------------------

namespace xunit3;

public class TestContextTests
{
    private readonly ITestContextAccessor _context;

    public TestContextTests(ITestContextAccessor context)
    {
        _context = context;
    }

    /// <summary>
    /// Demonstrates all overloads of SendDiagnosticMessage.
    ///
    /// SendDiagnosticMessage is like a hidden "console.log" for tests:
    /// the messages are INVISIBLE in normal runs ("dotnet test") — MTP mode
    /// does not display them, and "dotnet test -- --diagnostics" only logs
    /// MTP platform diagnostics, NOT these messages.
    ///
    /// Verified working solution — run the xUnit v3 console runner directly:
    ///
    ///     xunit3.exe -class xunit3.TestContextTests -diagnostics
    ///
    /// Useful console-runner options:
    ///     -diagnostics : shows TestContext.Current.SendDiagnosticMessage output
    ///     -method xunit3.TestContextTests.SendDiagnosticMessage_Demo : run just this test
    ///     -reporter verbose : extra progress detail
    ///
    /// Sample output:
    ///     [xunit3] Starting discount calculation
    ///     [xunit3] Input: price=100, percent=20
    ///     [xunit3] Count: 42
    ///     [xunit3] End: ok at 07:25:55
    ///     [xunit3] Multiple values: A, 123, True
    ///     [xunit3] Test completed at 07:25:55.
    ///     [xunit3] Starting Test2
    ///     [xunit3] Finished Test2
    ///
    /// Use it for debugging/troubleshooting breadcrumbs (what input was used,
    /// how long a step took, etc.) without polluting normal test output.
    /// It differs from TestOutputHelper.WriteLine, which is always visible
    /// in the test result output (TRX / Test Explorer).
    /// </summary>
    [Fact]
    public void SendDiagnosticMessage_Demo()
    {
        _context.Current.SendDiagnosticMessage("Starting discount calculation");
        _context.Current.SendDiagnosticMessage("Input: price={0}, percent={1}", 100, 20);
        _context.Current.SendDiagnosticMessage("Count: {0}", 42);
        _context.Current.SendDiagnosticMessage("End: {0} at {1:HH:mm:ss}", "ok", DateTime.UtcNow);
        _context.Current.SendDiagnosticMessage("Multiple values: {0}, {1}, {2}", "A", 123, true);

        // Messages merged from DiagnosticMessageExamples
        Thread.Sleep(100);
        string status = "completed";
        DateTime endTime = DateTime.UtcNow;
        _context.Current.SendDiagnosticMessage("Test {0} at {1:HH:mm:ss}.", status, endTime);
        _context.Current.SendDiagnosticMessage("Starting Test2");
        Thread.Sleep(50);
        _context.Current.SendDiagnosticMessage("Finished Test2");

        Assert.True(true);
    }

    /// <summary>
    /// Demonstrates the KeyValueStorage property of TestContext.
    ///
    /// KeyValueStorage is a shared dictionary that lives for the entire test
    /// pipeline (assembly → collection → class → test). It exists to let
    /// extensibility points talk to each other: an assembly-level fixture,
    /// a before-test event handler, or a custom framework extension can put
    /// a value in, and another stage later can read it back.
    ///
    /// In real life you'd use it for things like sharing a database seed,
    /// a correlation ID, or a feature flag that an earlier pipeline stage
    /// computed for the tests that follow.
    ///
    /// Two important notes:
    /// - The values are thrown away when the pipeline finishes; this is NOT
    ///   a caching or persistence mechanism.
    /// - Prefix your keys (e.g. "myapp:") to avoid collisions with keys
    ///   written by other extensions.
    /// </summary>
    [Fact]
    public void KeyValueStorage_Demo()
    {
        TestContext.Current.KeyValueStorage["xunit3:stage"] = "before-assert";
        TestContext.Current.KeyValueStorage["xunit3:seed"] = 12345;

        var seed = (int)TestContext.Current.KeyValueStorage["xunit3:seed"]!;
        Assert.Equal(12345, seed);
        Assert.Equal("before-assert", TestContext.Current.KeyValueStorage["xunit3:stage"]);
    }

    // ------------------------------------------------------------------
    // 3. AddWarning and Warnings
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates AddWarning and the Warnings collection.
    ///
    /// AddWarning attaches a non-fatal warning to the test result: the test
    /// still PASSES, but the message is reported alongside the result in
    /// Test Explorer, the TRX report, and the console runner output.
    ///
    /// To see the warning text in the console, run the xUnit v3 runner directly:
    ///
    ///     .\bin\Debug\net8.0\xunit3.exe -method xunit3.TestContextTests.AddWarning_Demo -diagnostics
    ///
    /// Useful for flagging things like "this test used a Mock" or
    /// "skipped live-service verification" without failing the test.
    /// </summary>
    [Fact]
    public void AddWarning_Demo()
    {
        TestContext.Current.AddWarning("This test uses a Mock because no real database is available.");

        Assert.NotEmpty(TestContext.Current.Warnings!);
    }

    // ------------------------------------------------------------------
    // 4. AddAttachment — all overloads
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates the string-content overload of AddAttachment.
    ///
    /// Attachments are files saved alongside the test result. To see the
    /// attached file, run the test via the console runner:
    ///
    ///     .\bin\Debug\net8.0\xunit3.exe -method xunit3.TestContextTests.AddAttachment_Text_Demo
    ///
    /// The text is written to:
    ///     TestResults\<run-folder>\In\<guid>\<session>\<execution-log.txt>
    /// </summary>
    [Fact]
    public void AddAttachment_Text_Demo()
    {
        // Overload: AddAttachment(string name, string content)
        // Saved as a plain-text file named "execution-log.txt".
        var log = "Line 1\nLine 2\nLine 3";
        TestContext.Current.AddAttachment("execution-log", log);

        Assert.True(true);
    }

    /// <summary>
    /// Demonstrates the binary (byte[]) overload of AddAttachment.
    ///
    /// Use this overload for any non-text payload (JSON, images, PDFs, zips…).
    ///
    /// NOTE: the standalone console runner (xunit3.exe) runs the test but does
    /// NOT save attachments to disk. Run via MTP (dotnet test) instead:
    ///
    ///     dotnet test -- --filter "xunit3.TestContextTests.AddAttachment_Binary_Demo"
    ///
    /// The bytes are written to:
    ///     TestResults\<run-folder>\In\<guid>\<session>\response.json   ({"ok":true})
    ///
    /// Command reference:
    ///     Run all tests + TRX:
    ///         dotnet test -- --report-trx
    /// </summary>
    [Fact]
    public void AddAttachment_Binary_Demo()
    {
        // Overload: AddAttachment(string name, byte[] content, string mediaType)
        // Saved as a binary file named "response.json" with a JSON media type.
        var bytes = Encoding.UTF8.GetBytes("{\"ok\":true}");
        TestContext.Current.AddAttachment("response.json", bytes, "application/json");

        Assert.True(true);
    }

    /// <summary>
    /// Demonstrates the replaceExistingValue overload of AddAttachment.
    ///
    /// By default, attaching two files with the same name throws or keeps the
    /// first one; passing replaceExistingValue: true overwrites the earlier
    /// attachment with the same name.
    ///
    ///     dotnet test -- --filter "xunit3.TestContextTests.AddAttachment_Replace_Demo"
    ///
    /// The file on disk ("log.txt") ends up containing ONLY "Second value":
    ///     TestResults\<run-folder>\In\<guid>\<session>\log.txt
    ///
    /// Command reference:
    ///     Run all tests + TRX:
    ///         dotnet test -- --report-trx
    /// </summary>
    [Fact]
    public void AddAttachment_Replace_Demo()
    {
        // Overload: AddAttachment(string name, string content, bool replaceExistingValue)
        // The second call overwrites "First value".
        TestContext.Current.AddAttachment("log", "First value");
        TestContext.Current.AddAttachment("log", "Second value", replaceExistingValue: true);

        Assert.True(true);
    }
    // ------------------------------------------------------------------
    // 5. Attachments — reading registered attachments
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates reading attachments via the TestContext.Attachments
    /// dictionary (name → attachment metadata) while the test is running.
    ///
    /// Useful for assertions or for letting pipeline stages discover what was attached.
    ///
    /// Command reference:
    ///     Run one test with attachments saved:
    ///         dotnet test -- --filter "xunit3.TestContextTests.Attachments_Are_Readable_In_Test_Demo"
    ///     Run all tests + TRX:
    ///         dotnet test -- --report-trx
    /// </summary>
    [Fact]
    public void Attachments_Are_Readable_In_Test_Demo()
    {
        TestContext.Current.AddAttachment("summary", "all good");

        Assert.NotNull(TestContext.Current.Attachments);
        Assert.True(TestContext.Current.Attachments!.ContainsKey("summary"));
    }

    // ------------------------------------------------------------------
    // 6. CancellationToken — fast test cancellation
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates TestContext.Current.CancellationToken for fast test cancellation.
    ///
    /// The token fires when the runner requests cancellation (Ctrl+C in the
    /// console, test-run abort in Test Explorer, or a timeout). Pass it to
    /// async operations so the test stops immediately instead of running to
    /// completion: Task.Delay(…, token), HttpClient.SendAsync(…, token), etc.
    ///
    /// Note: CancellationToken alone does NOT enforce a time limit. To make a
    /// test fail when it runs too long, combine it with the Timeout property:
    ///
    ///     [Fact(Timeout = 5000)]   // fails after 5 seconds
    ///     public void MyTest() { … }
    /// </summary>
    [Fact(Timeout = 5000)]
    public async Task CancellationToken_Demo()
    {
        var token = TestContext.Current.CancellationToken;
        Assert.False(token.IsCancellationRequested);

        // If the runner cancels the test, Task.Delay stops immediately
        await Task.Delay(10000, token);

        Assert.True(true);
    }

    // ------------------------------------------------------------------
    // 8. TestOutputHelper — text output for the test (v2's ITestOutputHelper equivalent)
    // ------------------------------------------------------------------
    [Fact]
    public void TestOutputHelper_Demo()
    {
        var output = TestContext.Current.TestOutputHelper;
        Assert.NotNull(output);
        output!.WriteLine("This text is shown in the test output (TRX/console output).");
    }

    // ------------------------------------------------------------------
    // 9. PipelineStage and test engine statuses
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates TestContext.Current.PipelineStage and the test engine
    /// status properties.
    ///
    /// PipelineStage (TestPipelineStage) tells you WHERE in the pipeline the
    /// current code is running (e.g. TestExecution, BeforeTest, AfterTest).
    ///
    /// What it's used for:
    /// - Extensibility points (BeforeTest/AfterTest events, fixtures, custom
    ///   framework extensions) can check "where am I?" and "is anything
    ///   already failing?" before doing work — e.g. skip expensive logging
    ///   or cleanup when the test has already failed.
    /// - Diagnostics: log which stage a message came from, or attach that
    ///   info to the test result.
    ///
    /// During a running test all of these report TestEngineStatus.Running.
    /// </summary>
    [Fact]
    public void PipelineStage_Demo()
    {
        // ------------------------------------------------------------------
        // All TestPipelineStage options, in real pipeline order:
        // ------------------------------------------------------------------
        var allStages = new[]
        {
            TestPipelineStage.Unknown,                  // 0 — not yet assigned / no context
            TestPipelineStage.Initialization,           // 1 — framework setup (before discovery)
            TestPipelineStage.Discovery,                // 2 — test discovery
            TestPipelineStage.TestAssemblyExecution,    // 3 — assembly-level fixtures/events
            TestPipelineStage.TestCollectionExecution,  // 4 — collection-level fixtures/events
            TestPipelineStage.TestClassExecution,       // 5 — class-level fixtures (constructor/dispose)
            TestPipelineStage.TestCaseExecution,        // 6 — test-case-level events
            TestPipelineStage.TestMethodExecution,      // 7 — method-level events (BeforeTest/AfterTest)
            TestPipelineStage.TestExecution,            // 8 — the test method body itself
        };

        foreach (var stage in allStages)
        {
            // Every stage value is defined and readable
            Assert.True(Enum.IsDefined(stage));
        }
    }

    // ------------------------------------------------------------------
    // 10. TestClassInstance and TestState
    // ------------------------------------------------------------------
    /// <summary>
    /// Demonstrates TestClassInstance and TestState.
    ///
    /// TestClassInstance is the live instance of the test class that is
    /// running the current test. xUnit creates a NEW class instance per test
    /// method, so inside a test it is always the same object as `this`.
    /// Useful in extensibility points (BeforeAfterTestAttribute, events,
    /// fixtures) where `this` is a different object — it gives you access to
    /// the test class's fields/state without any cast gymnastics
    /// (returned as object?, so cast to the expected type when needed).
    ///
    /// TestState holds the result data of the currently executing test
    /// (outcome, timing, etc.). It is null while the test body runs and is
    /// only populated AFTER execution completes — so it's meaningful from
    /// extensibility points that observe the finished test (e.g. AfterTest,
    /// after-test events), not from inside the test itself.
    /// </summary>
    [Fact]
    public void TestClassInstance_And_TestState_Demo()
    {
        Assert.Same(this, TestContext.Current.TestClassInstance);

        // TestState holds the test result and is not populated until execution completes
        Assert.Null(TestContext.Current.TestState);
    }
}

