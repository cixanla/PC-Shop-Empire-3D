# ADR-0048 — Canonical Processor Cooler Physical Build-Kit Handoff

**Status:** Accepted; physical lifecycle pending<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #77, child of Epic #10

## Context

Issues #68, #71, #73 and #75 moved the exact reserved motherboard, processor, DDR5 module and M.2 NVMe device into the first four custom-PC BuildKit slots. Issue #77 continues the same accepted work order with the canonical reserved processor cooler and advances the visible work ticket from `4/10` to `5/10` without mounting the cooler or applying thermal interface material.

The cooler cannot be selected by line ordinal, display name, product-value equality or regenerated identity. It must remain the same serialized item and Unity object through source, carry, preview, placement, replay and recovery. The existing Issue #58 processor-cooler four-point retention and TIM path is a separate Assembly authority and input target, so the BuildKit flow must prove that one frame cannot be consumed by both BuildKit placement and cooler mounting.

## Decision

- Resolve the canonical cooler only by `ComponentKind.ProcessorCooler` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append `ProcessorCoolerInHands = 9` and `ProcessorCoolerStaged = 10` without renumbering persisted BuildKit stages. Use a distinct stable operation identity and a processor-cooler-specific managed capacity-one BuildKit container; motherboard, processor, memory and storage operation, receipt, revision and staged custody remain unchanged.
- Permit cooler custody only through exact source → `ActorHands` → processor-cooler BuildKit. Generic Inventory transfer, world drop, box, stack, cart and Assembly paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory and M.2 storage staged prerequisites before cooler pickup or placement. Work-ticket progress is derived from authoritative staged receipts and changes only from `4/10` to `5/10` after the cooler commit.
- Commit domain custody before mutating world parent, pose, physics or visibility. A physical placement failure recovers the same cooler instance at the authoritative BuildKit pose; no clone, ghost or lost item is permitted.
- Keep processor-cooler BuildKit and Issue #58 cooler-seat/four-point-retention/TIM projection as distinct authorities. While a cooler BuildKit pickup receipt is active, primary, rotate, interact and drop edges belong to one BuildKit consumer; cooler mounting is available only outside that receipt.
- Preserve normal job-independent processor-cooler seating, four-point retention and TIM behavior when no BuildKit receipt is active. Issue #77 does not call or mutate cooler Assembly state, revisions, receipts, retention sequence or TIM state.
- Use one raycastable non-trigger support collider and one exact snap anchor for the cooler tray. Preview orientation advances in keyed `90°` quarter turns through four deterministic poses; decorative rails, labels and progress text remain non-authoritative and `Ignore Raycast`.
- Bind native evidence to exact technical commit/tree, exact test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact success marker, one exact r39 readiness marker, zero forbidden markers and zero residue.

## Consequences

GarageGraybox r39 now lets the player take the exact reserved processor cooler with real keyboard/mouse or gamepad input, carry and rotate the same physical object in keyed 90° steps, and place it into its separate BuildKit tray. The motherboard, processor, memory module and storage device remain staged, reservation/allocation identity stays live, the work ticket shows `5/10`, and cooler Assembly retention/TIM state, revisions and receipts remain untouched.

The remaining five BuildKit component transfers—GPU, PSU, ATX24 cable, EPS12V cable and PCIe/GPU cable—plus `10/10` completion, job-specific assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain separate dependent work.

## Verification

- Technical source commit `197233688c4fe587097dbfc1cbee843cfc78603e`, tree `58458f400a7efaa68e452a0e85e35d6d7eb5a3ab`.
- Unity 6000.3.21f1 full regression: EditMode `686/686`, PlayMode `96/96`; failed, skipped and inconclusive `0`. Focused processor-cooler BuildKit, preview/gamepad, scene-contract and runtime-smoke gates also passed, and `git diff --check` passed.
- Universal macOS Development/StrictMode build report `329,787,583` bytes. The ad-hoc signed executable is a valid deep/strict universal `x86_64 + arm64` bundle. Apple M1/Metal r39 readiness and the exact processor-cooler BuildKit success marker each appeared once; forbidden markers and player residue were `0`.
- A complete verified Git bundle produced collision-free detached-clean Windows source at the same technical commit/tree. Unity 6000.3.21f1 completed an x64 IL2CPP build with Direct3D11 only; report size was `1,333,221,634` bytes and the `issue77-hardened-v2` Burst/native-link fatal-token count was `0`. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive runtime emitted the exact host, r39 readiness and success markers once, exited `0`, shut down gracefully, deleted its scheduled task and left process residue `0`.
- Thirteen immutable test/build/runtime/procedure artifacts returned to the Mac with exact size/hash readback. The current Windows build receipt is not final checkpoint provenance; final `source-receipt.json` is created only after the exact nine-file source/docs commit and its Repository Guard succeed.
- `Tools/verify-checkpoint-package.sh ... issue77` fail-closes on the exact technical commit/tree, exact nine-file closure delta, `686/686` and `96/96`, `issue77-hardened-v2`, the exact r39 processor-cooler marker, 13 promoted artifacts, three procedures, task deletion and residue `0`.

Repository Guard, final source receipt, immutable local package, correct physical USB incoming/final double readback, lifecycle metadata, acceptance `24/24`, Issue closure and Roadmap `Done` remain pending and must be recorded separately. No physical-completion claim is made by this ADR revision.
