# ADR-0036 — Deterministic Single Air-Cooler Seating and Four-Point Retention

**Status:** Accepted and implemented<br>
**Date:** 22 August 2026<br>
**Scope:** Issue #58, child of Epic #10

## Context

The physical PC assembly line needed one processor-cooling component after the retained CPU and secured motherboard gates. The slice had to model a factory-applied, single-use thermal interface and visible four-point mounting without creating a second Inventory authority, a physics-driven attachment, manual thermal-paste scope, or a completed benchmark.

## Decision

- Add append-only `ProcessorCooler` catalog classification and one typed `Lga1700TopDownAirPreAppliedTim` product.
- Claim Workbench, ProcessorSocket, MemorySlot, StorageSlot and capacity-one ProcessorCoolerSlot atomically in one Inventory revision; partial or ghost claims fail closed.
- Model one immutable cooler slot, bracket and four retention-point topology. Retention uses exact cross order `1→3→2→4`; release uses its reverse.
- Model the reversible runtime sequence `EmptyOpen ↔ CoolerSeatedUnsecured ↔ CoolerRetained`. Seat/remove transfers the exact serialized item between Hands and ProcessorCoolerSlot. Retain/unretain does not mutate Inventory custody or revision.
- Consume the cooler item's pre-applied TIM exactly once at successful seating and preserve that fact with the serialized item. Receipt-history folding rejects duplicate consumption, stale lineage and cross-kind conflicts.
- Block CPU-retention opening and motherboard detachment while the cooler is installed. A retained cooler satisfies only its bounded readiness requirement; the PC remains `BuildIncomplete`.
- Use a deterministic two-orientation guided pose with range, focus, LOS, socket/interface, support, RAM-clearance and obstruction gates. Fixed NonAlloc saturation fails closed; presentation never becomes authority.
- Keep `PlayerCarryController` as the only input owner. Keyboard/mouse and gamepad use existing actions and dynamic prompts; generic placement, stacking and cart paths cannot consume a held cooler.
- GarageGraybox r27 shows a readable semi-realistic top-down air cooler with cold plate, TIM surface, fin stack, fan, bracket and four visible retention points.

## Consequences

The game now has a visible, reversible processor-cooler installation loop with stable item identity, authoritative custody and deterministic retention. Pre-applied TIM prevents unrealistic unlimited reseating of the same consumed item. Separate paste, cleaning/reapplication, liquid cooling, alternate sockets, fan cabling, GPU, PSU, cable routing, POST/BIOS/OS, completed benchmark scoring, final art and Windows/Steam validation remain separate gates.

## Verification

- EditMode: 521/521.
- Real Input System PlayMode: 38/38.
- Universal macOS Development/StrictMode build: 328,534,723 bytes.
- Active Apple Silicon/Metal workstation (Apple M1), 1280×720: r27 readiness and exact `GARAGE_COOLER_RUNTIME_SMOKE` passed.
- Feature commit: `e2f10a22c37101cb12c5d6530c8f104deb72e99d`; tree `55d5f0d733530a2e4c1400f4f83c29f37dcafff8`.
