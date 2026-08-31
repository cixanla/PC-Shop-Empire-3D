# ADR-0074 — Quality-Bound Physical Custom-PC Packaging and Dispatch Custody

**Status:** Mac technical source `79ea367`, tree `12dabe0` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #140 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #139; Issue #137 exact current `ReadyForPackaging` quality receipt'i üzerine kuruludur

## Context

Issue #137, exact müşteri işi, physical work ticket, on serialized reservation satırı, complete Assembly, passed/stable validation ve matching safe shutdown zincirini immutable `ReadyForPackaging` quality receipt'ine kadar tamamladı. Steam 1.0 fiziksel servis akışındaki sıradaki bounded adım, bu exact release'i bir fiziksel paket kimliğine dönüştürmek ve aynı paketin paketleme tezgâhı, oyuncu elleri, dünya zemini, taşıma arabası ve sevk sahnesi arasındaki custody değişimlerini kayıpsız/duplikasyonsuz izlemektir.

Bu dilim kargo firması, araç yükleme, rota, müşteri teslimatı/kabulü, final ödeme, fatura, garanti başlangıcı, iade, save/load persistence veya Steam release packaging üretmez. `DispatchStaging` teslim edilmiş anlamına gelmez; yalnız mühürlü fiziksel paketin mağaza içi sevk sahnesine bırakıldığını kanıtlar.

## Decision

- Unity bağımsız `PSE.Fulfillment` assembly'si `PSE.Core`, `PSE.Inventory`, `PSE.Retail`, `PSE.Orders`, `PSE.Assembly` ve `PSE.Quality` sözleşmelerini tüketir; `noEngineReferences: true` kalır. İkinci Inventory, Orders, Assembly veya Quality authority oluşturmaz.
- `CustomPcPackageAuthority`, one exact `CustomPcQualityReleaseAuthority` instance'ına reference-bound kurulur. Yalnız sealed-package identity, package revision ve append-only custody history'sini yönetir.
- Seal command non-empty stable package/operation ID, expected fulfillment revision ve owner authority'nin exact current `ReadyForPackaging` receipt instance'ını ister. Null, foreign, historical, stale veya drift etmiş release paket üretmeden fail-closed olur.
- Bir quality-release operation ID'den exactly one package üretilebilir. Aynı package veya quality release için duplicate seal reddedilir; Inventory item veya Assembly aggregate çoğaltılmaz.
- Immutable `CustomPcPackageReceipt`, package/seal operation kimliği, exact source quality receipt, work order, physical work ticket, customer binding, inventory claim, build/chassis ve expected/actual revision'ı taşır. Terminal package state `Sealed`, ilk custody `PackagingWorkbench`tir.
- Exact same seal operation + exact command replay aynı receipt instance'ını döndürür. Changed reuse `OperationConflict` olur. Fulfillment revision monotoniktir ve overflow fail-closed'dur.
- Custody enum yalnız `PackagingWorkbench`, `ActorHands`, `WorldFloor`, `TransportCart` ve `DispatchStaging` değerlerini taşır. İzinli directed transitions açık allowlist'tir: workbench→hands; hands→floor/cart/dispatch; floor/cart/dispatch→hands. Diğer geçişler mutation öncesi reddedilir.
- Her custody değişimi non-empty stable operation ID, exact owner package, exact source/target ve expected revision isteyen immutable `CustomPcPackageCustodyReceipt` üretir. Replay same-instance; changed reuse conflict'tir.
- `ValidateCustodyTransfer()` side-effect-free preflight'tır. Fiziksel item hareketi ancak preflight geçince başlar; domain commit başarısız olursa pickup/load/unload/drop/recovery/dispatch yolları fiziksel rollback yapar ve rollback de başarısızsa açık `PhysicalRollbackFailed` döner.
- Receipt history bütünlük kontrolü replay lookup'tan önce çalışır. Owner, mapping, order, expected/current revision, package/source quality lineage, transition veya reconstructed current custody bozulursa seal ve transfer replay'leri dahil bütün command yolları `ReceiptHistoryInvalid` ile fail-closed olur.
- Garage session authority'yi yalnız exact current quality release hazır olduğunda lazy oluşturur. Session invariants fulfillment receipt history'sini kapsar; observation authority yaratmaz.
- Existing ten source component/cable projections seal öncesi görünürdür. İkinci review/seal input'u tam bir sealed LargeBox projection'ı açar ve on source projection'ı gizler; ayrı ikinci fiziksel PC kopyası göstermez.
- Physical package stable serialized prototype ID, `LargeBox` carry profile, 12 kg rigidbody, collider, safe-pose/recovery ve identity label taşır. Paket `PackagingWorkbench` anchor'ında başlar; oyuncu aynı item instance'ını eller, dünya, cart ve dispatch arasında taşır.
- Packaging station iki adımlıdır: ilk `E / A` exact quality dosyasını review eder, ikinci `E / A` paketi mühürler. Pause, raw pause edge, range/focus/LOS kaybı, busy hands, competing world owner veya context drift input'u tüketmeden fail-closed olur.
- Dispatch station yalnız exact sealed package oyuncunun elindeyken stage eder. Physical placement ve custody commit birlikte başarılı olmalı; generic package teleport veya second writer yoktur.
- `WorldInteractionFocusGate` ortak range/focus/LOS/pause/competing-owner kontrolünü allocation-free ve fail-closed uygular. Packaging ve dispatch tek raycastable focus hedefi taşır; dekoratif geometry `Ignore Raycast` kalır.
- GarageGraybox marker `garage-quality-bound-physical-packaging-r68-v1` ve native flag `-pse-custom-pc-packaging-smoke` kullanır. Smoke nested exact quality-release setup, keyboard review, virtual-gamepad seal, one physical package, hidden source projections, hands→cart→hands, dispatch, four custody receipt, replay, upstream isolation ve invariants zincirini doğrular.
- Packaging workbench sol duvar servis alanında `(-3.28, 0, 0.50)` konumundadır. Bu konum pre-existing customer→work-ticket keyboard/mouse ve gamepad yürüyüş koridorunu açık bırakır; exact route regression ayrı testlidir.
- Intentional r68 geometry 9 active ve 5 initially inactive package renderer ekler. Runtime exact budgets retail path için `473 active / 502 total`; retail hero hariç Assembly regression için `468 active / 493 total`dır. Light/camera bütçesi `5/1` olarak değişmez.

## Consequences

GarageGraybox artık exact customer work order → physical ticket → ten-part Assembly → validation → quality release → sealed physical package → hands/cart/floor recovery → dispatch staging zincirini tek immutable package identity ve append-only custody history ile oynatır. Source projections seal sonrasında gizlenir; package authority source Quality/Assembly/Inventory gerçeğini yeniden üretmez.

Bu bounded sonuç mağaza içi paketleme ve sevk hazırlığıdır. Courier assignment, shipping label/barcode/serial policy, vehicle loading, route/travel, customer handover/acceptance, final ledger settlement, invoice/warranty/return, save/load ve Steam release logistics ayrı ürün dilimleridir.

## Current verification

- Technical source `79ea367af67549592a6ba58acd53afa74e7f25cb`, tree `12dabe0220ffe759750d73cc25e96e2c6774221d`; docs checkpoint `a2f74ec118b8de9021f4e400c1de961af0478ff7`; draft PR #140 açık/mergeable'dır. Issue #139 ve Roadmap kartı `OPEN / In Progress` kalır.
- Exact-head targeted fulfillment domain `4/4`, scene/r68 `13/13`, packaging input `4/4`, keyboard/mouse+gamepad work-ticket route regression `2/2`, Assembly/Retail hero regression `2/2`; full EditMode `815/815`, full PlayMode `195/195`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `330,891,503` bayt ve `306` dosyadır. Executable `117,179` bayt, SHA-256 `392b5596e46d2a01b96965ecc51979afcb3b542b53272e13994339c8f65da71d`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 graphical r68 native smoke readiness ve exact packaging success markerlarını birer kez üretir; observed exit `0` ve final PC Shop/Unity/player/IL2CPP residue `0`dır.
- Local Repository Guard technical head üzerinde `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1326` verdi. GitHub technical Repository Guard `33417330365` ve docs checkpoint Guard `33418499473` PASS'tir; `git diff --check`, meta pairing ve focused hygiene audit temizdir.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.
