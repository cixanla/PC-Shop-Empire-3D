# ADR-0049 — Canonical Graphics Card Physical Build-Kit Handoff

**Status:** Accepted and lifecycle-complete<br>
**Date:** 25 August 2026<br>
**Scope:** Issue #79, child of Epic #10

## Context

Issues #68, #71, #73, #75 and #77 moved the exact reserved motherboard, processor, DDR5 module, M.2 NVMe device and processor cooler into the first five custom-PC BuildKit slots. Issue #79 continues the same accepted work order with the canonical reserved graphics card and advances the visible work ticket from `5/10` to `6/10` without seating the GPU in PCIe x16, retaining its latch/rear bracket or routing its PCIe power cable.

The GPU cannot be selected by line ordinal, display name, product-value equality or regenerated identity. It must remain the same serialized item and Unity object through source, carry, preview, placement, replay and recovery. The existing Issue #59 GPU seat/latch/bracket authority and Issue #63 PCIe/GPU cable authority are separate Assembly targets, so the BuildKit flow must prove that one frame cannot be consumed by those modes and that their state, revisions and receipts do not change.

## Decision

- Resolve the canonical GPU only by `ComponentKind.GraphicsCard` and the complete accepted work-order/ticket/allocation `LineId`, `ProductId`, `ItemInstanceId` and `ReservationId` tuple.
- Append `GraphicsCardInHands = 11` and `GraphicsCardStaged = 12` without renumbering persisted BuildKit stages. Use a distinct stable operation identity and a graphics-card-specific managed capacity-one BuildKit container; the first five operation, receipt, revision and staged-custody records remain unchanged.
- Claim all six BuildKit containers atomically through `InventorySerializedTransferAccessSextuple`. Duplicate, foreign, partial or ghost topology fails before any authority is created.
- Permit GPU custody only through exact source → `ActorHands` → graphics-card BuildKit. Generic Inventory transfer, world drop, box, stack, cart and Assembly paths cannot bypass an active BuildKit receipt.
- Require motherboard, processor, DDR5 memory, M.2 storage and processor cooler staged prerequisites before GPU pickup or placement. Work-ticket progress is derived from authoritative staged receipts and changes only from `5/10` to `6/10` after the GPU commit.
- Commit domain custody before mutating world parent, pose, physics or visibility. A physical placement failure recovers the same GPU instance at the authoritative BuildKit pose; no clone, ghost or lost item is permitted.
- Keep graphics-card BuildKit, Issue #59 GPU seat/retention and Issue #63 PCIe route as distinct authorities. While a GPU BuildKit pickup receipt is active, primary, rotate, interact and drop edges belong to one BuildKit consumer; receipt-free legacy Assembly behavior remains available.
- Use one raycastable non-trigger support collider and one exact snap anchor for the GPU tray. Preview orientation advances through a keyed `0° ↔ 180°` half-turn; decorative rails, labels and progress text remain non-authoritative and `Ignore Raycast`.
- Bind native evidence to exact technical commit/tree, exact test XML, Mac build/runtime, Windows IL2CPP/D3D11 binaries, procedure hashes and interactive task cleanup. Runtime acceptance requires one exact success marker, one exact r40 readiness marker, zero forbidden markers and zero residue.

## Consequences

GarageGraybox r40 now lets the player take the exact reserved graphics card with real keyboard/mouse or gamepad input, carry and rotate the same physical object through a keyed 180° preview, and place it into its separate BuildKit tray. The first five components remain staged, reservation/allocation identity stays live, the work ticket shows `6/10`, and GPU Assembly plus PCIe cable authority remain untouched.

The remaining four BuildKit transfers—PSU, ATX24 cable, EPS12V cable and PCIe/GPU cable—plus `10/10` completion, job-specific assembly, electrical readiness, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, final art and Steam release remain separate dependent work.

## Verification

- Technical source commit `f40ef21058caf1a2aca3054218abfc1dd7305c01`, tree `c7500e7300f75f5d9b089bf23657750dccc5ffed`.
- Unity 6000.3.21f1 full regression: EditMode `690/690`, PlayMode `100/100`; failed, skipped and inconclusive `0`. Focused GPU BuildKit, Sextuple, preview/gamepad, scene-contract and runtime-smoke gates also passed, and `git diff --check` passed.
- Universal macOS Development/StrictMode build report `329,839,788` bytes. The ad-hoc signed executable is a valid deep/strict universal `x86_64 + arm64` bundle. Apple Silicon/Metal r40 readiness and the exact graphics-card BuildKit success marker each appeared once; forbidden markers and player residue were `0`.
- A complete verified Git bundle produced collision-free detached-clean Windows source at the same technical commit/tree. Unity 6000.3.21f1 completed an x64 IL2CPP build with Direct3D11 only; report size was `1,334,256,694` bytes and the `issue79-hardened-v3` Burst/native-link fatal-token count was `0`. Intel Iris Xe/Direct3D 11.0 feature level 11.1 interactive runtime emitted the exact host, r40 readiness and success markers once, exited `0`, shut down gracefully, deleted its scheduled task and left process residue `0`.
- Procedure-bound canonical evidence is exact `14/14`; `source-receipt.json` binds technical source, full tests, Mac/Windows native proof, procedures, source/docs commit `dd607d0af346bd1f0e28449f606761bc97e1495c`, tree `010b3a460c3241ed69d315bfb44047c1be82cb10` and successful Repository Guard `32874685021`.
- `Tools/verify-checkpoint-package.sh ... issue79` fail-closes on the exact technical commit/tree, exact nine-file closure delta, `690/690` and `100/100`, `issue79-hardened-v3`, the exact r40 GPU marker, 13 promoted artifacts, three procedures, task deletion and residue `0`.
- The immutable local package and the correct Windows-attached physical USB both passed incoming and atomically named final readbacks with identical `990/990` payload, `975/975` exact Git source, `14/14` evidence, `20,086,932` bytes and manifest `d2d399fa71ee37ed972b2e709987d0a375fe62fd8da3e5cfda5eb0ec571bb324`. Incoming residue, internal AppleDouble and final sidecar counts are `0`.

Physical lifecycle metadata commit `880523fcb71208796cce96564556a2170363c92a`, tree `448052665c3b64b1c565d460de6c648c498b698d` and Repository Guard `32876194890` passed. Acceptance is `24/24`; Issue #79 is closed and its Roadmap item is `Done`. PR #80 is the integration vehicle, while Issue #77 and parent Epic #10 remain open for their separate lifecycle and the next dependent component handoff.
