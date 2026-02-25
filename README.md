<p align="center">
  <img height="200" align="center" src="https://github.com/user-attachments/assets/fecd6c5f-0afd-4e37-a46a-5335dbc38b21"/>
</p>
<a href="https://get.microsoft.com/installer/download/9ng185jd1rfd?referrer=appbadge" target="_self">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200" align="right"/>
</a>

# Introduction

**Zarem** (Zippy Assembly Resolver, Emulator, & Manager) is a assembler, linker, emulator, and IDE targeted at academic uses. It is very much a WIP, but the goal is to create the most accessible environment possible for learning to write assembly code.

# Assembly/Emulation Architectures

Zarem is currently MIPS only, but support for other architectures is planned.

| Architecture   | Assembling     | Linking      | Emulating       | Disassembly   |
| -------------- | -------------- | ------------ | --------------- | ------------- |
| MIPS32         | ✅ Supported  | ✅ Supported | ✅ Supported    | ⚠️  WIP      |
| MIPS64         | ⌛ Planned    | ⌛ Planned   | ⌛ Planned      | ⌛ Planned   |
| RISC-V         | ⌛ Planned    | ⌛ Planned   | ⌛ Planned      | ⌛ Planned   |
| ARM            | ⌛ Planned    | ⌛ Planned   | ⌛ Planned      | ⌛ Planned   |
| ARM64          | ⌛ Planned    | ⌛ Planned   | ⌛ Planned      | ⌛ Planned   |

# Cross-Platform Support

The Zarem IDE is a WinUI 3 project, and therefore only available on Windows. **However**, the Assembler and Emulator are made in .NET 10 with no windows-only dependendencies, and therefore available on any machine with the .NET runtime.

|         | Assembler      | Emulator        | CLI          | IDE                 |
| ------- | -------------- | --------------- | ------------ | ------------------- | 
| Windows | ✅ Yes        | ✅ Yes          | ✅ Yes      | ✅ Native WinUI     |
| MacOS   | ✅ Yes        | ✅ Yes          | ✅ Yes      | ⚠️ WIP Uno Platform |
| Linux   | ✅ Yes        | ✅ Yes          | ✅ Yes      | ⚠️ WIP Uno Platform |
| Wasm    | ⚠️ WIP        | ⚠️ WIP          | ⚠️ WIP      | ⚠️ WIP Uno Platform |

# Translation

Zarem supports localization for both the IDE and the Assembler. Assembler localization is integrated directly into the assembler, and is therefore available in the console as-well-as the Zarem IDE app.

Current languages:

| Language | IDE Support | Assembler Support |
| -------- | ----------- | ----------------- |
| English  | ✅ Yes     | ✅ Yes            |
| Hebrew   | ✅ Yes     | ✅ Yes            |
| Spanish  | ⌛ Planned | ⌛ Planned        |
| Japanese | ⌛ Planned | ⌛ Planned        |

[![Crowdin](https://badges.crowdin.net/zarem/localized.svg)](https://crowdin.com/project/zarem)

Help translate on [Crowdin](https://crowdin.com/project/zarem)!

# Screenshots

<img width="1439" height="831" alt="image" src="https://github.com/user-attachments/assets/8c4d4d9a-dca8-4d9f-b676-dbb5af670c4a" />

<img width="1473" height="787" alt="image" src="https://github.com/user-attachments/assets/f9b4d9a2-d259-4442-bdd5-89fd17f2b75c" />

<img width="1429" height="826" alt="image" src="https://github.com/user-attachments/assets/f76a33cb-2e92-4f62-8197-c782de23915b" />
