# ADR-0045 — Canonical Processor Physical Build-Kit Handoff

**Status:** Accepted technically on macOS and Windows; immutable package, physical USB and lifecycle closure pending<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #71, child of Epic #10

## Context

Issue #68 established the first physical custom-PC BuildKit handoff by moving the exact reserved motherboard from its authoritative source through the player's hands into a dedicated capacity-one slot. Issue #71 extends that same accepted work order with the canonical reserved processor and advances the visible work ticket from `1/10` to `2/10` without starting CPU socket assembly.

The processor cannot be selected by line ordinal, display name, product-value equality or regenerated identity. It must remain the same serialized item and Unity object across source, carry, preview, placement, replay and recovery. The existing `ProcessorSocket` is a separate authority and input target, so the CPU BuildKit path must also prove that one frame cannot be consumed by both the kit and socket flows.

## Decision

- Resolve the canonical CPU only by `ComponentKind.Processor` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Use an append-only processor operation identity, immutable pickup/place receipts and a processor-specific managed capacity-one BuildKit container. Motherboard operation, receipt, revision and staged custody remain unchanged.
- Permit CPU custody only through exact source → `ActorHands` → processor BuildKit. Generic inventory transfer, world drop, box, stack, cart and Assembly socket paths cannot bypass an active BuildKit receipt.
- Require the motherboard prerequisite before CPU pickup or placement. Work-ticket progress is derived from authoritative staged receipts and changes only from `1/10` to `2/10` after the CPU commit.
- Commit domain custody before mutating world parent, pose, physics or visibility. A physical placement failure recovers the same CPU instance at the authoritative BuildKit pose; no clone, ghost or lost item is permitted.
- Keep processor BuildKit and `ProcessorSocket` as distinct projections and authorities. While a CPU BuildKit pickup receipt is active, primary, rotate, interact and drop edges belong to one BuildKit consumer regardless of legacy socket focus; socket input is available only outside that receipt.
- Preserve normal job-independent processor socket assembly when no BuildKit receipt is active. Issue #71 does not change keyed insertion, retention or Assembly receipts.
- Use one raycastable non-trigger support collider and one exact snap anchor for the CPU tray. Decorative rails, labels and progress text remain `Ignore Raycast` and non-authoritative.
- Bind native evidence to exact technical commit/tree, exact test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact success marker, zero forbidden markers and zero residue.

## Consequences

GarageGraybox r36 now lets the player take the exact reserved CPU with real keyboard/mouse or gamepad input, carry and rotate the same physical object, and place it into its separate BuildKit tray. The motherboard remains staged, the reservation/allocation remains live, the work ticket shows `2/10`, and ProcessorSocket/Assembly state, revisions and receipts remain untouched.

The remaining eight BuildKit component transfers, `10/10` completion, job-specific motherboard/CPU assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain separate dependent work.

## Verification

- Technical source commit `11683c8b567ad6edcd6777610875aeebd0e509ef`, tree `6890157f3f3625661314b34700259e0933ff2677`; [Repository Guard 32827174483](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32827174483) passed.
- Unity 6000.3.21f1 full regression on the exact source: EditMode `677/677`, PlayMode `81/81`; failed, skipped and inconclusive `0`. Post-test generated editor-setting traces were removed and the repository returned to exact clean HEAD with `git diff --check` passing.
- Universal macOS Development/StrictMode build report `329,627,927` bytes. The ad-hoc signed executable is a valid deep/strict universal `x86_64 + arm64` bundle. Apple M1/Metal r36 readiness and the exact CPU BuildKit success marker each appeared once; failure markers were `0`.
- Exact detached-clean Windows `hardened-v2` source produced a `1,329,802,474`-byte x64 IL2CPP/Direct3D11 build report and restored `ProjectSettings.asset` byte-exactly. Its expanded fatal policy explicitly rejects Burst internal compiler, `AotLinkerException`, native-link, `Win32 IO returned 232` and `burst-lld` failures; accepted build fatal count is `0`. Native binary and procedure hashes passed complete readback. The earlier recovered-import evidence is retained separately as provisional history and is not canonical acceptance evidence.
- The logged-on interactive Windows player used Intel Iris Xe / Direct3D11. Player exit was `0`, shutdown was graceful, readiness and exact success markers were each `1`, forbidden markers were `0`, the temporary scheduled task was deleted, cleanup was unnecessary and process residue was `0`.
- Canonical technical evidence is the procedure-bound exact `14/14` set at `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue71-11683c8b567a/canonical-evidence`. Detailed hashes, the 22-row acceptance mapping and the still-pending physical lifecycle are recorded in `Docs/Evidence/CANONICAL-PROCESSOR-PHYSICAL-BUILD-KIT-HANDOFF-CHECKPOINT-2026-08-25.md`.
