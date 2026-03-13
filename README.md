<p align="center">
  <img height="200" align="center" src="https://github.com/user-attachments/assets/fecd6c5f-0afd-4e37-a46a-5335dbc38b21"/>
</p>
<a href="https://apps.microsoft.com/detail/9ng185jd1rfd?referrer=appbadge&mode=full" target="_blank"  rel="noopener noreferrer">
	<img src="https://get.microsoft.com/images/en-us%20dark.svg" width="200"/>
</a>

# Introduction

**Zarem** (Zippy Assembly Resolver, Emulator, & Manager) is an assembler, linker, emulator, and IDE targeted at academic uses, with plans for advanced homebrewing in the future. It is very much a WIP, but the goal is to create the most accessible environment possible for learning to write assembly code.

# Assembly/Emulation Architectures

Zarem is currently MIPS only, but support for other architectures is planned.

| Architecture   | Assembling     |  Emulating       | Debugging     | Disassembly |
| -------------- | -------------- | --------------- | ------------- | ----------- |
| MIPS32         | ✅ Supported  | ✅ Supported    | ⚠️  WIP      | ⚠️  WIP     |
| MIPS64         | ⌛ Planned    | ⌛ Planned      | ⌛ Planned   | ⌛ Planned  |
| RISC-V         | ⌛ Planned    | ⌛ Planned      | ⌛ Planned   | ⌛ Planned  |
| ARM            | ⌛ Planned    | ⌛ Planned      | ⌛ Planned   | ⌛ Planned  |
| ARM64          | ⌛ Planned    | ⌛ Planned      | ⌛ Planned   | ⌛ Planned  |

# Cross-Platform Support

The Zarem IDE is a WinUI 3 project, and therefore only available on Windows. **However**, the Assembler and Emulator are made in .NET 10 with no windows-only dependendencies, and therefore available on any machine with the .NET runtime.

|         | Assembler      | Emulator        | CLI            | IDE                 |
| ------- | -------------- | --------------- | -------------- | ------------------- | 
| Windows | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ✅ Native WinUI     |
| MacOS   | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ⚠️ WIP Uno Platform |
| Linux   | ✅ Yes        | ✅ Yes          | ⚠️ WIP         | ⚠️ WIP Uno Platform |
| Wasm    | ⚠️ WIP        | ⚠️ WIP          | ❌ Not Planned | ⚠️ WIP Uno Platform |

# Localization

Zarem supports localization for both the IDE and the Assembler. Assembler localization is integrated directly into the assembler, and is therefore available in the console as-well-as the Zarem IDE app.

Current languages:

| Language | Provided    | Verified          |
| -------- | ----------- | ----------------- |
| English  | 🟦 100%    | ✅ 100%           |
| Hebrew   | [![he translation](https://img.shields.io/badge/dynamic/json?color=blue&label=he&style=flat&logo=crowdin&query=%24.progress.1.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![he proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=he&style=flat&logo=crowdin&query=%24.progress.1.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |
| Spanish  | [![es-ES translation](https://img.shields.io/badge/dynamic/json?color=blue&label=es-ES&style=flat&logo=crowdin&query=%24.progress.0.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![es-ES proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=es-ES&style=flat&logo=crowdin&query=%24.progress.0.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |
| Japanese | [![ja translation](https://img.shields.io/badge/dynamic/json?color=blue&label=ja&style=flat&logo=crowdin&query=%24.progress.2.data.translationProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) | [![ja proofreading](https://img.shields.io/badge/dynamic/json?color=green&label=ja&style=flat&logo=crowdin&query=%24.progress.2.data.approvalProgress&url=https%3A%2F%2Fbadges.awesome-crowdin.com%2Fstats-16364446-872226.json)](https://crowdin.com/project/zarem) |

[![Crowdin](https://badges.crowdin.net/zarem/localized.svg)](https://crowdin.com/project/zarem)

Help translate on [Crowdin](https://crowdin.com/project/zarem)!

# Screenshots

<img width="1439" height="831" alt="image" src="https://github.com/user-attachments/assets/8c4d4d9a-dca8-4d9f-b676-dbb5af670c4a" />

<img width="1473" height="787" alt="image" src="https://github.com/user-attachments/assets/f9b4d9a2-d259-4442-bdd5-89fd17f2b75c" />

<img width="1429" height="826" alt="image" src="https://github.com/user-attachments/assets/f76a33cb-2e92-4f62-8197-c782de23915b" />
