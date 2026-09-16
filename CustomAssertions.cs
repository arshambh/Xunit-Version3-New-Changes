using Xunit.Sdk;
using Xunit.v3;

namespace xunit3
{

    // Custom exception for Assertion Failure
    public class MyAssertionException : Exception, IAssertionException
    {
        public MyAssertionException(string message) : base(message) { }
    }

    // Custom exception for Timeout Failure
    public class MyTimeoutException : Exception, ITestTimeoutException
    {
        public MyTimeoutException(string message) : base(message) { }
    }

    public class SampleTests
    {
        // 1. FailureCause = Assertion
        [Fact]
        public void Should_fail_as_assertion()
        {
            throw new MyAssertionException("Expected value does not match the actual value");
        }

        // 2. FailureCause = Timeout
        [Fact]
        public void Should_fail_as_timeout()
        {
            throw new MyTimeoutException("The test exceeded the allowed time limit");
        }

        // 3. FailureCause = Exception (default)
        [Fact]
        public void Should_fail_as_exception()
        {
            throw new InvalidOperationException("Something unexpected happened");
        }

        // 4. Dynamic Skip (not a failure)
        [Fact]
        public void Should_skip_dynamically()
        {
            bool shouldSkip = true;

            if (shouldSkip)
            {
                throw new Exception("$XunitDynamicSkip$This test was skipped due to current conditions");
            }

            Assert.True(true);
        }
    }
}