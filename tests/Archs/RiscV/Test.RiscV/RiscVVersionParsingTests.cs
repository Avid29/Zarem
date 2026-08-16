// Avishai Dernis 2026

using Test.Archs;
using Zarem.Models.Versioning;

namespace Test.RiscV;

[TestClass]
public class RiscVVersionParsingTests : VersionParsingTests
{
    [DataTestMethod]
    [DataRow("RV32I")]
    [DataRow("RV32G")]
    [DataRow("RV32ICD")]
    [DataRow("RV64I")]
    [DataRow("RV64G")]
    [DataRow("RV64ICD")]
    public void TestRiscVVersion(string version) => ParsePrintTest<RiscVVersionInfo>(version);
}
