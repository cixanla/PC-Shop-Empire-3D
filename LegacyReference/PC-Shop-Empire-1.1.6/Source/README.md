# PC Shop Empire

PC Shop Empire is a desktop management simulation about building and operating a computer store and technical service business.

- Version: **1.1.6**
- Publisher and brand: **cixanla**
- Platform: Windows, with cross-platform source preparation for macOS
- License: Proprietary; see `GAME_LICENSE.txt`

## Technology

The game is built with:

- **JavaScript:** gameplay, economy, save system, and Electron desktop integration
- **HTML:** screens and interface structure
- **CSS:** visual design, animation, accessibility, and responsive layout
- **Node.js + Electron:** desktop runtime and Windows/macOS packaging

The project does not use TypeScript, C#, C++, Java, Python, Unity, Unreal Engine, or another game engine. Electron provides the Chromium and Node.js runtime components.

## Version 1.1.6 highlights

- Persistent English, German, and Turkish language selection across save loading, importing, and restarts
- Three independent save slots with legacy save migration
- Full-screen startup, F11 support, and a safe Exit option on the main menu
- Compatible component offers, increased market stock, and inventory resale
- Emergency financing, detailed operating expenses, and stronger staff effects
- Power failures, theft, repairs, and other operational incidents
- Technical service center, corporate tenders, and repair work
- Marketing campaigns, supplier relations, and market cycles
- Business Intelligence with financial history, forecasts, and risk indicators
- Daily objectives, achievements, and career progression
- General, display, audio, gameplay, accessibility, controls, and data settings

See `CHANGELOG.md` and `docs/RELEASE_NOTES_1.1.6.md` for the complete release details.

## Development

Requirements: a currently supported Node.js release and npm.

```powershell
npm.cmd ci
npm.cmd test
npm.cmd start
```

Build the Windows application and installer:

```powershell
npm.cmd run package
npm.cmd run make
```

Generated files are written to `out`. The `node_modules` and `out` directories are generated dependencies/build outputs and are intentionally excluded from the source archive.

## Updates and save data

1. Close the game.
2. Back up `%APPDATA%\PC Shop Empire`.
3. Run the 1.1.6 installer or open the packaged application.
4. Load the existing career from the save-slot menu.

Version 1.1.6 includes a compatibility layer for earlier save schemas, so starting a new game should not normally be required. Always keep a backup before changing versions.

## Project structure

- `index.html`, `styles.css`, `game.js`: core game and interface
- `src/`: 1.1.6 systems, settings, and integration layers
- `styles/`: 1.1.6 visual layer
- `assets/`: icons and visual assets
- `tests/`: smoke and long-running simulation tests
- `main.js`, `preload.js`: secured Electron desktop layer
- `forge.config.js`: Windows/macOS packaging configuration
- `docs/`: release, ownership, and platform documentation

## Rights and third-party components

Copyright © 2026 cixanla. All rights reserved.

This notice applies only to original project materials for which cixanla lawfully owns or controls the relevant rights. Electron, Chromium, Node.js, and other third-party components remain subject to their own licenses. See `GAME_LICENSE.txt`, `THIRD_PARTY_NOTICES.txt`, and `docs/OWNERSHIP_AND_REGISTRATION.md`.
