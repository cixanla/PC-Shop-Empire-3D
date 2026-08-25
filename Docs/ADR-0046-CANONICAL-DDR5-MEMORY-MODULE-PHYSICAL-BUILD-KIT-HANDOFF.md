# ADR-0046 — Canonical DDR5 Memory-Module Physical Build-Kit Handoff

**Status:** Accepted for technical source and macOS native gates; Windows/CI/USB lifecycle closure pending<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #73, child of Epic #10

## Context

Issue #68 moved the exact reserved motherboard into the first custom-PC BuildKit slot and Issue #71 moved the exact reserved processor into the second slot. Issue #73 continues that accepted work order with the canonical reserved DDR5 UDIMM and advances the visible work ticket from `2/10` to `3/10` without inserting the DIMM into the motherboard.

The memory module cannot be selected by line ordinal, display name, product-value equality or regenerated identity. It must remain the same serialized item and Unity object through source, carry, preview, placement, replay and recovery. The existing Issue #56 A2 dual-latch path is a separate Assembly authority and input target, so the BuildKit flow must also prove that one frame cannot be consumed by both BuildKit placement and A2 seating.

## Decision

- Resolve the canonical DIMM only by `ComponentKind.MemoryModule` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append `MemoryModuleInHands = 5` and `MemoryModuleStaged = 6` without renumbering persisted BuildKit stages. Use a distinct stable operation identity and a memory-specific managed capacity-one BuildKit container; motherboard and processor operation, receipt, revision and staged custody remain unchanged.
- Permit memory custody only through exact source → `ActorHands` → memory-module BuildKit. Generic Inventory transfer, world drop, box, stack, cart and Assembly paths cannot bypass an active BuildKit receipt.
- Require both motherboard and processor staged prerequisites before memory pickup or placement. Work-ticket progress is derived from authoritative staged receipts and changes only from `2/10` to `3/10` after the memory commit.
- Commit domain custody before mutating world parent, pose, physics or visibility. A physical placement failure recovers the same DIMM instance at the authoritative BuildKit pose; no clone, ghost or lost item is permitted.
- Keep memory-module BuildKit and the Issue #56 A2/dual-latch projection as distinct authorities. While a memory BuildKit pickup receipt is active, primary, rotate, interact and drop edges belong to one BuildKit consumer; A2 input is available only outside that receipt.
- Preserve normal job-independent A2 DIMM seating/retention when no BuildKit receipt is active. Issue #73 does not call or mutate A2 Assembly state, revisions, receipts, keyed insertion or latch state.
- Use one raycastable non-trigger support collider and one exact snap anchor for the memory tray. Preview orientation is keyed to `0° ↔ 180°`; decorative rails, labels and progress text remain non-authoritative and `Ignore Raycast`.
- Bind native evidence to exact technical commit/tree, exact test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact success marker, one exact r37 readiness marker, zero forbidden markers and zero residue.

## Consequences

GarageGraybox r37 now lets the player take the exact reserved DDR5 DIMM with real keyboard/mouse or gamepad input, carry and rotate the same physical object by 180°, and place it into its separate BuildKit tray. The motherboard and processor remain staged, reservation/allocation identity stays live, the work ticket shows `3/10`, and A2/dual-latch Assembly state, revisions and receipts remain untouched.

The remaining seven BuildKit component transfers, `10/10` completion, job-specific motherboard/CPU/DIMM assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain separate dependent work.

## Verification

- Technical source commit `a2df663d6fa0e9d2004697bfb038a65a5e6c3d81`, tree `e32a8e143049c4059e402bafbfcd39b9760cd025`.
- Unity 6000.3.21f1 full regression: EditMode `680/680`, PlayMode `86/86`; failed, skipped and inconclusive `0`. Targeted BuildKit, gamepad, scene-contract and runtime-smoke compile gates also passed. Generated ProBuilder/scene-template editor preferences were removed before the commit and `git diff --check` passed.
- Universal macOS Development/StrictMode build report `329,681,642` bytes. The ad-hoc signed executable is a valid deep/strict universal `x86_64 + arm64` bundle. Apple M1/Metal r37 readiness and the exact memory-module BuildKit success marker each appeared once; forbidden markers and player residue were `0`.
- Exact-source Windows x64 IL2CPP/Direct3D11 build/runtime, source/docs CI, immutable local/USB package and Issue/Project acceptance are separate pending lifecycle gates. They are not implied by the successful Mac technical evidence.
