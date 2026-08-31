# ADR-0073 — Validation-Bound Quality Sign-Off and Packaging Release Receipt

**Status:** Mac technical source `b6c0f62`, tree `36f8cb6` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #138 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #137; Issue #135 exact current validation receipt'i üzerine kuruludur

## Context

Issue #135, exact customer işi ve fiziksel 10-parça/üç-kablo build için driver-bound deterministic benchmark, stress, thermal ve power sonucu üreten immutable `PcValidationReceipt` zincirini tamamladı. Steam 1.0 fiziksel servis akışındaki sıradaki bounded adım, passed/stable validation'ı aynı cycle'daki exact güvenli power-off ve değişmemiş Assembly ile bağlayıp oyuncunun PC'yi paketleme aşamasına serbest bırakmasıdır.

Bu dilim fiziksel paket/kutu item'ı, ambalaj malzemesi, custody transferi, etiket/seri/barkod, kargo, teslimat, müşteri kabulü, final ödeme veya garanti başlangıcı üretmez. `ReadyForPackaging` yalnız bu sonraki fiziksel aşamanın fail-closed giriş receipt'idir.

## Decision

- Unity bağımsız yeni `PSE.Quality` assembly'si `PSE.Core`, `PSE.Catalog`, `PSE.Inventory`, `PSE.Retail`, `PSE.Orders` ve `PSE.Assembly` domain sözleşmelerini tüketir; `noEngineReferences: true` kalır. İkinci Inventory, Orders, Assembly, power veya validation authority oluşturmaz.
- `CustomPcQualityReleaseAuthority`, exact owner `CustomPcWorkOrderAuthority` ile exact owner `PcValidationAuthority` instance'larına reference-bound kurulur. Yalnız quality-release kararlarını ve kendi immutable receipt history'sini yönetir.
- Completion non-empty stable operation ID, expected quality revision, owner work order, owner physical work ticket, owner passed/stable validation receipt ve o validation'ın exact power-on/preflight lineage'ını kapatan owner safe power-off receipt ister.
- Work order ve ticket aynı request, quote, customer binding, inventory claim, workbench ve exact serialized reservation setine ait olmalıdır. On satırın yedisi exact component item/product eşliği; üçü typed ATX24, EPS12V ve PCIe-GPU cable item eşliği taşır. Foreign, duplicate, missing, unowned veya claim dışı serialized reservation fail-closed'dur.
- Safe shutdown receipt `PowerOff / Off` olmalı; validation'ın exact `SourcePowerOnReceipt` ve `PreflightReceipt` instance'larını taşımalıdır. Current power authority Off, de-energized ve exact power-off revision'ında kalmalıdır.
- Completion current `EvaluateElectricalReadiness()` sonucunu validation'ın source readiness snapshot'ıyla build/chassis, yedi component item, üç cable item, bütün retain/secure/route operation ID'leri ve Assembly/cable revision'ları üzerinden exact karşılaştırır. Validation sonrası mekanik veya kablo drift'i receipt üretmez.
- Immutable `CustomPcQualityReleaseReceipt`, exact work order/ticket ve validation/power-off/readiness provenance'ını; score, stress, thermal, power ve quality değerlerini; expected/actual monotonik quality revision'ını taşır. Tek terminal sonucu `ReadyForPackaging`dır.
- Exact same operation + exact command replay aynı receipt instance'ını döndürür. Changed reuse `OperationConflict` olur. Farklı operation ID ile exact aynı current contextte kontrollü tekrar inceleme mümkündür ve monotonik history üretir.
- Receipt history bütünlük kontrolü replay lookup'tan önce çalışır. Owner, mapping, order, expected/current revision, source readiness reference veya historical lineage bozulursa replay dahil bütün command yolları `ReceiptHistoryInvalid` ile fail-closed olur.
- New power cycle veya Assembly drift, historical receipt'i silmez fakat current evaluation'ı `NotCurrent` yapar. Yeni paketleme release'i için yeni current validation + matching safe shutdown gerekir.
- Existing electrical station ve Workbench tekrar kullanılır. Validation sonrasında `E / A` strict-priority power-off yapar. İlk `LMB / RT` quality dosyası review'ını açar, ikinci `LMB / RT` release receipt'ini üretir. Yeni scene geometry, collider, camera, modal cursor veya input action yoktur.
- Same-frame Interact + Primary Action yalnız power transition yürütür; release atlanmaz. Pause, raw Pause edge, range/focus/LOS kaybı, busy hands, competing world owner veya exact context değişimi açık quality review'ını Primary edge'i tüketmeden sıfırlar.
- Workbench observer-only `WaitingForValidation / AwaitingSafeShutdown / ReadyForReview / Reviewing / ReadyForPackaging / Rejected / NotCurrent` durumlarını gösterir. Observation `TryGetQualityRelease` kullanır; authority yaratmaz.
- Malformed quality history normal power-on yolunu softlock edemez. Quality gate primary action'ı fail-closed reddederken explicit `E / A` yeni cycle power-on'u çalıştırabilir.
- GarageGraybox marker `garage-validation-bound-quality-release-r67-v1` ve native flag `-pse-quality-release-smoke` kullanır. Smoke validation prerequisite'ini nested assisted setup ile kurar; player-triggered safe shutdown, mouse review, virtual-gamepad release, exact ten-line job, immutable replay/history ve no-upstream-mutation invariantlarını tek üst markerda doğrular.

## Consequences

GarageGraybox artık exact customer work order → physical ticket → complete Assembly → driver/validation → safe shutdown zincirini tek immutable `ReadyForPackaging` receipt'ine kadar oynatır. Quality kararı source validation sonucunu yeniden icat etmez; exact owner receipt'ini ve current fiziksel durumu tüketir.

Bu bounded sonuç görünür kalite review ve paketleme giriş kapısıdır; gerçek arıza teşhisi/onarım kararı, fiziksel packaging workcell/item/custody, save/load persistence, müşteri teslimatı, garanti/fatura/ledger settlement veya release logistics değildir. Sıradaki ayrı ürün dilimi bu receipt'i fiziksel paketleme item/custody ve teslimat zincirine bağlamalıdır.

## Current verification

- Technical source `b6c0f629b78566d743dbb041bfaf792f7c0164c8`, tree `36f8cb6cec9340966181511a18f3caa276eb12f2`; draft PR #138 açıktır. Issue #137 ve Roadmap kartı `OPEN / In Progress` kalır.
- Final-source targeted scene `12/12`, quality authority/history `5/5`, keyboard/mouse/gamepad/context/P0 quality input `4/4`, validation regression `6/6`; final full EditMode `810/810`, full PlayMode `191/191`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `330,776,338` bayt ve `304` dosyadır. Executable `117,179` bayt, SHA-256 `de920bcd2d1c0ac8c8e7317ba082356487d4c50999b2acb20cecb04fded00941`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 graphical r67 native smoke readiness ve exact quality-release success markerlarını birer kez üretir; observed exit `0` ve final PC Shop process residue `0`dır.
- Local Repository Guard technical head üzerinde `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1290` verdi; final PR run ayrı olarak kaydedilecektir.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.
