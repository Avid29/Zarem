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
    [DataRow("RV32ICD", true)]
    [DataRow("RV64I")]
    [DataRow("RV64G")]
    [DataRow("RV64ICD", true)]
    public void TestRiscVVersion(string version, bool reparse = false) => ParsePrintTest<RiscVVersionInfo>(version, reparse);
}
