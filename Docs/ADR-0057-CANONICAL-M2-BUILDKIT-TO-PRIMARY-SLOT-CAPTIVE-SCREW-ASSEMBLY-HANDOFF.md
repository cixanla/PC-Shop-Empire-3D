# ADR-0057 — Canonical M.2 BuildKit-to-Primary-Slot Captive-Screw Assembly Handoff

**Status:** Accepted for source/domain/scene/input/full-regression, exact-head macOS/Windows native and technical-CI gates; source/docs CI, final canonical/local package, real-human session, healthy physical USB and administrative closure pending
**Date:** 26 August 2026
**Scope:** Issue #95, child of Epic #10

## Context

Issues #68 through #87 stage the exact ten reserved custom-PC components into component-specific capacity-one BuildKit slots. Issue #89 moves the same reserved motherboard into the existing chassis Assembly authority and secures it. Issue #91 moves the exact CPU into the existing ProcessorSocket and closes retention. Issue #93 then moves the exact DDR5 UDIMM into the existing A2 MemorySlot and leaves both latches closed. The original ten staging receipts remain immutable preparation history while live custody advances through Assembly.

Issue #57 already owns the single M.2 2280 storage family, primary M-key slot topology, 2280 standoff, guided 18-degree insertion, raised/flat seat states, motherboard-owned captive screw, exact seat/press/secure/loosen/remove receipts and replay rules. Issue #75 stages the reserved M.2 NVMe into the canonical Storage BuildKit. The missing boundary is a reservation-safe custody bridge from that BuildKit slot to the existing Issue #57 Assembly authority after the motherboard, CPU and DDR5 prerequisite chain is live.

A second Inventory, shadow storage slot, duplicate captive screw, regenerated SSD identity or receipt-free shortcut would split authority. It could leave the serialized item economically reserved while physically duplicated, lost or replaced by a value-equal object.

## Decision

- Resolve storage only from the exact owned work order, canonical `Storage` line and full `LineId`, `ProductId`, `ItemInstanceId`, `ReservationId`, parent allocation and original staging-receipt tuple. Ordinal, display name, component-only and value-equivalent matches cannot acquire authority.
- Require the historical exact ten-receipt `10/10` aggregate plus the live Issue #89, #91 and #93 Assembly chain. The exact motherboard must remain Workbench-owned and `SeatedSecured`; the CPU must remain ProcessorSocket-owned and `ProcessorRetained`; DDR5 must remain exact A2 MemorySlot-owned and `MemoryModuleRetained`. Their source receipts must be live and exact.
- Require the configured primary M.2 2280 slot to be managed, capacity one, foreign-container-free, empty/open and paired with its existing motherboard-owned captive fastener. Full hands, occupied/secured slot, wrong tool, stale authority or revision overflow fail closed.
- Use a stable storage assembly-handoff operation identity distinct from staging, motherboard/CPU/DDR5 handoffs and Issue #57 seat/press/screw operations. Immediate and delayed replay return the same immutable receipt without a second custody or revision change.
- Add only the narrowly registered Storage BuildKit → `ActorHands` release. Subsequent reversible transfer remains the existing exact Assembly-owned M.2 Slot ↔ `ActorHands` path; generic reserved transfer and checkout remain closed.
- Preserve reservation and parent allocation through pickup, guided insertion, press-flat, captive-screw tighten/loosen, detach and reseat. Preserve all ten original staging receipts and visible `10/10` preparation history while current custody is tracked separately.
- Reuse `AssemblyBuildAuthority` storage seat, press, fastener and remove state/replay rules. The handoff authorizes the existing Issue #57 path; it does not recreate or relax M.2 family, topology, M-key orientation, obstruction, preview-equals-commit, tool or fastener gates.
- Preserve the same Unity component instance and stable serialized ItemId through BuildKit → hands → primary M.2 slot → raised → flat → retained → flat → raised → hands → reseat. The motherboard stays secured, CPU and DDR5 stay retained, and the other six uninstalled BuildKit components, containers, receipts, revisions and projections remain untouched.
- Fail closed with zero authoritative mutation for foreign order/operation/line/product/item/reservation/allocation/staging/prerequisite/target, value-equal forgery, source drift, stale BuildKit/Inventory/Assembly revision, full hands, occupied slot, wrong tool and overflow.
- Stage physical pickup reversibly before committing BuildKit custody: `BeginCarry` must succeed without publishing a usable held-input state; then the exact authority handoff commits. If authority rejects, the same physical instance returns to the exact BuildKit safe pose and no held item is exposed. Tests must prove both physical-stage failure and authority-rejection rollback. This is the explicit atomicity refinement for Issue #95; it replaces the issue draft's older domain-first wording without weakening no-duplicate/no-loss guarantees.
- After both physical staging and authority success, one exact held item becomes visible to input. Storage pickup, M.2 seat/screw and generic drop edges have one deterministic consumer. Range, focus, line of sight, pause, empty-hands, held/co-edge and release/repress rules remain fail closed for keyboard/mouse and gamepad.
- Keep motherboard unsecure/detach blocked while storage is seated or secured. Keep storage detach blocked while the captive screw is tight. Keep CPU and DDR5 custody/state unchanged throughout this slice.
- Bind acceptance to the exact technical commit/tree, full test XML, Mac and Windows native artifacts, procedure hashes, cleanup and package readbacks. Runtime acceptance requires one r48 readiness marker containing `storage-assembly-handoff=ready`, one exact storage handoff success marker, zero handoff-failure/fatal markers and zero player/Unity/task residue.

## Consequences

GarageGraybox r48 lets the player approach the completed `10/10` BuildKit after the motherboard is secured, CPU retained and DDR5 retained, take the canonical M.2 SSD with `E / Gamepad South`, carry that exact object, open the existing keyed M.2 seat mode with `Mouse Left / Gamepad RT`, commit the M-key-aligned 18-degree insertion and flat seat with `G / Gamepad East`, tighten the captive screw, prove removal is blocked while secured, loosen, detach and reseat the same instance. The BuildKit slot reports `M.2 MONTAJDA`; immutable ticket identity and completed preparation history remain visible and unchanged.

The PC is not complete or electrically ready. Cooler/TIM, GPU, PSU and cable installation, electrical validation, power-on, POST/BIOS/OS/drivers, benchmark/QA, packaging, delivery, settlement, Save/Guardian, staff/customer/world expansion and final art remain separate dependent work.

Pause and focus-regain are also fail-safe continuous-input boundaries. Held keyboard/gamepad movement and gamepad look must return fully neutral before they can drive a resumed frame; neutral detection inspects each resolved Move control rather than the aggregate vector, so opposing key pairs cannot cancel and release the latch early. Fresh pointer delta remains responsive after resume.

## Current verification

- Technical source commit `42c1ae4dff2421b38879c0bfc82b4bf52522be1e`, tree `16304340da0ae7e42d8e7dd1ea6aef66ffe27efc`.
- Unity 6000.3.21f1 full EditMode `722/722` and full PlayMode `130/130`; failed, skipped and inconclusive `0`.
- macOS Development build report `330,195,891` bytes. The deep/strict-valid universal `x86_64 + arm64` executable emits one r48 readiness marker and one exact M.2 Assembly handoff success marker, reaches Input System shutdown, exits `0` and leaves no player residue.
- Detached-clean Windows exact-head Unity 6000.3.21f1 x64 IL2CPP/only-D3D11 build report `1,343,654,204` bytes. Intel Iris Xe Direct3D 11.0 feature level 11.1 runtime has exact host/readiness/success counts `1/1/1`, forbidden count `0`, exit `0`, graceful shutdown, deleted scheduled task and residue `0`.
- Exact-head technical Repository Guard [32962078481](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32962078481) passed.
- Source/docs Repository Guard, final source receipt and canonical `14/14`, immutable local package, healthy physical-USB lifecycle, exact-r48 real-human session and Issue/Project administrative closure remain pending. Automated smoke is not relabelled as human evidence. Windows D: reports `Warning / Full Repair Needed` and remains read-only.
