# PC Shop Empire 1.1.6 — Release Notes

## Release identity

- Product: PC Shop Empire
- Version: 1.1.6
- Publisher / brand: cixanla
- Application identifier: `com.cixanla.pcshopempire`
- Save schema: 3 (maintained separately from the visible application version)

## Delivered in this release

1. Persistent English, German, and Turkish selection across save slots, imports, and restarts
2. Rebuilt main menu, general interface, and 1.1.6 visual identity
3. Three save slots and legacy save compatibility
4. Full-screen startup, F11 support, and main-menu Exit
5. Market stock/compatibility improvements and component resale
6. Staff morale, fatigue, specialization, and stronger staff effects
7. Detailed operating expenses, emergency financing, and corrected loan accounting
8. Operational incidents, configurable event frequency, and dynamic market conditions
9. Technical service, corporate tenders, marketing, and supplier systems
10. Business Intelligence, financial history, forecasting, and risk analysis
11. Daily objectives, achievements, and career progression
12. General, display, audio, gameplay, accessibility, controls, and data settings
13. Windows packaging metadata and macOS source preparation

## Validation

Run these checks from the source delivery:

```powershell
npm.cmd test
npm.cmd run package
npm.cmd run make
```

`npm.cmd test` runs both the smoke suite and a 45-working-day simulation. After packaging, manually validate at least:

- Main menu, save selection, and legacy save loading
- Settings tabs, language persistence, full screen, and audio controls
- Market, inventory resale, and compatible system assembly
- Technical service, corporate tender, and marketing screens
- Business Intelligence and Career screens
- Saving, export, import, restart, and recovery behavior

## Installation and migration

End users do not need to merge source files manually. Recommended process:

1. Close the game.
2. Back up `%APPDATA%\PC Shop Empire`.
3. Run the 1.1.6 Windows installer generated under `out\make`.
4. Open the **PC Shop Empire 1.1.6** shortcut.
5. Load the existing slot and verify money, day, inventory, and staff data.

The 1.1.6 compatibility layer recognizes the earlier save schema. Starting a new career is not required. If developing by manual merge, use the complete core files together with `src/`, `styles/`, and the current `assets/` content. Copying only `game.js` into an older folder will produce missing screens and inconsistent save behavior.

## Known distribution limits

- Windows will not show a verified publisher until the executable and installer are signed with a trusted Authenticode certificate.
- macOS source and packaging commands are prepared, but a signed/notarized release requires a Mac, Xcode, and an Apple Developer account.
- Content Credentials / C2PA provenance data embedded in visual assets must be preserved.
- Third-party components are not part of the cixanla proprietary license.
- Legal registration and store requirements must be completed for each target country and platform.

## Delivery documents

- `README.md`: project, technology, and usage information
- `CHANGELOG.md`: version 1.1.6 change history
- `GAME_LICENSE.txt`: proprietary license
- `THIRD_PARTY_NOTICES.txt`: third-party notices
- `docs/OWNERSHIP_AND_REGISTRATION.md`: ownership, registration, and trademark guidance
- `docs/MACOS_BUILD_GUIDE.md`: macOS packaging and distribution guide
- This file: installation and release-delivery summary

Copyright © 2026 cixanla. All rights reserved, except third-party materials governed by their own licenses.
