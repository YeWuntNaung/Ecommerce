using System.Reflection;
using Ecommerce.Domain.Primitives;
using NetArchTest.Rules;
using Shouldly;

namespace Ecommerce.ArchitectureTests;

public sealed class LayerBoundaryTests
{
    private static readonly Assembly DomainAssembly = typeof(Entity).Assembly;
    private static readonly Assembly ApplicationAssembly = typeof(Application.DependencyInjection).Assembly;
    private static readonly Assembly ContractsAssembly = typeof(Contracts.Common.PagedRequest).Assembly;
    private static readonly Assembly ApiAssembly = typeof(Api.Common.ResultExtensions).Assembly;

    [Fact]
    public void Domain_has_no_outward_dependencies()
    {
        AssertNoDependency(
            DomainAssembly,
            "Ecommerce.Application",
            "Ecommerce.Infrastructure",
            "Ecommerce.Api",
            "Ecommerce.Contracts",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Contracts_depend_on_nothing_inside_the_product()
    {
        AssertNoDependency(
            ContractsAssembly,
            "Ecommerce.Domain",
            "Ecommerce.Application",
            "Ecommerce.Infrastructure",
            "Ecommerce.Api");
    }

    [Fact]
    public void Application_does_not_depend_on_infrastructure_or_transport()
    {
        AssertNoDependency(
            ApplicationAssembly,
            "Ecommerce.Infrastructure",
            "Ecommerce.Api",
            "Microsoft.EntityFrameworkCore",
            "Microsoft.AspNetCore");
    }

    [Fact]
    public void Api_does_not_reach_into_persistence()
    {
        AssertNoDependency(
            ApiAssembly,
            "Microsoft.EntityFrameworkCore",
            "Npgsql");
    }

    [Fact]
    public void Catalog_application_slice_does_not_depend_on_other_modules_slices()
    {
        var result = Types.InNamespace("Ecommerce.Application.Catalog")
            .ShouldNot()
            .HaveDependencyOn("Ecommerce.Application.Vendors")
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    private static void AssertNoDependency(Assembly assembly, params string[] forbiddenDependencies)
    {
        var result = Types.InAssembly(assembly)
            .ShouldNot()
            .HaveDependencyOnAny(forbiddenDependencies)
            .GetResult();

        result.IsSuccessful.ShouldBeTrue(FailureMessage(result));
    }

    private static string FailureMessage(TestResult result) =>
        result.FailingTypeNames is null
            ? "Architecture rule violated."
            : $"Architecture rule violated by: {string.Join(", ", result.FailingTypeNames)}";
}
