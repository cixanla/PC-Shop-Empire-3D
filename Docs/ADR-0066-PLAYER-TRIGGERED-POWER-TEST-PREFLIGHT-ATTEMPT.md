# ADR-0066 — Player-Triggered Power-Test Preflight Attempt

**Status:** Accepted for Mac technical source `3c26ce0`, tree `58dd983`; physical Windows and USB gates are deferred; draft PR #124 is the integration record<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #123, dependent on Issue #119 exact electrical readiness and Issue #121 exact power budget

## Context

Issue #119 proves exact retained component and routed ATX24/EPS12V/PCIe-GPU lineage. Issue #121 recomputes a deterministic `380 W / 500 W / 550 W` power-budget result from that exact state. Neither issue records that the player intentionally requested a power-test preflight, and neither is allowed to imply that the PC was energized.

The next bounded product step needs one player-triggered command and one immutable historical receipt. It must remain separate from Assembly, cable, inventory, reservation, custody, benchmark and future power-on authorities. A historical receipt must remain replayable after the physical configuration changes, while the current presentation must detect that the old receipt no longer describes the current configuration.

## Decision

- Add one `PowerTestAttemptAuthority` reference-bound to the exact `PcPowerBudgetAuthority` and `AssemblyBuildAuthority` instances. Null, foreign or mismatched authority configuration fails closed.
- Capture one immutable `PowerTestAttemptContext` from the current power-budget snapshot. The context binds exact build/chassis, seven installed component items, three power-cable items, seven retain/secure operations, three route operations, Assembly and cable revisions, electrical product IDs, policy ID, all load values, `380 W` draw, `500 W` recommendation, installed `550 W` PSU and `+50 W` margin.
- Require a stable non-empty operation ID, expected attempt revision and caller-observed exact context. The authority recomputes the current budget before first acceptance; cached UI state and caller-supplied success are never authority.
- Publish exactly one `PreflightReady` receipt in this bounded slice. A same-operation exact command replay returns the same receipt instance without mutation, even after the current physical context becomes stale. Reusing that operation with different command lineage is `OperationConflict`; a second distinct completion is `AlreadyCompleted`.
- Keep historical replay and current validity separate. `EvaluateCurrentReceipt()` recomputes current exact context and returns `ContextStale` when cable or Assembly lineage changes. It never rewrites or invalidates the stored historical receipt.
- Include the attempt authority in the session invariant chain only after it has been created. Invariant validation does not instantiate optional state as a side effect.
- Reuse the existing Assembly Workbench focus anchor, status text and indicator. Add no gameplay collider, renderer, light, camera, NavMesh obstacle, physical item or second Assembly authority.
- Route normal keyboard/mouse `Interact` and Input System gamepad South through the same command. Reject paused or pause-co-edge frames, out-of-range, missing focus, occlusion, busy hands, cart/Assembly ownership, a competing world Interact owner and same-frame replay. Consume Interact only after every gate passes.
- Use a fixed-capacity `Physics.RaycastNonAlloc` line-of-sight buffer. Ignore only the player-root hierarchy; any other hit blocks, and buffer saturation fails closed. Cache prompt construction for the current frame so repeated HUD reads reuse the same observed string while the command still performs a fresh authoritative read.
- Present `GÜÇ TESTİ ÖN KONTROLÜNÜ ÇALIŞTIR` before acceptance, `ÖN KONTROL GEÇTİ • POWER-ON BEKLİYOR` for a current receipt and `ÖN KONTROL GEÇERSİZ` after lineage drift. The result never claims electrical energization.
- Keep power-on, connector pinout/polarity, short-circuit/rail/transient simulation, electrical fault/damage, POST, BIOS/UEFI, OS, driver, benchmark, packaging, delivery and settlement outside Issue #123.
- Require targeted/full Mac tests, a universal native build and an Apple M1/Metal runtime smoke now. Keep clean exact-commit physical Windows x64 IL2CPP/only-D3D11/Intel Iris Xe validation mandatory before closure; UTM is not equivalent evidence.

## Consequences

GarageGraybox r60 now lets a correctly positioned player with empty hands focus the existing Workbench and request one real preflight using the normal Interact action. Acceptance creates a durable fact about that exact configuration and leaves every gameplay authority unchanged. The status remains explicit that power-on has not started.

The replay/current split prevents two opposite errors: historical evidence is not silently destroyed when a cable moves, and stale historical success is not presented as current readiness. Future energization can therefore require a currently valid preflight receipt without mutating or overloading the receipt defined here.

The interaction adds no heap allocation for line-of-sight checks and avoids repeated same-frame prompt reconstruction. It also remains fail-closed when the non-alloc hit buffer saturates or when another world interaction owns the frame.

## Current verification

- Technical source `3c26ce0d6de80c975b064f2dff68d96fbd4378bc`, tree `58dd983e314ecb78d94b3871dc672641e0a87b5d`; draft PR #124; Issue/Roadmap remain open and In Progress.
- Repository Guard run `33357285973` passed on the source commit.
- A bounded independent review found one P1 historical-replay ordering defect and two P2 gaps: missing session-invariant participation and repeated same-frame HUD allocation. All three were corrected before the final verification chain; the rerun found no remaining P0, P1 or P2 issue.
- Targeted domain/scene tests pass `6/6`; targeted keyboard/mouse, virtual-gamepad and presentation tests pass `3/3`.
- Unity 6000.3.21f1 full Mac EditMode `773/773` and PlayMode `161/161`; failed, skipped and inconclusive `0`.
- Universal macOS Development build reports `330,507,808` bytes across `302` files. The executable is deep/strict-valid universal Mach-O `x86_64 + arm64`.
- Apple M1/Metal native smoke emits the exact r60 success marker for keyboard+gamepad, single-consumer, range/focus/LOS/pause/co-edge, immutable receipt, replay, stale detection, zero gameplay mutation, untouched benchmark, presentation and invariants. It exits `0`, shuts Input System down cleanly and leaves no player residue.
- The separately preserved ProBuilder user-setting remains SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b`, unstaged and outside the Issue #123 commit. No other ProjectSettings or Packages path differs from source.
- Physical Windows x64 IL2CPP/D3D11/Iris Xe, physical-human HID/endurance and USB checkpoint/readback remain deferred and are not claimed.
