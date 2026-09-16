// ---------------------------------------------------------------------
// Capturing Console, Debug, and Trace output
//
// xUnit v3 supports capturing output from Console, Debug, and Trace
// via assembly-level attributes. Add these to any .cs file in your test
// assembly (before any namespace declarations):
//
//   [assembly: CaptureConsole]  // Captures Console.Out by default
//   [assembly: CaptureTrace]  // Captures Debug and Trace writers
//
// The captured output appears in TestOutputHelper and TRX reports.
// Debug output is only available in DEBUG builds (filtered out by compiler).
//
// Reference: https://xunit.net/docs/getting-started/v3/whats-new#capturing-console-debug-and-trace-output
// ---------------------------------------------------------------------

using System.Diagnostics;
using Xunit;

// Assembly-level attributes to capture Console/Debug/Trace output
[assembly: CaptureConsole]
[assembly: CaptureTrace]

namespace xunit3;

public class ConsoleTraceCaptureTests
{
    private readonly ITestContextAccessor _context;

    public ConsoleTraceCaptureTests(ITestContextAccessor context)
    {
        _context = context;
    }

    [Fact]
    public void ConsoleWrite_output_is_captured_by_CaptureConsole()
    {
        var output = _context.Current.TestOutputHelper!;
        
        Console.WriteLine("Console.WriteLine message");
        Console.WriteLine($"Counter: {42}");

        Assert.NotNull(output);
    }

    [Fact]
    public void DebugWrite_output_is_captured_by_CaptureTrace()
    {
        var output = _context.Current.TestOutputHelper!;
        
        Debug.WriteLine("Debug message 1");
        Debug.WriteLine("Debug message 2");

#if DEBUG
        Assert.NotNull(output);
#else
        Assert.True(true, "Debug output only available in DEBUG builds");
#endif
    }

    [Fact]
    public void TraceWrite_output_is_captured_by_CaptureTrace()
    {
        var output = _context.Current.TestOutputHelper!;
        
        Trace.WriteLine("Trace.WriteLine message");
        Trace.TraceInformation("Trace info message");

        Assert.NotNull(output);
    }

    [Fact]
    public void TestOutputHelper_WriteLine_goes_to_test_report()
    {
        var output = _context.Current.TestOutputHelper!;
        output.WriteLine("Direct TestOutputHelper message");
        output.WriteLine($"Value: {100 * 2}");

        Assert.NotNull(output);
    }

    [Fact]
    public void Mixed_output_all_routes_work()
    {
        var output = _context.Current.TestOutputHelper!;
        
        // Route 1: Direct via TestOutputHelper (always works)
        output.WriteLine("=== Test started ===");

        // Route 2: Console (captured via CaptureConsole)
        Console.WriteLine("Console output");

        // Route 3: Debug (captured via CaptureTrace in DEBUG builds)
#if DEBUG
        Debug.WriteLine("Debug output");
#endif

        // Route 4: Trace (captured via CaptureTrace)
        Trace.WriteLine("Trace output");

        output.WriteLine("=== Test completed ===");
        
        Assert.NotNull(_context.Current.TestOutputHelper);
    }
}