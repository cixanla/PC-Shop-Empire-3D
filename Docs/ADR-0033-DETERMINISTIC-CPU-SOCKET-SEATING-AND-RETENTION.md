# ADR-0033 — Deterministic CPU Socket Seating and Retention

**Tarih:** 16 Ağustos 2026<br>
**Durum:** Kabul edildi; kaynak, test, Universal macOS build, Apple M4/Metal native runtime ve feature Repository Guard tamamlandı; final source/docs ile Issue/Project kapanışı checkpoint akışında bağlanır; USB kullanıcı talimatıyla ertelendi<br>
**Bağlam:** Epic #10 / Issue #55 — tek CPU için bounded fiziksel socket ve retention dilimi

## Bağlam

Issue #53–#54 tek serialized `MicroAtx` anakartı açık kasaya oturtup Assembly-owned fastener ile güvenli biçimde sabitledi. Sonraki küçük dilim, bütün PC build graph'ını veya genel Inventory modelini büyütmeden tek canonical CPU'yu doğru sokete yönlü biçimde oturtmalı, retention mekanizmasıyla kilitlemeli, ters sırada sökmeli ve aynı fiziksel kimliği recovery boyunca korumalıydı.

## Karar

- CPU Catalog'da serialized `Processor` component kind ve açık socket family kimliği taşır. Garajda yalnız tek canonical CPU item/projection vardır; socket capacity `1`dir.
- Inventory, Workbench ve ProcessorSocket container'larını atomik pair claim ile Assembly planına devreder. İki claim tek revision üretir; conflict, overflow veya ikinci-container failure ilk container'ı ghost-managed bırakmaz. Public raw transfer iki managed hedefe de kapalıdır.
- Assembly processor state'i yalnız `Unsupported`, `EmptyOpen`, `ProcessorSeatedOpen` veya `ProcessorRetained` olabilir. Bounded akış `EmptyOpen → ProcessorSeatedOpen → ProcessorRetained → ProcessorSeatedOpen → EmptyOpen`dır.
- Seat, close, open ve remove exact build/chassis/motherboard slot/processor slot/retention/item/product ile attach, secure, seat ve retention source lineage'ını doğrular. Her başarılı işlem bir Assembly revision ve immutable receipt üretir; immediate ve delayed exact replay aynı receipt referansını döndürür, historical state'i geri sarmaz.
- Seat ve retention close yalnız `SeatedSecured` motherboard üzerinde kabul edilir. CPU takılıyken motherboard unsecure olabilir fakat detach `assembly.processor-installed` ile engellenir. Retained CPU unsecured host üzerinde açılabilir; seated-open CPU yeniden kapatılamaz fakat çıkarılabilir.
- Close/open Inventory custody veya revision değiştirmez. Seat, CPU'yu ActorHands→ProcessorSocket; remove ProcessorSocket→ActorHands taşır. Full hands, wrong identity/family/state/orientation, occupied socket, stale revision, conflict ve revision overflow bütün authority ve fiziksel projectionlarda no-mutation kapanır.
- `ProcessorSocketSolver` pause, authority, range, focus, LOS, keyed 90° orientation, overlap ve swept insertion obstruction kapılarını `NonAlloc` sorgularla fail-closed değerlendirir. Buffer saturation ve near-hit tie deterministik olarak obstruction seçer.
- Oyuncu CPU'yu `E / Gamepad South` ile alır; `Mouse Left / Gamepad RT` guided mode'u açar; `R / Right Shoulder` 90° döndürür; `G / Gamepad East` oturtur; boş elde LMB/RT retention kolunu işletir ve E/A seated-open CPU'yu çıkarır. Same-frame co-edge tek semantic transition üretir; pause bütün gameplay edge'lerini drain eder.
- Guided mode kapalıyken CPU ghost'u, placement-valid state'i veya seat PhysX sorgusu yoktur. Compact assembly HUD, elde CPU varken checkout/customer promptundan önce gerçek ilk input consumer'ın dinamik binding metnini gösterir.
- CPU/socket presentation authority değildir. r24 sahne tek renderer–iki submesh notched LGA-style package, ayrı PCB/IHS materyali, matching triangular key, aperture load plate ve lever kullanır. Hard-surface face normals/UV vardır; renderer/collider/text bütçesi `21/11/1` kalır.
- Recovery aynı Unity instance, ItemId/ProductId, parent, authored loose pose, Rigidbody/safe pose ve tek canonical projection sayısını korur. Secured-host socket recovery ve unsecured-host WorldFloor fallback ayrı authority sonuçlarıdır; duplicate/kayıp item kabul edilmez.

## Sonuçlar

- Oyuncu görünür anakart üzerindeki CPU'yu yönüne dikkat ederek fiziksel olarak oturtabilir, kolu kapatıp açabilir, güvenli sırayla çıkarabilir ve kayıp/titreme/duplicate üretmeden recovery yapabilir.
- Domain receipt lineage'ı ile Inventory custody ve sahnedeki cover/lever/CPU pozu birbirini doğrular; presentation drift authority'yi değiştirmez ve invariantı fail-closed bozar.
- Mevcut motherboard pickup/fastener, kutu, cart, stok, müşteri, checkout, Economy ve NavMesh akışları regresyon testlerinde korunur.

## Bilinçli kapsam dışı

- RAM/DIMM, GPU, storage, PSU, cooler, thermal paste, cable routing, POST/BIOS/OS/driver ve benchmark score.
- Pin/socket damage, bent-pin simülasyonu, ESD, torque/lever analog grind, tool Inventory authority'si, final el animasyonu/ses/VFX/UI.
- Genel Inventory authority genişlemesi veya revision-max hardening, Save/journal/Guardian mutation ve gerçek Windows/DirectX/Steam/IL2CPP doğrulaması.
- Mevcut r24 geometri okunaklı yarı-gerçekçi graybox/lookdev kanıtıdır; final üretim modeli veya gerçek marka kopyası değildir.

## Yerel doğrulama

- EditMode `430/430`; gerçek Input System PlayMode `31/31`; failed/skipped/inconclusive `0`.
- Universal macOS Development/StrictMode Player; Mach-O `x86_64 + arm64`.
- Apple M4/Metal, 1280×720 native runtime; `garage-cpu-socket-retention-r24-v1` readiness ve exact CPU socket smoke başarılı.
- Ayrıntılı boyut, SHA-256, marker, commit, GitHub ve USB durumu tarihli Issue #55 evidence/checkpoint belgesinde tutulur.
