# ADR-0043 — Immutable Custom-PC Work Order, Physical Work Ticket and Fail-Closed Handoff

**Status:** Accepted; verified on macOS, Windows and physical USB; Issue closed and Roadmap Done<br>
**Date:** 24 August 2026<br>
**Scope:** Issue #66, child of Epic #10

## Context

Issue #64 froze one accepted custom-PC request, one compatible ten-line quote and one exact serialized reservation set. The game still lacked the next physical business boundary: converting that accepted commercial result into one owned build job and one visible work ticket at the authored garage workbench. The handoff had to remain deterministic and replay-safe without deleting, releasing, moving or consuming any reserved item and without claiming that physical assembly, electrical readiness, POST, OS, benchmark, packaging or delivery had begun.

## Decision

- Introduce stable typed `BuildOrderId`, `WorkTicketId` and handoff `OperationId`. One immutable build order and one immutable work ticket freeze the exact request, quote, customer, managed claim, target workbench and all ten quoted line/reservation/item identities.
- Revalidate the complete managed reservation set immediately before commit. Missing, extra, duplicate, stale, foreign, forged, drifted or value-equal-but-not-owned identities fail closed before mutation.
- Publish one terminal, operation-keyed Inventory allocation receipt exactly once. The ten reservations and ten serialized item records remain live and unchanged; generic checkout consume/release paths are not used and no item moves container.
- Give one quote, claim and target workbench only one active build-order/work-ticket owner. Exact replay returns the same records and receipt without revision drift. A mismatched replay is a conflict.
- Recover interrupted authority publication only from the exact stored allocation receipt. Recovery cannot create an orphan order, a second allocation or a different ticket.
- Keep `CustomPcQuoteAuthority`, Inventory allocation authority and `AssemblyBuildAuthority` isolated. Handoff cannot change assembly revision, receipts, slots, retained parts or cable routes.
- Author one physical `CustomPcWorkTicketStationProjection` at the canonical garage workbench. It shows the stable job identity, exact `10/10` reservation result and `MONTAJA HAZIR • HENÜZ BAŞLAMADI`. Decorative presentation is colliderless and non-authoritative; the dedicated interaction target owns focus and line-of-sight.
- Require authored range, focus, line of sight, empty hands and one fresh Interact edge. Pause, held/co-edge input and competing carry/assembly targets resolve to one deterministic consumer and otherwise fail closed.
- Preserve the complete first-person customer-to-workbench route for W/S/A/D, mouse-look, keyboard/mouse and real Input System gamepad. The dashboard cannot teleport the player, parts, kit or assembly state.
- Pin the Windows validation player to x64 IL2CPP and Direct3D11, read back the applied settings, restore prior project settings in `finally`, and withhold the success marker until the original `ProjectSettings.asset` bytes are restored exactly.

## Consequences

GarageGraybox r34 now exposes the next visible custom-PC workflow boundary. The player can turn the accepted ten-part quote into one immutable build order, walk to the authored workbench and publish one physical work ticket. Inventory records that the exact reservation set belongs to that job while every serialized item stays where it was. The ticket says that assembly is ready but not started.

Physical component transfer into a build kit, component attachment, electrical power-on, POST/BIOS, fictional OS and drivers, benchmark/QA, packaging, delivery, payment/final settlement, Save/Guardian authority and final art/audio/VFX/UI remain separate dependent packages.

## Verification

- Core feature `f9545605baff423f05615e7326902e24dc82aeeb`, tree `c0ed1add79162c334df4fc833eedf1dfaeb5cbc8`.
- Current technical head `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`, tree `69ea366cc49e99b653f5d02d9c0f238b4906de69`. It retains the byte-exact Windows builder restore/readback path and adds deterministic physical-ticket/carry/cart Interact arbitration plus complete no-teleport item-state assertions.
- Full EditMode `661/661` and PlayMode `66/66`; failed and skipped `0`.
- Universal macOS Development/StrictMode build `329,478,891` bytes; active Apple M1/Metal 1280×720 player emitted r34 readiness and the exact work-ticket success marker once with no failure/assertion/unhandled exception.
- Exact clean Windows source produced a `1,328,828,053`-byte x64 IL2CPP player with only Direct3D11 and a byte-exact project-settings restore marker.
- The active Windows console player ran on Intel Iris Xe / Direct3D 11.0 feature level 11.1. Windows host, r34 readiness and exact work-ticket success markers each appeared once; forbidden markers were `0`.
- Technical-source [Repository Guard 32721069982](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32721069982) passed for `f8afd62326c74aff23fa10bb33ef79ecb9a656b6`. Exact source/docs checkpoint `4e1ef4322d9ef049e3aac915c611474f6bee92fd`, tree `4df76fb1b50da53bdee7e65cb64acf0e73a5c018`, passed [Repository Guard 32723213686](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32723213686). Local staging metadata `2dc67d2c83000287925dc34bf784b1287cffc916` and provenance-corrected pre-USB head `67529275f2c844a44511de5dc344cedbe1158624` passed Guards `32724354230` and [32724718603](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32724718603). Physical USB metadata `a80e325b1534f4f45fc171a36d73fdfe4ccfc95b` passed [Repository Guard 32726202296](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/32726202296); acceptance is `18/18`, Issue #66 is `CLOSED/COMPLETED`, Roadmap is `Done`, and parent Epic #10 remains open/In Progress. [PR #67](https://github.com/cixanla/PC-Shop-Empire-3D/pull/67) is the integration vehicle for this accepted checkpoint.
- Detailed hashes, byte counts, marker receipts, the exact canonical nine-file allowlist and closure boundaries are recorded in `Docs/Evidence/IMMUTABLE-CUSTOM-PC-WORK-ORDER-PHYSICAL-WORK-TICKET-HANDOFF-CHECKPOINT-2026-08-24.md`; the canonical local evidence source is `/Users/cixanla/Developer/PCShopEmpire3D/TestResults/issue66-f8afd62`.
- Local immutable package `2026-08-24_STAGE_B_IMMUTABLE_CUSTOM_PC_WORK_ORDER_PHYSICAL_WORK_TICKET_HANDOFF` passed incoming and final verifier readbacks with `906/906` manifest rows, `896/896` exact Git source, `9/9` evidence, `17,330,935` payload bytes and manifest `1514481a5b8dc90aae89f6de1d0e49ac4c6e964ee280c8170e6e224206444121`.
- The external physical USB was verified at exact `/Volumes/cixanla/CIXANLA`, with `90_BACKUPS/PCShopEmpire3D` and the prior Issue #62 milestone chain present. Collision-free `.incoming-issue66-6752927` passed the same complete verifier, was renamed atomically to the final milestone on the same filesystem, and the final directory passed the complete verifier a second time. Both physical readbacks proved `906/906`, `896/896`, `9/9`, `17,330,935` bytes and manifest `1514481a5b8dc90aae89f6de1d0e49ac4c6e964ee280c8170e6e224206444121`; internal/sibling AppleDouble and remaining incoming counts are `0`.
