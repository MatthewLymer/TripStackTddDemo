using Xunit;

namespace TripStack.TddDemo.StoreApi.Tests.CurrencyExchange
{
    // 
    // Rules of Test Driven Development (http://butunclebob.com/ArticleS.UncleBob.TheThreeRulesOfTdd)
    // 1. You are not allowed to write any production code unless it is to make a failing unit test pass.
    // 2. You are not allowed to write any more of a unit test than is sufficient to fail; and compilation failures are failures.
    // 3. You are not allowed to write any more production code than is sufficient to pass the one failing unit test.
    // 
    // Red, Green, Blue Loop
    // 1. (Red) Write a failing unit test
    // 2. (Green) Make that unit test pass
    // 3. (Blue) Refactor / clean up both production code and tests
    // 
    // Test Cases
    // 1. Ensure unit test setup works 
    // 
    
    public sealed class CachingGetExchangeRatesDecoratorTests
    {
        [Fact]
        public void ShouldPass()
        {
            Assert.True(true);
        }
    }
}