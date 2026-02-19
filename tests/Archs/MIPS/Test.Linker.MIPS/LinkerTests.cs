// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Zarem.Assembler.Logging;
using Zarem.Assembler.Logging.Enum;
using Zarem.Linker;
using Zarem.Linker.Config;
using Zarem.Linker.Handler;
using Zarem.Models;

namespace Test.Linker.MIPS;

[TestClass]
public class LinkerTests
{
    [TestMethod]
    public void WrongArchitecture()
    {
        var mipsModule = new Module("MIPS");
        var riscvModule = new Module("RISC-V");

        var logger = new Logger();
        ZaLinker.Link(new LinkerConfig(), new MIPSLinkerHandler(), logger, mipsModule, riscvModule);

        Assert.AreEqual((uint)LogId.WrongArchitecture, logger.CurrentLog[0].Code.Id);
    }
}
