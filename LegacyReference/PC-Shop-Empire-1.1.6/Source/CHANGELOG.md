# Changelog

## 1.1.6 — July 22, 2026

### Language and settings

- Fixed English and German selections reverting to Turkish after loading an older save
- Made the global language preference authoritative across every save slot
- Preserved the selected language during save import, new-game creation, autosave, and restart
- Rebuilt cached settings content when the language changes so every settings tab uses the selected locale
- Preserved unrelated display, audio, gameplay, and accessibility settings when saving

### Compatibility and distribution

- Kept save schema 3 and compatibility with existing 1.1.5 and earlier supported saves
- Updated application, package, installer, executable, documentation, and visible interface metadata to 1.1.6
- Regenerated the Windows installer, portable package, source archive, and SHA-256 manifest
- Retained English-only public file and folder names while keeping English, German, and Turkish as in-game languages

## 1.1.5 — July 22, 2026

### New systems

- Technical Service Center with standard and premium repairs, duration, success probability, and revenue tracking
- Corporate tenders with deadlines, compatibility requirements, risk, and rewards
- Brand & Market screen with campaigns, supplier relations, and time-limited supplier deals
- Business Intelligence with a transaction ledger, 90-day financial history, business health, risk, and forecasts
- Career screen with daily objectives, rewards, and achievements
- Dynamic market conditions: calm, surplus, shortage, gaming demand, business demand, and price wars
- Staff morale, fatigue, specialization, and stronger employee effects

### Gameplay and economy

- Increased overall market stock and processor availability
- Improved compatible component generation
- Added inventory resale for incompatible or unnecessary parts
- Added controlled emergency financing to prevent unrecoverable cash deadlocks
- Fixed accountant discounts incorrectly reducing loan principal payments
- Added campaign, market-cycle, competitor, morale, and fatigue processing to daily settlement
- Expanded operational incidents and incident-frequency settings

### Interface and visuals

- Rebuilt the main-menu presentation and general visual language
- Added four main screens for service, marketing, analytics, and career progression
- Improved layout behavior across window sizes and UI scale settings
- Added high contrast, large text, reduced motion, color profiles, and larger interaction targets
- Standardized visible product metadata as **1.1.5 / cixanla**

### Settings

- General: language, autosave, save interval, pause on focus loss, and notifications
- Display: full screen, UI scale, and quality level
- Audio: master volume, interface sounds, and mute controls
- Gameplay: difficulty, event frequency, and guidance hints
- Accessibility and control reference
- Data: manual save, JSON export/import, and settings reset
- About: version, publisher, and legal notices

### Saves and desktop integration

- Added three save slots
- Separated the visible application version from the save-schema version while preserving legacy compatibility
- Added full-screen startup and F11 full-screen switching
- Added Exit to the main menu
- Restricted unexpected navigation, external links, and new-window behavior
- Standardized Windows metadata and the macOS bundle identifier `com.cixanla.pcshopempire`

### Quality

- Added automated smoke tests and a 45-working-day simulation test
- Enabled ASAR integrity validation and hardened Electron fuse settings
- Excluded development/test material from the production ASAR
