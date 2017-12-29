using System;
using System.Linq;
using System.Reflection;
using SimpleMusicPlayer.Core;
using SimpleMusicPlayer.ViewModels;
using Xunit;

namespace SimpleMusicPlayer.Tests
{
    /// <summary>
    /// Test to avoid async void methods
    /// from: http://haacked.com/archive/2014/11/11/async-void-methods/
    /// by: https://github.com/haacked
    /// </summary>
    public class AsyncVoidMethodsTest
    {
        //AssertExtensions
        private static void AssertNoAsyncVoidMethods(Assembly assembly)
        {
            var messages = assembly
                .GetAsyncVoidMethods()
                .Select(method =>
                    String.Format("'{0}.{1}' is an async void method.",
                        method.DeclaringType.FullName,
                        method.Name))
                .ToList();
            Assert.False(messages.Any(),
                "Async void methods found!" + Environment.NewLine + String.Join(Environment.NewLine, messages));
        }

        [Fact]
        public void EnsureNoAsyncVoidTests()
        {
            AssertNoAsyncVoidMethods(GetType().Assembly);
            AssertNoAsyncVoidMethods(typeof(MainViewModel).Assembly);
        }
    }
}