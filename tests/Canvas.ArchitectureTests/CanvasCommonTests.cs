using Canvas.Common.Primitives;
using FluentAssertions;
using NetArchTest.Rules;

namespace Canvas.ArchitectureTests;

public class CanvasCommonTests
{
    [Fact]
    public void CanvasCommon_ShouldNotDependOnAnyOtherCanvasAssembly()
    {
        var result = Types
            .InAssembly(typeof(Entity<>).Assembly)
            .ShouldNot()
            .HaveDependencyOnAny(
                "Canvas.Infrastructure",
                "Canvas.Infrastructure.Shared",
                "Canvas.Host",
                "Canvas.Idea",
                "Canvas.Project",
                "Canvas.Planning",
                "Canvas.Architecture",
                "Canvas.Memory",
                "Canvas.Integration")
            .GetResult();

        result.IsSuccessful.Should().BeTrue();
    }
}
