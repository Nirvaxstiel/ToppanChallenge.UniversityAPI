using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text.Json;
using UniversityAPI.Framework.Model;
using UniversityAPI.Middleware;
using UniversityAPI.Tests.Shared.Models;
using UniversityAPI.Utility;

namespace UniversityAPI.Tests.UnitTests.Middleware
{
    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _mockLogger;
        private readonly Mock<IWebHostEnvironment> _mockEnvironment;

        public ExceptionHandlingMiddlewareTests()
        {
            _mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            _mockEnvironment = new Mock<IWebHostEnvironment>();
        }

        [Fact]
        public async Task InvokeAsync_NoException_ContinuesToNextMiddleware()
        {
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => Task.CompletedTask,
                _mockLogger.Object,
                _mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ApiException_ReturnsCorrectStatusCodeAndMessage()
        {
            var expectedException = new NotFoundException("Resource not found");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal(404, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            Assert.NotNull(response);
            Assert.Equal(404, ConvertHelper.ToInt(response.GetValueOrDefault("Status")));
            Assert.True(StringHelper.Compare(ReasonPhrases.GetReasonPhrase(404), ConvertHelper.ToString(response["ErrorCode"])));
            Assert.True(StringHelper.Compare("Resource not found", ConvertHelper.ToString(response["Message"])));
        }

        [Fact]
        public async Task InvokeAsync_GeneralException_Returns500StatusCode()
        {
            var expectedException = new InvalidOperationException("Something went wrong");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            Assert.Equal(500, context.Response.StatusCode);
            Assert.Equal("application/json", context.Response.ContentType);

            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            Assert.NotNull(response);
            Assert.Equal(500, ConvertHelper.ToInt(response["Status"]));
            Assert.True(StringHelper.Compare("UNKNOWN_ERROR", response["ErrorCode"]));
            Assert.True(StringHelper.Compare("Something went wrong", response["Message"]));
        }

        [Fact]
        public async Task InvokeAsync_DevelopmentEnvironment_IncludesExceptionDetails()
        {
            _mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Development);
            var expectedException = new BadRequestException("Invalid input");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            Assert.NotNull(response);
            Assert.NotNull(response["Details"]);
            Assert.True(StringHelper.Contains(ConvertHelper.ToString(response["Details"]), "Invalid input"));
        }

        [Fact]
        public async Task InvokeAsync_ProductionEnvironment_ExcludesExceptionDetails()
        {
            _mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
            var expectedException = new ConflictException("Resource conflict");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object);

            var context = new DefaultHttpContext();
            context.Response.Body = new MemoryStream();

            await middleware.InvokeAsync(context);

            context.Response.Body.Position = 0;
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

            Assert.NotNull(response);
            Assert.Null(response["Details"]);
        }

        [Fact]
        public async Task InvokeAsync_DifferentApiExceptionTypes_ReturnCorrectStatusCodes()
        {
            var testCases = new List<TestExceptionModel>
            {
                new TestExceptionModel { Exception = new BadRequestException("Bad request"), ExpectedStatusCode = 400 },
                new TestExceptionModel { Exception = new NotFoundException("Not found"), ExpectedStatusCode = 404 },
                new TestExceptionModel { Exception = new ConflictException("Conflict"), ExpectedStatusCode = 409 }
            };

            foreach (var testCase in testCases)
            {
                var middleware = new ExceptionHandlingMiddleware(
                    next: (context) => throw testCase.Exception,
                    _mockLogger.Object,
                    _mockEnvironment.Object
                                                                );

                var context = new DefaultHttpContext();
                context.Response.Body = new MemoryStream();

                await middleware.InvokeAsync(context);

                Assert.Equal(testCase.ExpectedStatusCode, context.Response.StatusCode);
                Assert.Equal("application/json", context.Response.ContentType);

                context.Response.Body.Position = 0;
                var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
                var response = JsonSerializer.Deserialize<Dictionary<string, object>>(responseBody);

                Assert.NotNull(response);
                Assert.Equal(testCase.ExpectedStatusCode, ConvertHelper.ToInt(response["Status"]));
                Assert.True(StringHelper.Compare(testCase.Exception.Message, response["Message"]));
            }
        }

        [Fact]
        public async Task InvokeAsync_LogsExceptions()
        {
            var expectedException = new Exception("Test exception");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task InvokeAsync_ApiException_LogsWithCorrectMessage()
        {
            var expectedException = new NotFoundException("Resource not found");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                _mockLogger.Object,
                _mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("API Exception occurred")),
                    expectedException,
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()),
                Times.Once);
        }
    }
}