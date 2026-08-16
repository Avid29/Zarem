// Avishai Dernis 2026

using Test.Archs;
using Zarem.Mips.Models.Versioning;

namespace Test.Mips;

[TestClass]
public class MipsVersionParsingTests : VersionParsingTests
{
    [DataTestMethod]
    [DataRow("mips1")]
    [DataRow("mips2")]
    [DataRow("mips3")]
    [DataRow("mips3_32bit")]
    [DataRow("mips4")]
    [DataRow("mips4_32bit")]
    [DataRow("mips5")]
    [DataRow("mips5_32bit")]
    [DataRow("MipsI")]
    [DataRow("MipsII")]
    [DataRow("MipsIII")]
    [DataRow("MipsIV")]
    [DataRow("MipsV")]
    [DataRow("mips32r1")]
    [DataRow("mips64r1")]
    [DataRow("mips32r2")]
    [DataRow("mips64r2")]
    public void TestMipsVersion(string version) => ParsePrintTest<MipsVersionInfo>(version);
}
