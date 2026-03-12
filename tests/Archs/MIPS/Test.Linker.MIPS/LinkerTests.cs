// Avishai Dernis 2026

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
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
        var mipsModule = new Module("MipsModule", "MIPS");
        var riscvModule = new Module("RiscvModule", "RISC-V");

        var logger = new Logger();
        var config = new MipsLinkerConfig();
        var handler = new MipsLinkerHandler(config);
        ZaLinker.Link(config, handler, logger, mipsModule, riscvModule);

        var log = logger.CurrentLog.FirstOrDefault(x => x.Code.Id is (uint)LogId.WrongArchitecture);

        Assert.IsNotNull(log);
        Assert.AreEqual((uint)LogId.WrongArchitecture, log.Code.Id);
        Assert.AreEqual("RiscvModule", log.FilePath);
    }
}
