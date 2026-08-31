# ADR-0072 — Driver-Bound Deterministic Benchmark, Stress and Thermal Validation

**Status:** Mac technical source `f082ef5`, tree `c387100` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #136 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #135; Issue #133 exact current fictional driver receipt'i üzerine kuruludur

## Context

Issue #133, exact installed fictional OS ve current firmware → POST → power-on → preflight lineage'ı üzerinde oyuncunun Workshop Driver Bundle kurulumunu immutable receipt ile tamamladı. Steam 1.0 fiziksel PC servis zincirindeki sıradaki bounded adım, bu exact current driver ve aynı fiziksel 10-parça/üç-kablo build için oyuncunun mevcut elektrik test istasyonunda görünür bir validation çalıştırmasıdır.

Bu dilim gerçek Cinebench/3DMark veya vendor benchmark binary'si, işletim sistemi süreci, internet indirmesi, host CPU/GPU probu, gerçek sensör/telemetri, wall-clock yükü, fan eğrisi, overclock, fault injection veya hardware damage üretmez. Ürün sözleşmesi yalnız sürümlü fictional katalog/profile verisinden hesaplanan deterministic benchmark, fixed stress, peak thermal, power margin ve quality sonucu ile onun immutable provenance'ıdır.

## Decision

- Ayrı `PcValidationAuthority`, exact `PcFictionalDriverInstallationAuthority`, `PcPowerStateAuthority`, `AssemblyBuildAuthority` ve `PcPowerBudgetAuthority` owner zincirine reference-bound kurulur. Validation dışındaki hiçbir gameplay authority veya ledger'ın sahipliğini üstlenmez.
- Immutable `PcPerformanceCatalog`, canonical `PcComponentCatalog` ile aynı exact owner'a ait yedi fictional ürünün component kind, integer performance score, thermal load ve cooling capacity verilerini bağlar. Unsupported kind, foreign catalog, duplicate/malformed metadata ve eksik specification fail-closed'dur.
- Sürümlü `PcValidationProfile`; fixed stress step, ambient sıcaklık, integer thermal-rise scale, CPU/GPU limitleri, minimum power margin ve Standard/Good/Excellent score eşiklerini bağlar. Bütün değerler bounded integer'dır.
- Validation completion non-empty stable operation ID, exact current installed driver receipt, exact current-cycle firmware receipt, expected current power-state revision ve expected validation revision ister. Power state `Energized`, driver/OS/storage current ve exact electrical readiness + sufficient power budget olmalıdır.
- Receipt exact driver/OS/storage; firmware → POST → power-on → preflight; build/chassis; bütün component item/product/retain-or-secure operation; üç power-cable item/route operation/revision; performance catalog/profile; power budget ve result metric lineage'ını immutable taşır.
- Sonuç hesabı yalnız integer işlemler kullanır. Wall-clock, `Time.deltaTime`, frame timing, FPS, random seed, host hardware probe, floating nondeterminism veya gerçek sensor verisi validation gerçeği değildir.
- Prototype fictional katalog değerleri `34 + 117 + 31 + 49 + 25 + 121 + 24 = 401` aggregate benchmark score üretir. Fixed `300` stress step, `22 °C` ambient, integer ceiling thermal hesabıyla CPU `67 °C`, GPU `64 °C`; current power budget `380 W / 500 W / 550 W` ve `+50 W` margin üretir. Sonuç `Stable / Good / PassedForQualityStage` olur.
- `AssemblyBuildAuthority.EvaluateBenchmarkReadiness()` aggregate mekanik kapı olarak düzeltilmiştir: incomplete required component/route tanımı `BuildIncomplete`, eksik ATX24/EPS12V/PCIe-GPU route `PowerCableMissing`, tam 10-parça/üç-kablo route `Success` döndürür. Validation ayrıca exact current electrical/power/driver lineage'ını bağımsız doğrular.
- Exact same operation + exact command replay aynı `PcValidationReceipt` instance'ını döndürür. Changed reuse `OperationConflict` olur. Farklı stable operation ID ile aynı exact current contextte kontrollü rerun mümkündür; revision ve history monotonik kalır.
- Receipt history bütünlük fold'u replay lookup'tan önce çalışır. History veya operation dictionary bozulmuşsa replay dahil bütün command yolları `ReceiptHistoryInvalid` ile fail-closed olur; corrupted historical receipt meşru replay gibi kullanılamaz.
- Power-off history'yi silmez fakat current evaluation `NotCurrent` olur. Yeni power cycle, POST ve firmware baseline sonrasında yeni explicit validation run gerekir; eski receipt immutable historical evidence olarak kalır.
- Existing station, focus anchor, range, LOS, empty-hands, ownership ve Input System action'ları tekrar kullanılır. İlk `LMB / RT` review açar, ikinci `LMB / RT` validation run'ı tamamlar. Yeni scene geometry, collider, camera, modal cursor veya input action yoktur.
- `E / A` explicit power-off strict priority taşır. Same-frame Interact + Primary Action yalnız power-off yürütür ve validation receipt üretmez. Malformed validation history normal power-off yolunu softlock edemez.
- Açık validation review; motor pause, raw Pause edge, range/focus/LOS kaybı, busy hands, competing world-interact owner, driver/firmware/power revision veya exact current context değişimiyle Primary edge'i tüketmeden sıfırlanır. Dönüşte fresh review gerekir.
- Workbench observer-only `Waiting / Reviewing / Passed / Rejected / NotCurrent` presentation state'lerini, exact failure code'u ve score/stress/thermal/power/quality satırlarını gösterir. `TryGetValidation` kullanır; observation sırasında authority yaratmaz.
- Validation completion veya rejection Inventory, reservations, BuildKit, custody, Assembly, cable route, power transition, POST, firmware, OS, driver, Economy veya customer state'ini değiştirmez.
- GarageGraybox marker `garage-driver-bound-validation-r66-v1` ve native flag `-pse-validation-smoke` kullanır. Smoke, prerequisite driver zincirini nested assisted setup ile kurar; keyboard + mouse + virtual gamepad review/run/power-off yolunu, immutable replay'i, current-after-power-off false/history preserved semantiğini ve upstream no-mutation invariantlarını tek üst markerda doğrular.

## Consequences

GarageGraybox artık exact current fictional driver ve energized cycle sonrasında oyuncuya bounded fakat gerçek authority kullanan görünür validation workflow'u sunar. Aynı exact source/profile Mac ve Windows üzerinde aynı integer result'ı vermek zorundadır. Rerun history'si denetlenebilir, replay same-instance ve corrupted history fail-closed'dur.

Bu bounded sonuç ayrıntılı fault diagnosis/repair, gerçek sensor/telemetry, fan/airflow/noise, throttle/overclock, hardware damage, customer-use-case quality matching, save/load, Guardian, packaging, delivery, warranty veya final settlement değildir. Sıradaki ayrı ürün dilimi validation receipt'ini tüketen görünür fault/quality review ve servis kararı zinciridir.

## Current verification

- Technical source `f082ef5df913ce6a4664cdda5eb64d1b26f007d6`, tree `c387100c6dd7e314768756ebfb78104f6557081d`; draft PR #136 açıktır. Issue #135 ve Roadmap kartı `OPEN / In Progress` kalır.
- Accepted targeted catalog `5/5`, validation authority/history `125/125`, keyboard/mouse/gamepad/context/P0 `6/6`, scene/r66 `12/12`, power→POST→UEFI→OS→driver→validation regression `29/29`; final full EditMode `804/804`, full PlayMode `187/187`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode build `330,709,325` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `0e5bbb99a8eef26e6d121660788c5bec6c3de3c667725defb7e4f8b388a7672f`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal 1280×720 graphical r66 native smoke readiness ve exact validation success markerlarını birer kez üretir; exit `0`, player/Unity residue `0`dır.
- Local Repository Guard `REPOSITORY_GUARD_OK unity=6000.3.21f1 legacy=26 project_bible_docs=12 tracked=1268`, `git diff --check` ve draft PR #136 Repository Guard run `33389640619` geçer.
- `ProjectSettings/ProjectSettings.asset` SHA-256 `b1b99a75273d4a1c7737da9cb5ab4fa8e0fc5a414b367c3506078584aeca0244` olarak byte-exact kaldı. User/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında korundu.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.
