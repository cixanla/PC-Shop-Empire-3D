# ADR-0042 — Accepted Custom-PC Request, Immutable Quote and Exact Serialized Reservation

**Status:** Implemented and verified on macOS and Windows; physical USB closure pending<br>
**Date:** 24 August 2026<br>
**Scope:** Issue #64, child of Epic #10

## Context

The garage prototype already had one authoritative customer consultation chain, exact serialized stock, and a physically retained ten-part PC component/cable set. It still lacked the business boundary that turns a consultation into an owned custom-PC job: an accepted request, a frozen compatible bill of materials and an all-or-none reservation of the exact physical components. This slice had to add that boundary without claiming physical assembly completion, electrical readiness, POST/OS, benchmark/QA, packaging, settlement, Save/Guardian, final art, or Steam release readiness.

## Decision

- Create one stable typed custom-PC request bound to the exact customer, active visit, owned consultation, graphics-first profile, integer budget, currency and acceptance time. A conflicting reuse of the request identity fails closed.
- Freeze one immutable quote containing exactly ten deterministically ordered lines: motherboard, CPU, DDR5 DIMM, M.2 SSD, processor cooler, graphics card, PSU, ATX24 cable, EPS12V cable and PCIe/GPU 6+2 cable. Every line binds one exact ProductId, ItemInstanceId, ReservationId, integer minor-unit price and compatible component role.
- Validate the complete BOM, product metadata, required role set, duplicate identities, CPU/motherboard socket compatibility, single currency and bounded budget before publishing a quote or mutating Inventory.
- Reserve the full serialized set through one managed Inventory operation and one revision. Partial reservation, raw release/consume, an eleventh item, an externally adopted claim, an already-owned item, reservation drift or duplicate item/reservation identity fails without mutation.
- Represent a committed managed set with one `ManagedSerializedReservationSetRegistration`. The claim-keyed and operation-keyed indexes reference that same registration and validate operation ID, claim ID, applied revision, canonical payload and access-object identity before returning the internal access capability.
- Make exact replay idempotent. Reusing an operation ID with another claim or payload is a conflict. If Inventory committed the exact managed set but quote publication was interrupted, retry recovers only the matching owned access and publishes the one quote without a second reservation or revision.
- Keep Retail request/quote authority, Inventory reservation authority, customer visit/consultation, basket/checkout/economy and physical Assembly state isolated. A failed or replayed custom-PC command cannot mutate unrelated authorities.
- Keep `GarageCustomerFlowRuntime` and `PlayerCarryController` as the visible first-person route and single input-consumer boundary. Keyboard/mouse and real Input System gamepad deliberately advance consultation to accepted request and then to visible ten-line quote/reservation. Range, focus, line of sight, pause, release/repress and competing carry input remain fail-closed.
- Preserve an accepted request beyond the original consultation deadline so the visible quote can be completed without the customer silently leaving. `FirstPersonMotor` returns immediately on a pause-toggle frame, preventing stale movement/look input from producing a resume lurch.

## Consequences

GarageGraybox r33 now exposes the first authoritative custom-PC commercial handoff. The player can accept one owned graphics-first request, generate one visible compatible ten-line quote and reserve the exact physical component set atomically. Existing retail checkout, economy settlement and physical assembly continue to use their own authorities. Reservation release into a build order, physical build progress, power-on/POST, fictional OS, benchmark/QA, packaging, delivery, payment/final settlement, Save/Guardian and final UI/art remain separate dependent packages.

## Verification

- Feature commit `c7d38845ffccb5ae6e5365e580c238d70f8dac95`; tree `615c9c4398f6a0be16c3a693dd812aa3f5541291`.
- Full macOS suites: EditMode `647/647`, PlayMode `59/59`; failed, skipped and inconclusive `0`.
- Universal macOS Development/StrictMode build: `329,396,456` bytes; executable contains `x86_64` and `arm64` slices. Active Apple M1/Metal 1280×720 readiness `garage-custom-pc-quote-reservation-r33-v1` and the exact custom-PC smoke passed once.
- Full Windows suites from the exact clean source SHA: EditMode `647/647`, PlayMode `59/59`; failed, skipped and inconclusive `0`.
- Windows x64 IL2CPP Development/StrictMode build: `1,326,137,709` report bytes. The real player ran in the active Windows console session on Intel Iris Xe, Direct3D 11.0 feature level 11.1, emitted r33 readiness and the exact custom-PC smoke once, then closed by its PID.
- A short visible Mac human-play pass observed forward/right/back/left position changes and verified that pause blocks movement and resume does not replay the held left input. Mouse-look was not promoted to a manual observation because the UI-control layer lost the window during that specific drag attempt; automated Input System and native readiness evidence remain the claimed mouse/gamepad gates.
- Feature Repository Guard [32698054990](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32698054990), success. Draft PR [#65](https://github.com/cixanla/PC-Shop-Empire-3D/pull/65) remains unmerged until documentation, final CI and physical USB closure are complete.
- Detailed artifact hashes and platform evidence are recorded in `Docs/Evidence/ACCEPTED-CUSTOM-PC-REQUEST-IMMUTABLE-QUOTE-AND-EXACT-SERIALIZED-RESERVATION-CHECKPOINT-2026-08-24.md`.
- Issue #64 and its Roadmap item remain open/In Progress until the final source/docs package completes two physical USB manifest readbacks and the post-checkpoint Repository Guard succeeds.
