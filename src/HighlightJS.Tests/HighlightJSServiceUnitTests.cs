using Microsoft.AspNetCore.NodeServices;
using Microsoft.AspNetCore.NodeServices.HostingModels;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace JeremyTCD.WebUtils.SyntaxHighlighters.HighlightJS.Tests
{
    public class HighlightJSServiceUnitTests
    {
        private readonly MockRepository _mockRepository = new MockRepository(MockBehavior.Default) { DefaultValue = DefaultValue.Mock };

        [Fact]
        public async Task HighlightAsync_ThrowsExceptionIfCodeIsNull()
        {
            // Arrange
            HighlightJSService highlightJSService = CreateHighlightJSService();

            // Act and assert
            ArgumentNullException result = await Assert.ThrowsAsync<ArgumentNullException>(() => highlightJSService.HighlightAsync(null, null)).ConfigureAwait(false);
            Assert.Equal($"{Strings.Exception_ParameterCannotBeNull}\nParameter name: code", result.Message, ignoreLineEndingDifferences: true);
        }

        [Theory]
        [MemberData(nameof(HighlightAsync_ReturnsCodeIfCodeIsEmptyOrWhitespace_Data))]
        public async Task HighlightAsync_ReturnsCodeIfCodeIsEmptyOrWhitespace(string dummyCode)
        {
            // Arrange
            HighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            string result = await highlightJSService.HighlightAsync(dummyCode, null).ConfigureAwait(false);

            // Assert
            Assert.Equal(dummyCode, result);
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
        public async Task HighlightAsync_ThrowsExceptionIfLanguageAliasIsNotAValidHighlightJSLanguageAlias()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService();
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias)).ReturnsAsync(false);

            // Act and assert
            ArgumentException result = await Assert.ThrowsAsync<ArgumentException>(() => mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias)).ConfigureAwait(false);
            Assert.Equal(result.Message, string.Format(Strings.Exception_InvalidHighlightJSLanguageAlias, dummyLanguageAlias));
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task HighlightAsync_ThrowsExceptionIfANodeErrorOccurs()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            const string dummyClassPrefix = "dummyClassPrefix";
            var dummyNodeInvocationException = new NodeInvocationException("", "");
            var dummyAggregateException = new AggregateException("", dummyNodeInvocationException);
            Mock<INodeServices> mockNodeServices = _mockRepository.Create<INodeServices>();
            mockNodeServices.
                Setup(n => n.InvokeExportAsync<string>(HighlightJSService.BUNDLE, "highlight", dummyCode, dummyLanguageAlias, dummyClassPrefix)).
                ThrowsAsync(dummyAggregateException);
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService(mockNodeServices.Object);
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias)).ReturnsAsync(true);

            // Act and assert
            NodeInvocationException result = await Assert.
                ThrowsAsync<NodeInvocationException>(() => mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias, dummyClassPrefix)).
                ConfigureAwait(false);
            Assert.Same(dummyNodeInvocationException, result);
            _mockRepository.VerifyAll();
        }

        [Fact]
        public async Task HighlightAsync_IfSuccessfulInvokesHighlightInInteropJSAndReturnsHighlightedCode()
        {
            // Arrange
            const string dummyCode = "dummyCode";
            const string dummyHighlightedCode = "dummyHighlightedCode";
            const string dummyLanguageAlias = "dummyLanguageAlias";
            const string dummyClassPrefix = "dummyClassPrefix";
            Mock<INodeServices> mockNodeServices = _mockRepository.Create<INodeServices>();
            mockNodeServices.Setup(n => n.InvokeExportAsync<string>(HighlightJSService.BUNDLE, "highlight", dummyCode, dummyLanguageAlias, dummyClassPrefix)).ReturnsAsync(dummyHighlightedCode);
            Mock<HighlightJSService> mockHighlightJSService = CreateMockHighlightJSService(mockNodeServices.Object);
            mockHighlightJSService.CallBase = true;
            mockHighlightJSService.Setup(p => p.IsValidLanguageAliasAsync(dummyLanguageAlias)).ReturnsAsync(true);

            // Act
            string result = await mockHighlightJSService.Object.HighlightAsync(dummyCode, dummyLanguageAlias, dummyClassPrefix).ConfigureAwait(false);

            // Assert
            Assert.Equal(dummyHighlightedCode, result);
            _mockRepository.VerifyAll();
        }

        [Theory]
        [MemberData(nameof(IsValidLanguageAliasAsync_ReturnsFalseIfLanguageAliasIsNullOrWhitespace_Data))]
        public async Task IsValidLanguageAliasAsync_ReturnsFalseIfLanguageAliasIsNullOrWhitespace(string dummyLanguageAlias)
        {
            // Arrange
            HighlightJSService highlightJSService = CreateHighlightJSService();

            // Act
            bool result = await highlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

            // Assert
            Assert.False(result);
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

        [Theory]
        [MemberData(nameof(IsValidLanguageAliasAsync_IfSuccessfulReturnsTrueIfAliasesContainsLanguageAliasAndFalseIfItDoesNot_Data))]
        public async Task IsValidLanguageAliasAsync_IfSuccessfulReturnsTrueIfAliasesContainsLanguageAliasAndFalseIfItDoesNot(
            string dummyLanguageAlias,
            string[] dummyAliases,
            bool expectedResult)
        {
            // Arrange
            Mock<INodeServices> mockNodeServices = _mockRepository.Create<INodeServices>();
            mockNodeServices.Setup(n => n.InvokeExportAsync<string[]>(HighlightJSService.BUNDLE, "getAliases")).ReturnsAsync(dummyAliases);
            HighlightJSService highlightJSService = CreateHighlightJSService(mockNodeServices.Object);

            // Act
            bool result = await highlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias).ConfigureAwait(false);

            // Assert
            Assert.Equal(expectedResult, result);
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

        [Fact]
        public async Task IsValidLanguageAliasAsync_ThrowsExceptionIfANodeErrorOccurs()
        {
            // Arrange
            const string dummyLanguageAlias = "dummyLanguageAlias";
            var dummyNodeInvocationException = new NodeInvocationException("", "");
            var dummyAggregateException = new AggregateException("", dummyNodeInvocationException);
            Mock<INodeServices> mockNodeServices = _mockRepository.Create<INodeServices>();
            mockNodeServices.Setup(n => n.InvokeExportAsync<string[]>(HighlightJSService.BUNDLE, "getAliases")).ThrowsAsync(dummyAggregateException);
            HighlightJSService highlightJSService = CreateHighlightJSService(mockNodeServices.Object);

            // Act and assert
            NodeInvocationException result = await Assert.ThrowsAsync<NodeInvocationException>(() => highlightJSService.IsValidLanguageAliasAsync(dummyLanguageAlias)).ConfigureAwait(false);
            Assert.Same(dummyNodeInvocationException, result);
            _mockRepository.VerifyAll();
        }

        private HighlightJSService CreateHighlightJSService(INodeServices nodeServices = null)
        {
            return new HighlightJSService(nodeServices);
        }

        private Mock<HighlightJSService> CreateMockHighlightJSService(INodeServices nodeServices = null)
        {
            return _mockRepository.Create<HighlightJSService>(nodeServices);
        }
    }
}
