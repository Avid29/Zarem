<p align="center">
  <img height="200" align="center" src="https://github.com/user-attachments/assets/fecd6c5f-0afd-4e37-a46a-5335dbc38b21"/>
</p>
<a href="https://apps.microsoft.com/detail/9ng185jd1rfd?referrer=appbadge&mode=full" target="_blank"  rel="noopener noreferrer">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

# Introduction

**Zarem** (Zippy Assembly Resolver, Emulator, & Manager) is an assembler, linker, emulator, and IDE targeted at academic uses, with plans for advanced homebrewing in the future. It is very much a WIP, but the goal is to create the most accessible environment possible for learning to write assembly code.

# Pre-release Notice

This project is in pre-release stages of development. Changes to Project File formats, emulator behavior, and plug-in APIs should be expected. **If you have any questions or run into any issues, please open a new Q&A discussion and I will get back to you ASAP.**

# Assembly/Emulation Architectures

| Architecture   | Assembling      | Emulation (Interpret) | Emulation (JIT) | Debugging      | Disassembly     | Static Analysis |
| -------------- | --------------- | --------------------- | --------------- | -------------- | --------------- | --------------- |
| MIPS*          | ✅ Supported    | ✅ Supported         | ⚠️ WIP          | ✅ Supported   | ⚠️ WIP         | ⌛ Planned      |
| RISC-V         | ✅ Supported    | ✅ Supported         | ⚠️ WIP          | ✅ Supported   | ⌛ Planned     | ⌛ Planned      |
| ARM            | ⌛ Planned      | ⌛ Planned           | ⌛ Planned      | ⌛ Planned     | ⌛ Planned     | ⌛ Planned      |
| ARM64          | ⌛ Planned      | ⌛ Planned           | ⌛ Planned      | ⌛ Planned     | ⌛ Planned     | ⌛ Planned      |
| x86_64         | ❌ Not Planned  | ❌ Not Planned       | ❌ Not Planned  | ❌ Not Planned | ❌ Not Planned | ❌ Not Planned  |

\* *MIPS32/64 Release 6 is considerably different from every other version of mips and is not currently supported*.

# Cross-Platform Support

The Zarem IDE is a WinUI 3 project, and therefore only available on Windows. **However**, the Assembler and Emulator are made in .NET 10 with no windows-only dependendencies, and therefore available on any machine with the .NET runtime.

|         | Assembler      | Emulator        | CLI            | IDE                  | Cross Assembling |
| ------- | -------------- | --------------- | -------------- | -------------------- | ---------------- |
| Windows | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ✅ Native WinUI     | ⌛ Planned      |
| MacOS   | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ⚠️ WIP Uno Platform | ⌛ Planned      |
| Linux   | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ⚠️ WIP Uno Platform | ⌛ Planned      |
| Wasm    | ⚠️ WIP        | ⚠️ WIP          | ❌ Not Planned | ⚠️ WIP Uno Platform | ⌛ Planned      |

# Localization

Zarem supports localization for both the IDE and the Assembler. Assembler localization is integrated directly into the assembler, and is therefore available in the console as-well-as the Zarem IDE app.

### Current languages:

| Language | Provided    | Verified          |
| -------- | ----------- | ----------------- |
| English  | 🟦 100%    | ✅ 100%           |
| Hebrew   | [![he translation](https://img.shields.io/badge/dynamic/json?color=blue&label=he&style=flat&logo=crowdin&query=%24.progress.1.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![he proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=he&style=flat&logo=crowdin&query=%24.progress.1.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |
| Spanish  | [![es-ES translation](https://img.shields.io/badge/dynamic/json?color=blue&label=es-ES&style=flat&logo=crowdin&query=%24.progress.0.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![es-ES proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=es-ES&style=flat&logo=crowdin&query=%24.progress.0.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |
| Japanese | [![ja translation](https://img.shields.io/badge/dynamic/json?color=blue&label=ja&style=flat&logo=crowdin&query=%24.progress.2.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![ja proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=ja&style=flat&logo=crowdin&query=%24.progress.2.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |

[![Crowdin](https://badges.crowdin.net/zarem/localized.svg)](https://crowdin.com/project/zarem)

Help translate on [Crowdin](https://crowdin.com/project/zarem)!

# Screenshots

<img width="1467" height="845" alt="Zarem-Running" src="https://github.com/user-attachments/assets/2ba34911-95f9-4762-a314-08cc88a8ec2a" />

<img width="1468" height="833" alt="Zarem-Settings" src="https://github.com/user-attachments/assets/82c0dde9-9a45-4464-a2a6-66a0d153d962" />
