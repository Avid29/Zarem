// Avishai Dernis 2025

using System.Collections.Generic;
using System.Threading.Tasks;
using Test.Zarem;
using Zarem.Elf;
using Zarem.MIPS;
using Zarem.Registry;
using Zarem.TrapHandlers;

namespace Test.Mips.Projects;

[TestClass]
public class ProjectTests : ProjectTestsBase
{
    public static IEnumerable<object[]> ProjectsPaths
    {
        get
        {
            foreach (var path in GetProjectPaths("Mips"))
                yield return new object[] { path };
        }
    }

    [TestInitialize]
    public void RegisterDescriptors()
    {
        ZaremRegistry.Formats.Register(new ElfModuleDescriptor());
        ZaremRegistry.TrapHandlers.Register(new ZaremTrapHandlerDescriptor());
        ZaremRegistry.RegisterArchitecture(new MipsArchitectureDescriptor());
    }

    [DataTestMethod]
    [DynamicData(nameof(ProjectsPaths))]
    public Task RunTestAsync(string projectPath) => RunProjectTest(projectPath);
}
