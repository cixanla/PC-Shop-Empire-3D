# ADR-0067 — Current-Preflight-Bound Safe Power-State Interlock

**Status:** Mac teknik source `01b89e2`, tree `bc1e5a8` için kabul edildi; fiziksel Windows ve USB kapıları ertelendi; draft PR #126 entegrasyon kaydıdır<br>
**Date:** 31 August 2026<br>
**Scope:** Issue #125; Issue #119 exact electrical readiness, Issue #121 exact power budget ve Issue #123 player-triggered preflight üzerine kuruludur

## Context

Issue #123 oyuncunun exact current Assembly, üç güç kablosu ve `380 W / 500 W / 550 W` bütçe lineage'ı için bir preflight istediğini immutable receipt ile kanıtlar. Bu receipt PC'yi enerjilendirmez. Bir sonraki bounded ürün adımı, aynı fiziksel Workbench üzerinden benign bir `Off → Energized → Off` döngüsü kurmalı; fakat Energized durumu POST, görüntü, BIOS/UEFI, işletim sistemi, driver, benchmark, termal kararlılık, elektrik arızası veya hasar anlamına gelmemelidir.

Power-on ile Assembly bakım komutları aynı anda serbest kalırsa routed kablo, retained component, fastener ve authority lineage'ı elektrik açıkken değişebilir. Bu yüzden bakım kilidi presentation boolean'ına değil, exact Assembly'ye bağlı tek power-state authority'ye dayanmalıdır. Replay de güvenli kalmalıdır: daha önce kabul edilmiş exact command, güç açıkken dahi aynı receipt instance'ını döndürmeli; yeni maintenance mutation ise ilk state değişikliğinden önce bloklanmalıdır.

## Decision

- Ayrı `PcPowerStateAuthority`, exact `PowerTestAttemptAuthority` ve `AssemblyBuildAuthority` instance'larına reference-bound oluşturulur. Null, foreign, mismatched veya ikinci binding fail-closed `OperationResult` döndürür; desteklenen failure `.Value` exception'ına çevrilmez.
- Başlangıç state'i `Off` ve revision `0`dır. Power-on; non-empty stable operation ID, exact expected revision ve `EvaluateCurrentReceipt()` ile yeniden doğrulanan exact current preflight receipt'i ister.
- Accepted power-on receipt; operation/expected/resulting revision, transition kind, exact preflight receipt identity ve exact Assembly/cable/budget lineage'ını immutable taşır. Exact replay aynı receipt instance'ını döndürür; changed reuse `OperationConflict` olur.
- Power-off ayrı stable operation ve receipt'tir. Exact active power-on receipt ve current revision ister; authority'yi deterministik olarak `Off` durumuna döndürür. Değişmeyen preflight birden fazla açık/kapalı döngüsünde yeniden kullanılabilir.
- `AssemblyBuildAuthority` yalnız bir power-state owner bağlar ve `IsElectricallyEnergized` durumunu ondan kabul eder. Bir central maintenance interlock bütün player-reachable remove, detach, unsecure/unretain ve ATX24/EPS12V/PCIe-GPU unroute validator'larına uygulanır.
- Historical exact replay, maintenance interlock'tan önce çözülür. Böylece accepted replay identity'si korunur; distinct maintenance command güç açıkken `AssemblyFailures.ElectricalPowerOnMaintenanceBlocked` ile domain/world mutation'dan önce durur.
- Existing `ElectricalPowerTestStationProjection` aynı focus/LOS/range/busy/competing-owner/single-consumer Interact yolunu kullanır. Current preflight yoksa preflight, current preflight + Off ise power-on, Energized ise explicit power-off çalışır.
- Prompt, readiness ve gate sorguları optional attempt/power-state authority yaratmaz. Creation yalnız gated Interact başarıyla consume edildikten sonra açık result-bearing `Ensure...Authority()` komutlarında yapılır. Readiness projection gerçek energized state'i early error yollarında yanlışlıkla Off'a sıfırlamaz.
- Existing Workbench status/focus surface yeniden kullanılır. `GÜÇ AÇIK • POST BEKLİYOR` ve `BAKIM KİLİDİ AKTİF` görünür; yeni gameplay collider, renderer, light, camera, NavMesh obstacle, physical item veya ikinci Assembly authority eklenmez.
- Keyboard/mouse ve Input System gamepad South aynı command yoludur. Concurrent same-frame keyboard+gamepad press tek transition üretir; paused power-off input'u state değiştirmez veya tüketmez.
- Native r61 smoke, preflight ve power on/off'u gerçek player input adapter yolundan geçirir; energized durumdayken routed PCIe/GPU kablosunu `PlayerCarryController.TryPickup` yoluyla sökmeyi dener ve physical ownership/route ile bütün revisions/receipt counts'ın değişmediğini doğrular.
- Smoke flag conflict native player'da exit `1` üretir. Guard, root ve nested coroutine'leri stack üzerinden çalıştırır; exception'da failure marker/quit verir ve external stop/dispose dâhil her çıkışta nested `finally` cleanup'larını dispose eder.
- POST, display output, firmware, OS, driver, benchmark, fault/damage, packaging, delivery ve settlement bu ADR'nin dışında kalır. Existing benchmark readiness `BuildIncomplete` kalır.

## Consequences

GarageGraybox r61, exact current preflight sonrasında oyuncuya gerçek bir güç açma/kapama döngüsü verir. Power açıkken visible status açıkça POST'un başlamadığını söyler ve bütün live Assembly bakım yolları mutation öncesi kapanır. Power kapatılınca aynı physical item, reservation, custody, Assembly/cable revisions ve historical receipts korunarak bakım tekrar açılır.

Authority creation ile presentation observation ayrıldığı için yalnız HUD okumak gameplay state'i değiştirmez. Supported composition failures exception olarak kaçmaz. Power-state receipt history ve Assembly'nin energized bit'i karşılıklı invariant ile denetlenir; ikinci power-state authority veya stale presentation safety kaynağı oluşmaz.

## Current verification

- Technical source `01b89e21e4329489b9a3c666edf5391710eb9c2f`, tree `bc1e5a8ec2e9852dd6d0b32c08b514bbd2c224a4`; draft PR #126 mergeable/open'dır. Issue/Roadmap açık ve In Progress kalır.
- Repository Guard run `33361533350` source commit üzerinde geçti.
- İki bounded read-only review turu; conflicting smoke flags, supported-failure exception escape, presentation-side authority creation/state reset, player-path interlock coverage ve coroutine cleanup/disposal alanlarında P1/P2 bulgular verdi. Hepsi final verification zincirinden önce düzeltildi; son review başka somut P0/P1/P2 bulmadı.
- Exact-commit targeted EditMode `6/6`, targeted PlayMode `4/4`; full Mac EditMode `778/778`, PlayMode `164/164`; failed, skipped ve inconclusive `0`.
- Universal macOS Development build `330,540,613` bayt ve `302` dosyadır. Executable `117,179` bayt, SHA-256 `cd5643fbe7e455ca049ae29350a8847b984bf8a040efbdea419b42a32c989e26`, deep/strict-valid universal Mach-O `x86_64 + arm64`dır.
- Apple M1/Metal native r61 smoke exact success marker'ını bir kez üretir; player exit `0`, Input System shutdown tamam, player/Unity/shader/IL2CPP residue `0`dır. Runtime log SHA-256 `408b778e18679e6fbe224103e508d8cac40022a4569b5ba14352dfd36585c17e`.
- Build'in ürettiği `ProjectSettings.asset` preloaded-assets hunk'ı kanıtla kaldırıldı. Ayrı user/editor-owned ProBuilder setting SHA-256 `20e33f89c50cf395e10b9ec90ba16b027561a87de80917ae86baaa92fcea001b` olarak unstaged ve commit dışında kaldı.
- Fiziksel Windows x64 IL2CPP/only-D3D11/Intel Iris Xe, physical-human HID/endurance ve USB checkpoint/readback yoktur; geçmediği hâlde geçmiş sayılmaz. UTM fiziksel Windows kanıtı değildir.
