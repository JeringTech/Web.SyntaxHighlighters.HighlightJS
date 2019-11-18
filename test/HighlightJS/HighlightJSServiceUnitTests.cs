using Jering.Javascript.NodeJS;
using Moq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Jering.Web.SyntaxHighlighters.HighlightJS.Tests
{
    public class HighlightJSServiceUnitTests
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };
        private const int _timeoutMS = 60000;

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfNodeJSServiceIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new HighlightJSService(null, _mockRepository.Create<IEmbeddedResourcesService>().Object));
        }

        [Fact]
        public void Constructor_ThrowsArgumentNullExceptionIfEmbeddedResourcesServiceIsNull()
        {
            // Act and assert
            Assert.Throws<ArgumentNullException>(() => new HighlightJSService(_mockRepository.Create<INodeJSService>().Object, null));
        }

        [Fact]
        public async Task HighlightAsync_ThrowsObjectDisposedExceptionIfInstanceHasBeenDisposed()
        {
            // Arrange
            HighlightJSService testSubject = CreateHighlightJSService();
            testSubject.Dispose();

            // Act and assert
            ObjectDisposedException result = await Assert.
                ThrowsAsync<ObjectDisposedException>(async () => await testSubject.HighlightAsync(null, null).ConfigureAwait(false)).
                ConfigureAwait(false);
        }

        [Fact]
        public async Task HighlightAsync_ThrowsArgumentNullExceptionIfCodeIsNull()
        {
            // Arrange
            using (HighlightJSService testSubject = CreateHighlightJSService())
            {
                // Act and assert
                ArgumentNullException result = await Assert.
                    ThrowsAsync<ArgumentNullException>(async () => await testSubject.HighlightAsync(null, null).ConfigureAwait(false)).
                    ConfigureAwait(false);
                Assert.Equal($"{Strings.Exception_ParameterCannotBeNull}\nParameter name: code", result.Message, ignoreLineEndingDifferences: true);
            }
        }

        [Theory]
        [MemberData(nameof(HighlightAsync_ReturnsCodeIfCodeIsEmptyOrWhitespace_Data))]
        public async Task HighlightAsync_ReturnsCodeIfCodeIsEmptyOrWhitespace(string dummyCode)
        {
            // Arrange
            using (HighlightJSService testSubject = CreateHighlightJSService())
            {
                // Act
                string result = await testSubject.HighlightAsync(dummyCode, null).ConfigureAwait(false);

                // Assert
                Assert.Equal(dummyCode, result);
            }
        }

        public static IEnumerable<object[]> HighlightAsync_ReturnsCodeIfCodeIsEmptyOrWhitespace_Data()
        {
            return new object[][]
            {
                new object[]
                {
                    string.Empty
                },
                new object[]
                {
                    " "
                }
            };
        }

        [Fact]
        public async Task HighlightAsync_ThrowsArgumentExceptionIfLanguageAliasIsNotAValidHighlightJSLanguageAlias()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService();
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias, default(CancellationToken))).ReturnsAsync(false);

            // Act and assert
            ArgumentException result = await Assert.
                ThrowsAsync<ArgumentException>(async () => await mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias).ConfigureAwait(false)).
                ConfigureAwait(false);
            Assert.Equal(result.Message, string.Format(Strings.Exception_InvalidHighlightJSLanguageAlias, dummyLanguageAlias));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task HighlightAsync_InvokesFromCacheIfModuleIsCached()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyHighlightedCode = "dummyHighlightedCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            const string dummyClassPrefix = "dummyClassPrefix";
            Mock<INodeJSService> mockNodeJSService = _mockRepository.Create<INodeJSService>();
            mockNodeJSService.
                Setup(n => n.TryInvokeFromCacheAsync<string>(HighlightJSService.MODULE_CACHE_IDENTIFIER,
                    "highlight",
                    It.Is<object[]>(arr => arr[0].Equals(dummyCode) && arr[1].Equals(dummyLanguageAlias) && arr[2].Equals(dummyClassPrefix)),
                    default(CancellationToken))).
                ReturnsAsync((true, dummyHighlightedCode));
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService(mockNodeJSService.Object);
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias, default(CancellationToken))).ReturnsAsync(true);

            // Act
            string result = await mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias, dummyClassPrefix).ConfigureAwait(false);

            // Assert
            Assert.Equal(dummyHighlightedCode, result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task HighlightAsync_InvokesFromStreamIfModuleIsNotCached()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            const string dummyHighlightedCode = "dummyHighlightedCode";
            const string dummyClassPrefix = "dummyClassPrefix";
            var dummyStream = new MemoryStream();
            Mock<IEmbeddedResourcesService> mockEmbeddedResourcesService = _mockRepository.Create<IEmbeddedResourcesService>();
            mockEmbeddedResourcesService.
                Setup(e => e.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, HighlightJSService.BUNDLE_NAME)).
                Returns(dummyStream);
            Mock<INodeJSService> mockNodeJSService = _mockRepository.Create<INodeJSService>();
            mockNodeJSService.
                Setup(n => n.TryInvokeFromCacheAsync<string>(HighlightJSService.MODULE_CACHE_IDENTIFIER,
                    "highlight",
                    It.Is<object[]>(arr => arr[0].Equals(dummyCode) && arr[1].Equals(dummyLanguageAlias) && arr[2].Equals(dummyClassPrefix)),
                    default(CancellationToken))).
                ReturnsAsync((false, null));
            mockNodeJSService.
                Setup(n => n.InvokeFromStreamAsync<string>(dummyStream,
                    HighlightJSService.MODULE_CACHE_IDENTIFIER,
                    "highlight",
                    It.Is<object[]>(arr => arr[0].Equals(dummyCode) && arr[1].Equals(dummyLanguageAlias) && arr[2].Equals(dummyClassPrefix)),
                    default(CancellationToken))).
                ReturnsAsync(dummyHighlightedCode);
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService(mockNodeJSService.Object, mockEmbeddedResourcesService.Object);
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias, default(CancellationToken))).ReturnsAsync(true);

            // Act
            string result = await mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias, dummyClassPrefix).ConfigureAwait(false);

            // Assert
            Assert.Equal(dummyHighlightedCode, result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task IsValidLanguageAliasAsync_ThrowsObjectDisposedExceptionIfInstanceHasBeenDisposed()
        {
            // Arrange
            HighlightJSService testSubject = CreateHighlightJSService();
            testSubject.Dispose();

            // Act and assert
            ObjectDisposedException result = await Assert.
                ThrowsAsync<ObjectDisposedException>(async () => await testSubject.IsValidLanguageAliasAsync(null, default(CancellationToken)).ConfigureAwait(false)).
                ConfigureAwait(false);
        }

        [Theory]
        [MemberData(nameof(IsValidLanguageAliasAsync_ReturnsFalseIfLanguageAliasIsNullOrWhitespace_Data))]
        public async Task IsValidLanguageAliasAsync_ReturnsFalseIfLanguageAliasIsNullOrWhitespace(string dummyLanguageAlias)
        {
            // Arrange
            using (HighlightJSService testSubject = CreateHighlightJSService())
            {
                // Act
                bool result = await testSubject.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

                // Assert
                Assert.False(result);
            }
        }

        public static IEnumerable<object[]> IsValidLanguageAliasAsync_ReturnsFalseIfLanguageAliasIsNullOrWhitespace_Data()
        {
            return new object[][]
            {
                new object[]
                {
                    null
                },
                new object[]
                {
                    string.Empty
                },
                new object[]
                {
                    " "
                }
            };
        }

        [Theory(Timeout = _timeoutMS)]
        [MemberData(nameof(IsValidLanguageAliasAsync_IfSuccessfulReturnsTrueIfAliasesContainsLanguageAliasAndFalseIfItDoesNot_Data))]
        public async Task IsValidLanguageAliasAsync_IfSuccessfulReturnsTrueIfAliasesContainsLanguageAliasAndFalseIfItDoesNot(
            string dummyLanguageAlias,
            string[] dummyAliases,
            bool expectedResult)
        {
            // Arrange
            var dummyStream = new MemoryStream();
            Mock<IEmbeddedResourcesService> mockEmbeddedResourcesService = _mockRepository.Create<IEmbeddedResourcesService>();
            mockEmbeddedResourcesService.
                Setup(e => e.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, HighlightJSService.BUNDLE_NAME)).
                Returns(dummyStream);
            Mock<INodeJSService> mockNodeJSService = _mockRepository.Create<INodeJSService>();
            mockNodeJSService.
                Setup(n => n.InvokeFromStreamAsync<string[]>(
                    dummyStream,
                    HighlightJSService.MODULE_CACHE_IDENTIFIER,
                    "getAliases",
                    null,
                    default(CancellationToken))).
                ReturnsAsync(dummyAliases);
            using (HighlightJSService testSubject = CreateHighlightJSService(mockNodeJSService.Object, mockEmbeddedResourcesService.Object))
            {
                // Act
                bool result = await testSubject.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

                // Assert
                Assert.Equal(expectedResult, result);
            }
            _mockRepository.VerifyAll();
        }

        public static IEnumerable<object[]> IsValidLanguageAliasAsync_IfSuccessfulReturnsTrueIfAliasesContainsLanguageAliasAndFalseIfItDoesNot_Data()
        {
            const string dummyLanguageAlias = "dummyLanguageAlias";

            return new object[][]
            {
                // If aliases contains language alias, should return true
                new object[]
                {
                    dummyLanguageAlias,
                    new string[]{ dummyLanguageAlias },
                    true
                },
                // Otherwise, should return false
                new object[]
                {
                    dummyLanguageAlias,
                    new string[0],
                    false
                }
            };
        }

        [Fact(Timeout = _timeoutMS)]
        public void IsValidLanguageAliasAsync_FirstThreadLazilyRetrievesAliases()
        {
            // Arrange
            const string dummyLanguageAlias = "dummyLanguageAlias";
            var dummyStream = new MemoryStream();
            Mock<IEmbeddedResourcesService> mockEmbeddedResourcesService = _mockRepository.Create<IEmbeddedResourcesService>();
            mockEmbeddedResourcesService.
                Setup(e => e.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, HighlightJSService.BUNDLE_NAME)).
                Returns(dummyStream);
            Mock<INodeJSService> mockNodeJSService = _mockRepository.Create<INodeJSService>();
            mockNodeJSService.
                Setup(n => n.InvokeFromStreamAsync<string[]>(
                    dummyStream,
                    HighlightJSService.MODULE_CACHE_IDENTIFIER,
                    "getAliases",
                    null,
                    default(CancellationToken))).
                ReturnsAsync(new string[] { dummyLanguageAlias });
            using (HighlightJSService testSubject = CreateHighlightJSService(mockNodeJSService.Object, mockEmbeddedResourcesService.Object))
            {
                // Act
                var results = new ConcurrentQueue<bool>();
                const int numThreads = 5;
                var threads = new List<Thread>();
                for (int i = 0; i < numThreads; i++)
                {
                    var thread = new Thread(() => results.Enqueue(testSubject.IsValidLanguageAliasAsync(dummyLanguageAlias).GetAwaiter().GetResult()));
                    threads.Add(thread);
                    thread.Start();
                }
                foreach (Thread thread in threads)
                {
                    thread.Join();
                }

                // Assert
                _mockRepository.VerifyAll();
                mockEmbeddedResourcesService.Verify(e => e.ReadAsStream(typeof(HighlightJSService).GetTypeInfo().Assembly, HighlightJSService.BUNDLE_NAME), Times.Once()); // Only called when aliases hasn't been instantiated
                Assert.Equal(numThreads, results.Count);
                foreach (bool result in results)
                {
                    Assert.True(result);
                }
            }
        }

        [Fact]
        public void Dispose_DoesNothingIfInstanceIsAlreadyDisposed()
        {
            // Arrange
            Mock<INodeJSService> mockNodeJSService = _mockRepository.Create<INodeJSService>();
            HighlightJSService testSubject = CreateHighlightJSService(mockNodeJSService.Object);

            // Act
            testSubject.Dispose();
            testSubject.Dispose();

            // Assert
            mockNodeJSService.Verify(n => n.Dispose(), Times.Once());
        }

        private HighlightJSService CreateHighlightJSService(INodeJSService nodeJSService = null, IEmbeddedResourcesService embeddedResourcesService = null)
        {
            return new HighlightJSService(nodeJSService ?? _mockRepository.Create<INodeJSService>().Object,
                embeddedResourcesService ?? _mockRepository.Create<IEmbeddedResourcesService>().Object);
        }

        private Mock<HighlightJSService> CreateMockHighlightJSService(INodeJSService nodeJSService = null, IEmbeddedResourcesService embeddedResourcesService = null)
        {
            return _mockRepository.Create<HighlightJSService>(nodeJSService ?? _mockRepository.Create<INodeJSService>().Object,
                embeddedResourcesService ?? _mockRepository.Create<IEmbeddedResourcesService>().Object);
        }
    }
}
