using Core.Abstractions.Applications.Validators;
using Core.Application.Pipelines.Validation.Attributes;
using Core.Application.Pipelines.Validation.Behaviors;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Client;
using Xunit;

namespace Core.Tests.Application.Pipelines.ValidationBehaviorTests
{
    public class IdTypeValidationBehaivorTests
    {
        private readonly ServiceProvider _provider;
        public IdTypeValidationBehaivorTests()
        {
            ServiceCollection services = new ServiceCollection();

            //FakeValidator default: true
            services.AddSingleton<IIdTypeValidator>(new FakeValidator(true));

            _provider = services.BuildServiceProvider();
        }

        private async Task<TResponse> ExecuteBehaviorTest<TRequest, TResponse>(TRequest request, TResponse expectedResponse)
            where TRequest : IRequest<TResponse>
        {
            var behavior = CreateBehavior<TRequest, TResponse>();
            bool nextCalled = false;
            RequestHandlerDelegate<TResponse> next = _ =>
            {
                nextCalled = true;
                return Task.FromResult(expectedResponse);
            };
            var result = await behavior.Handle(request, next, default);
            nextCalled.Should().BeTrue();
            result.Should().Be(expectedResponse);
            return result;
        }

        private IdTypeValidationBehavior<TRequest, TResponse> CreateBehavior<TRequest, TResponse>()
                where TRequest : IRequest<TResponse>
        {
            return new IdTypeValidationBehavior<TRequest, TResponse>(_provider);
        }

        public class FakeValidator : IIdTypeValidator
        {
            private readonly bool _result;

            public FakeValidator(bool result)
            {
                _result = result;
            }

            public bool IsValid(string value) => _result;
        }


        // TEST REQUEST TYPES
        public class TestRequestWithoutAttribute : IRequest<Guid>
        {
            public Guid Id { get; set; } = Guid.NewGuid();
        }

        [IdTypeValidator(typeof(IIdTypeValidator), "Invalid ID")]
        public class TestRequestWithoutIdProperty : IRequest<string> { }

        [IdTypeValidator(typeof(IIdTypeValidator), "Invalid ID")]
        public class TestRequestWithAttribute : IRequest<string>
        {
            public string? Id { get; set; }
        }


        [Fact]
        public async Task Should_Skip_When_Attribute_Is_Not_Present()
        {
            var request = new TestRequestWithoutAttribute();
            Guid expected = Guid.NewGuid();

            Guid result = await ExecuteBehaviorTest<TestRequestWithoutAttribute, Guid>(
                request, expected);
            result.Should().Be(expected);
        }
    }
}
