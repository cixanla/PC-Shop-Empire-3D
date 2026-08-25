# ADR-0047 — Canonical M.2 NVMe Storage Physical Build-Kit Handoff

**Status:** Accepted; physical lifecycle pending<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #75, child of Epic #10

## Context

Issue #68 moved the exact reserved motherboard into the first custom-PC BuildKit slot, Issue #71 moved the exact reserved processor into the second slot, and Issue #73 moved the exact reserved DDR5 module into the third slot. Issue #75 continues the same accepted work order with the canonical reserved M.2 2280 NVMe storage device and advances the visible work ticket from `3/10` to `4/10` without inserting that SSD into the motherboard.

The storage device cannot be selected by line ordinal, display name, product-value equality or regenerated identity. It must remain the same serialized item and Unity object through source, carry, preview, placement, replay and recovery. The existing Issue #57 M.2 guided-insertion/captive-screw path is a separate Assembly authority and input target, so the BuildKit flow must also prove that one frame cannot be consumed by both BuildKit placement and motherboard M.2 seating.

## Decision

- Resolve the canonical SSD only by `ComponentKind.StorageDevice` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append `StorageInHands = 7` and `StorageStaged = 8` without renumbering persisted BuildKit stages. Use a distinct stable operation identity and a storage-specific managed capacity-one BuildKit container; motherboard, processor and memory operation, receipt, revision and staged custody remain unchanged.
- Permit storage custody only through exact source → `ActorHands` → storage BuildKit. Generic Inventory transfer, world drop, box, stack, cart and Assembly paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor and DDR5 memory staged prerequisites before storage pickup or placement. Work-ticket progress is derived from authoritative staged receipts and changes only from `3/10` to `4/10` after the storage commit.
- Commit domain custody before mutating world parent, pose, physics or visibility. A physical placement failure recovers the same NVMe instance at the authoritative BuildKit pose; no clone, ghost or lost item is permitted.
- Keep storage BuildKit and the Issue #57 M.2 slot/captive-screw projection as distinct authorities. While a storage BuildKit pickup receipt is active, primary, rotate, interact and drop edges belong to one BuildKit consumer; M.2 seating is available only outside that receipt.
- Preserve normal job-independent M.2 guided insertion and captive-screw retention when no BuildKit receipt is active. Issue #75 does not call or mutate M.2 Assembly state, revisions, receipts, keyed insertion or screw state.
- Use one raycastable non-trigger support collider and one exact snap anchor for the storage tray. Preview orientation is keyed to `0° ↔ 180°`; decorative rails, labels and progress text remain non-authoritative and `Ignore Raycast`.
- Bind native evidence to exact technical commit/tree, exact test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact success marker, one exact r38 readiness marker, zero forbidden markers and zero residue.

## Consequences

GarageGraybox r38 now lets the player take the exact reserved M.2 NVMe SSD with real keyboard/mouse or gamepad input, carry and rotate the same physical object by 180°, and place it into its separate BuildKit tray. The motherboard, processor and memory module remain staged, reservation/allocation identity stays live, the work ticket shows `4/10`, and M.2 Assembly insertion/retention state, revisions and receipts remain untouched.

The remaining six BuildKit component transfers, `10/10` completion, job-specific motherboard/CPU/DIMM/NVMe assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain separate dependent work.

## Verification

- Technical source commit `646e66cfa269a217ecb1f6942f9accb77f9e463c`, tree `ee9b0b2c0bb5e1fb07de397da222d00a7480b23c`.
- Unity 6000.3.21f1 full regression: EditMode `683/683`, PlayMode `90/90`; failed, skipped and inconclusive `0`. Focused storage BuildKit, gamepad, scene-contract and runtime-smoke gates also passed. Five new Unity GUIDs are unique and `git diff --check` passed.
- Universal macOS Development/StrictMode build report `329,735,698` bytes. The ad-hoc signed executable is a valid deep/strict universal `x86_64 + arm64` bundle. Apple M1/Metal r38 readiness and the exact storage BuildKit success marker each appeared once; forbidden markers and player residue were `0`.
- A complete verified Git bundle produced collision-free detached-clean Windows source at the same technical commit/tree. Unity 6000.3.21f1 completed an x64 IL2CPP build with Direct3D11 only; report size was `1,332,182,927` bytes and the hardened-v2 Burst/native-link fatal-token count was `0`. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive runtime emitted the exact host, r38 readiness and success markers once, exited `0`, shut down gracefully, deleted its scheduled task and left process residue `0`.
- Thirteen immutable test/build/runtime/procedure artifacts returned to the Mac with exact size/hash readback. The current Windows build receipt is not final checkpoint provenance; final `source-receipt.json` is created only after the exact nine-file source/docs commit and its Repository Guard succeed.
- `Tools/verify-checkpoint-package.sh ... issue75` fail-closes on the exact technical commit/tree, exact nine-file closure delta, `683/683` and `90/90`, `issue75-hardened-v2`, the exact r38 storage marker, 13 promoted artifacts, three procedures, task deletion and residue `0`.

Repository Guard, final source receipt, immutable local package, correct physical USB incoming/final double readback, lifecycle metadata, acceptance `23/23`, Issue closure and Roadmap `Done` remain pending and must be recorded separately. No physical-completion claim is made by this ADR revision.
