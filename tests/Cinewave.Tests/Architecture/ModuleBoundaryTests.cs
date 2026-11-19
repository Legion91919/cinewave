using System.Reflection;
using System.Runtime.CompilerServices;

namespace Cinewave.Architecture.Tests;

/// <summary>Fails if a module reaches into another module, or exposes more than its Contracts.</summary>
public class ModuleBoundaryTests
{
    private static readonly Assembly[] Modules =
    [
        typeof(Cinewave.Catalogue.CatalogueModule).Assembly,
        typeof(Cinewave.Playback.PlaybackModule).Assembly,
        typeof(Cinewave.Subscriptions.SubscriptionsModule).Assembly,
        typeof(Cinewave.Profiles.ProfilesModule).Assembly,
    ];

    private static readonly HashSet<string> ModuleNames = Modules.Select(m => m.GetName().Name!).ToHashSet();

    [Fact]
    public void Modules_do_not_reference_other_modules()
    {
        Assert.All(Modules, module =>
        {
            var name = module.GetName().Name!;
            var offending = module.GetReferencedAssemblies().Select(r => r.Name!).Where(r => ModuleNames.Contains(r) && r != name).ToList();
            Assert.True(offending.Count == 0, $"{name} references {string.Join(", ", offending)}");
        });
    }

    [Fact]
    public void Modules_only_expose_contracts_and_their_registration_class()
    {
        Assert.All(Modules, module =>
        {
            var name = module.GetName().Name!;
            var leaks = module.GetExportedTypes()
                .Where(t => t.Namespace != $"{name}.Contracts" && !(t.Namespace == name && t.Name.EndsWith("Module", StringComparison.Ordinal)))
                .Select(t => t.FullName)
                .ToList();
            Assert.True(leaks.Count == 0, $"{name} exposes internals: {string.Join(", ", leaks)}");
        });
    }

    [Fact]
    public void Module_internals_are_only_visible_to_the_test_project()
    {
        Assert.All(Modules, module =>
        {
            var friends = module.GetCustomAttributes<InternalsVisibleToAttribute>().Select(a => a.AssemblyName).ToList();
            Assert.All(friends, f => Assert.Equal("Cinewave.Tests", f));
        });
    }

    [Fact]
    public void Contract_signatures_only_use_contract_or_framework_types()
    {
        Assert.All(Modules, module =>
        {
            var name = module.GetName().Name!;
            foreach (var type in module.GetExportedTypes().Where(t => t.Namespace == $"{name}.Contracts"))
            {
                var used = type.GetMethods().SelectMany(m => m.GetParameters().Select(p => p.ParameterType).Append(m.ReturnType))
                    .Concat(type.GetProperties().Select(p => p.PropertyType))
                    .SelectMany(Flatten);
                foreach (var t in used.Where(t => t.Assembly == module))
                {
                    Assert.True(t.IsPublic || t.IsNestedPublic, $"{type.Name} exposes non-public {t.FullName}");
                }
            }
        });
    }

    private static IEnumerable<Type> Flatten(Type t) =>
        t.IsGenericType ? t.GetGenericArguments().SelectMany(Flatten).Append(t) : [t];
}
