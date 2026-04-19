using Canvas.Infrastructure.Shared.Behaviors;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using MediatR;
using NSubstitute;

namespace Canvas.Infrastructure.Shared.Tests.Behaviors;

public sealed record ValidationTestRequest : IRequest<string>;

public class ValidationBehaviorTests
{
    [Fact]
    public async Task Handle_NoValidators_CallsNext()
    {
        var behavior = new ValidationBehavior<ValidationTestRequest, string>([]);
        var called = false;
        RequestHandlerDelegate<string> next = () => { called = true; return Task.FromResult("ok"); };

        var result = await behavior.Handle(new ValidationTestRequest(), next, default);

        result.Should().Be("ok");
        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ValidRequest_CallsNext()
    {
        var validator = Substitute.For<IValidator<ValidationTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([validator]);
        var called = false;
        RequestHandlerDelegate<string> next = () => { called = true; return Task.FromResult("ok"); };

        await behavior.Handle(new ValidationTestRequest(), next, default);

        called.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_InvalidRequest_ThrowsValidationException()
    {
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        var validator = Substitute.For<IValidator<ValidationTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([validator]);
        RequestHandlerDelegate<string> next = () => Task.FromResult("ok");

        var act = () => behavior.Handle(new ValidationTestRequest(), next, default);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Handle_InvalidRequest_DoesNotCallNext()
    {
        var failures = new List<ValidationFailure> { new("Name", "Name is required") };
        var validator = Substitute.For<IValidator<ValidationTestRequest>>();
        validator.ValidateAsync(Arg.Any<ValidationContext<ValidationTestRequest>>(), Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(failures));

        var behavior = new ValidationBehavior<ValidationTestRequest, string>([validator]);
        var called = false;
        RequestHandlerDelegate<string> next = () => { called = true; return Task.FromResult("ok"); };

        try { await behavior.Handle(new ValidationTestRequest(), next, default); } catch { }

        called.Should().BeFalse();
    }
}
