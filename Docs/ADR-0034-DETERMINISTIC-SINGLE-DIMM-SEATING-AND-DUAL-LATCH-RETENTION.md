# ADR-0034 — Deterministic Single DIMM Seating and Dual-Latch Retention

**Tarih:** 16 Ağustos 2026<br>
**Durum:** Kabul edildi; kaynak, test, Universal macOS build, Apple M4/Metal native runtime ve feature Repository Guard kapıları tamamlandı; source/docs, Issue/Project ve USB metadata kapanışı sıradadır<br>
**Bağlam:** Epic #10 / Issue #56 — tek DDR5 UDIMM için bounded A2 slot ve dual-latch retention dilimi

## Bağlam

Issue #53–#55 tek serialized anakartı açık kasaya oturtup sabitledi ve tek canonical CPU'yu yönlü socket/retention akışıyla tamamladı. Sonraki küçük dilim, bütün RAM population algoritmasını veya genel Inventory modelini büyütmeden tek canonical DDR5 UDIMM'i doğru A2 slotuna yönlü biçimde oturtmalı, iki görünür mandalı tek Assembly-owned retention aggregate'i olarak deterministik sırada işletmeli ve aynı fiziksel kimlikle söküp recovery yapmalıydı.

## Karar

- Catalog'a append-only `MemoryModule` component kind ve typed `Ddr5Udimm` metadata'sı eklenir. Garajda yalnız tek canonical serialized DIMM item/projection vardır.
- Topology immutable ve tekildir: stable A2 slot, Channel A, Bank 2, population priority 1. İkinci slot, dual-channel population veya timing/XMP/ECC bu kararın parçası değildir.
- Inventory; Workbench, ProcessorSocket ve capacity-1 MemorySlot container'larını tek all-or-none triple claim ile Assembly planına devreder. Başarı tek Inventory revision üretir; conflict, overflow veya üçüncü-container failure partial/ghost management bırakmaz. Public raw transfer managed hedeflere kapalıdır.
- Assembly memory state'i yalnız `Unsupported`, `EmptyOpen`, `MemoryModuleSeatedOpen` veya `MemoryModuleRetained` olabilir. Bounded akış `EmptyOpen → MemoryModuleSeatedOpen → MemoryModuleRetained → MemoryModuleSeatedOpen → EmptyOpen`dır.
- Seat, close, open ve remove exact build/chassis/motherboard slot/memory slot/retention/item/product ile attach, secure, seat ve retention source lineage'ını doğrular. Başarılı her işlem bir Assembly revision ve immutable receipt üretir; immediate ve delayed exact replay aynı receipt referansını döndürür.
- Seat ve retention close yalnız secured motherboard üzerinde kabul edilir. DIMM installed iken motherboard detach `assembly.memory-module-installed` ile fail-closed kilitlenir. Retained DIMM açılabilir; yalnız seated-open DIMM çıkarılabilir.
- Close/open Inventory custody veya revision değiştirmez. Seat ActorHands→MemorySlot, remove MemorySlot→ActorHands transferidir. Full hands, wrong identity/type/state/orientation, occupied slot, stale/conflict/overflow ve projection failure bütün authority'lerde no-mutation kapanır.
- Oyuncu DIMM'i `E / Gamepad South` ile alır; `Mouse Left / Gamepad RT` guided mode'u açar; `R / Right Shoulder` yalnız fiziksel keyed `0° ↔ 180°` orientationları arasında geçer; `G / Gamepad East` oturtur. Boş elde fresh Primary sol mandal→sağ mandal sırasıyla kapatır, ters sırayla açar.
- `DimmSlotSolver` pause, authority, range, focus, LOS, keyed orientation, overlap ve swept insertion obstruction kapılarını sabit `NonAlloc` sorgularla fail-closed değerlendirir. Buffer saturation ve near-hit tie deterministiktir. Player input yalnız 0/2 üretse de solver doğrudan verilen 1/3 quarter-turn değerlerini adversarial olarak reddeder.
- İki görünür mandal ayrı pivot/animasyon taşır fakat tek retention operation/revision/receipt üretir. Animasyon fazı logical authority state'ini değiştirmez; strict stable projection ve phase-aware invariant kontrolleri ayrıdır.
- Guided mode kapalıyken ghost veya DIMM seat physics query çalışmaz. Compact HUD son aktif cihaz ailesinin gerçek keyboard/gamepad bindinglerini gösterir ve aynı-frame co-edge/pause edge'leri tek consumer tarafından drain edilir.
- DIMM/slot presentation authority değildir. r25 sahne; gerçek orana yakın PCB, chip, heat-spreader, notch, slot rail ve iki latch taşır. Recovery aynı Unity instance, ItemId/ProductId, parent, authored loose pose, Rigidbody/safe pose ve tek canonical projection sayısını korur.

## Sonuçlar

- Oyuncu görünür anakart üzerinde DIMM'in anahtar yönüne dikkat ederek modülü A2 slotuna oturtabilir, iki mandalı okunur sırayla kapatıp açabilir ve kayıp/duplicate/titreme üretmeden sökebilir.
- Inventory custody, Assembly receipt lineage ve sahnedeki DIMM/latch pozu birbirini doğrular. Presentation drift authority'yi değiştirmez ve invariantı fail-closed bozar.
- Benchmark readiness artık memory missing ile memory unretained nedenlerini ayırır; CPU+RAM tamamlanmış olsa da bounded prototip tam PC benchmarkı iddia etmez.
- Orders, Retail, Economy, customer authority'leri ve canonical processor state/custody runtime smoke boyunca başlangıç–final snapshotlarıyla izole kalır.

## Bilinçli kapsam dışı

- İkinci DIMM/slot, dual-channel population, kapasite/timing/XMP/ECC performans modeli.
- GPU, storage, PSU, cooler, thermal paste, cable routing, POST/BIOS/OS/driver ve benchmark score.
- Genel Inventory revision-max refactor'u, Save/journal/Guardian mutation, final el animasyonu/ses/VFX/UI ve final licensed art.
- Native Windows x64/IL2CPP/DirectX/Steam doğrulaması; Epic #21 dış platform kapısıdır.

## Yerel doğrulama

- EditMode `461/461`; gerçek Input System PlayMode `33/33`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode Player; Mach-O `x86_64 + arm64`; Unity build raporu `328268700` bayt.
- Apple M4/Metal, 1280×720 native runtime; `garage-dimm-dual-latch-r25-v1` readiness ve exact DIMM smoke başarılı.
- Feature commit `7482fc9aabe6a3a27ba41730db12c60e18aac515`, tree `291b23cb2fe774cb44ba71b26716d7c8131370a2`; [Repository Guard 31919985055](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31919985055) başarılıdır.
- Ayrıntılı boyut, SHA-256, marker ve kapanış durumu tarihli evidence/checkpoint belgelerinde tutulur.
