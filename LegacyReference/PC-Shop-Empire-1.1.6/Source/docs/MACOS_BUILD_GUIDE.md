# PC Shop Empire 1.1.6 — macOS Build and Distribution Guide

## Current status

PC Shop Empire is Electron-based and most of its source is shared across Windows and macOS. The bundle identifier `com.cixanla.pcshopempire`, macOS application category, native menu behavior, and full-screen shortcut are configured. Packaging commands are available for Intel (`x64`) and Apple Silicon (`arm64`).

A signed and Apple-notarized public macOS release cannot be completed on this Windows computer. It requires either:

- a physical Mac with macOS and Xcode Command Line Tools, or
- a trusted macOS CI runner.

An active Apple Developer Program membership, a Developer ID Application certificate, and notarization credentials are also required. The source is prepared for macOS; signing and notarization remain a separate release stage.

## Local validation on a Mac

Copy the project to a Mac and run:

```bash
npm ci
npm test
npm start
```

Build the required architecture:

```bash
npm run package:mac:arm64
# or
npm run package:mac:intel
```

The Forge configuration includes the macOS ZIP maker:

```bash
npm run make:mac
```

Cross-running these commands from Windows is not a substitute for an official macOS build, particularly when native binaries, code signing, and notarization are involved.

## Requirements for a public release

1. **macOS icon:** generate a correct `.icns` set from a high-resolution source and connect it to the Forge configuration.
2. **Code signing:** configure `osxSign` with an Apple Developer ID certificate.
3. **Notarization:** configure `osxNotarize` with Apple credentials or an App Store Connect API key.
4. **Hardened Runtime and entitlements:** enable only the permissions the application genuinely needs.
5. **Both architectures:** publish tested Intel and Apple Silicon builds, or establish a tested universal build process.
6. **Gatekeeper validation:** test the downloaded package in a clean macOS account and verify it with `spctl` and `codesign`.
7. **Game validation:** test saves, import/export, full screen, menu shortcuts, audio, and every new screen on both architectures.

Never commit Apple certificates, passwords, or private API keys. Store them as encrypted secrets when using CI.

## Recommended release sequence

1. Freeze and tag the Windows 1.1.6 source.
2. Check out the same tag on a macOS runner and run the tests.
3. Package arm64 and x64 applications.
4. Verify signing, notarization, and stapling.
5. Run end-user tests on clean Intel and Apple Silicon Macs.
6. Add SHA-256 checksums for the signed artifacts to the release record.

These steps require an Apple account and a genuine macOS environment and therefore are not automatically completed with the Windows release.
