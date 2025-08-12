namespace UniversityAPI.Tests.UnitTests.Middleware
{
    using System.Text.Json;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.WebUtilities;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Moq;
    using UniversityAPI.Framework.Model;
    using UniversityAPI.Framework.Model.Exception;
    using UniversityAPI.Middleware;
    using UniversityAPI.Tests.Shared.Models;
    using UniversityAPI.Utility.Helpers;

    public class ExceptionHandlingMiddlewareTests
    {
        private readonly Mock<ILogger<ExceptionHandlingMiddleware>> mockLogger;
        private readonly Mock<IWebHostEnvironment> mockEnvironment;

        public ExceptionHandlingMiddlewareTests()
        {
            this.mockLogger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
            this.mockEnvironment = new Mock<IWebHostEnvironment>();
        }

        [Fact]
        public async Task InvokeAsync_NoException_ContinuesToNextMiddleware()
        {
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => Task.CompletedTask,
                this.mockLogger.Object,
                this.mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            Assert.Equal(200, context.Response.StatusCode);
        }

        [Fact]
        public async Task InvokeAsync_ApiException_ReturnsCorrectStatusCodeAndMessage()
        {
            var expectedException = new NotFoundError("Resource not found");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                this.mockLogger.Object,
                this.mockEnvironment.Object
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
                this.mockLogger.Object,
                this.mockEnvironment.Object
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
            this.mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Development);
            var expectedException = new BadRequestError("Invalid input");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                this.mockLogger.Object,
                this.mockEnvironment.Object);

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
            this.mockEnvironment.Setup(x => x.EnvironmentName).Returns(Environments.Production);
            var expectedException = new ConflictError("Resource conflict");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                this.mockLogger.Object,
                this.mockEnvironment.Object);

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
                new() { Exception = new BadRequestError("Bad request"), ExpectedStatusCode = 400 },
                new() { Exception = new NotFoundError("Not found"), ExpectedStatusCode = 404 },
                new() { Exception = new ConflictError("Conflict"), ExpectedStatusCode = 409 }
            };

            foreach (var testCase in testCases)
            {
                var middleware = new ExceptionHandlingMiddleware(
                    next: (context) => throw testCase.Exception,
                    this.mockLogger.Object,
                    this.mockEnvironment.Object
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
                this.mockLogger.Object,
                this.mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            this.mockLogger.Verify(
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
            var expectedException = new NotFoundError("Resource not found");
            var middleware = new ExceptionHandlingMiddleware(
                next: (context) => throw expectedException,
                this.mockLogger.Object,
                this.mockEnvironment.Object
                                                            );

            var context = new DefaultHttpContext();

            await middleware.InvokeAsync(context);

            this.mockLogger.Verify(
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