# ADR-0032 — Deterministic Motherboard Fastener Secure/Unsecure Gate

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Kabul edildi; kaynak, test, Universal macOS build, Apple M4/Metal native runtime, private push, Repository Guard ve Issue/Project `Done` kapıları tamamlandı; USB kullanıcı talimatıyla ertelendi<br>
**Bağlam:** Epic #10 / Issue #54 — tek motherboard fastener için bounded fiziksel assembly dilimi

## Bağlam

Issue #53 tek serialized `MicroAtx` anakartı açık kasadaki doğru slota `SeatedUnsecured` olarak oturtmuştu. Sonraki küçük dilim, Inventory authority'sini veya bütün PC build graph'ını genişletmeden bu anakartı görünür bir captive fastener ve screwdriver etkileşimiyle güvenli biçimde sabitlemeli; secured board'un yanlışlıkla sökülmesini engellemeliydi.

## Karar

- Fastener kimliği `assembly.fastener.motherboard-main-01` olarak `PSE.Assembly` aggregate'ına aittir. Ayrı Catalog ürünü, Inventory item'ı, container veya shadow authority değildir.
- Anakart seat durumu yalnız `Empty`, `SeatedUnsecured` veya `SeatedSecured` olabilir. Secure/unsecure komutları exact build/chassis/slot/item/product/fastener ve source attach/secure lineage'ını doğrular.
- Başarılı secure/unsecure işlemi bir Assembly revision ve immutable receipt üretir; Inventory custody, item kaydı ve Inventory revision değişmez. Exact operation replay aynı receipt referansını döndürür; historical replay daha yeni unsecure/detach/recovery generation'ını geri sarmaz.
- Wrong identity/state/lineage, stale expectation, conflict ve revision overflow bütün Assembly, Inventory ve world projection durumlarında no-mutation kapanır. Receipt-history invariantı Assembly revision sırasını ve Inventory revision monotonluğunu baştan sona fold eder.
- `SeatedSecured` anakart presentation pickup preflight'ında ve doğrudan Assembly detach authority'sinde `assembly.component-secured` ile reddedilir. Unsecure olmadan fiziksel parent/pose/ownership değişmez.
- `MotherboardFastenerSolver` pause, authority, aktif target, range, focus, LOS ve obstruction kapılarını tek `RaycastNonAlloc` sorgusuyla değerlendirir. 32-hit saturation fail-closed `Obstructed` olur; minimum mesafe indirgemesi geliş sırasından bağımsızdır ve `0,0001 m` eşitlik bandında obstruction kazanır.
- Screw head, screwdriver, status plate ve metin yalnız presentation'dır. Secured görünümünde vida başı authored local forward yönünde exact `4 mm` ilerler ve 90° döner. Transform/collider authority değildir; projection drift invariantı bozar fakat domain state'ini değiştirmez.
- Renk tek anlam taşıyıcısı değildir: physical plate ve compact HUD `[ ]`, `[O]`, `[OK]`, `[X]`/pause sembolü ile kısa Türkçe durum metni taşır. Büyük yüzen debug metni yoktur.
- `Mouse Left / Gamepad RT` focused fastener'ı secure/unsecure eder. `E / Gamepad South` secured durumda sökme kilidini bildirir. Valid veya blocked fastener context aynı frame'de Primary/Interact/Drop edge'lerinin tek sahibidir; cached evaluation yeniden raycast olmadan kullanılır. Pause co-edge bütün gameplay edge'lerini drain eder ve release–repress ister.
- `SeatedSecured` hâlâ tam PC değildir. Benchmark authority bu durumda `assembly.benchmark.build-incomplete` verir; CPU/RAM/GPU veya başarı skoru bu dilimden türetilmez.

## Sonuçlar

- Oyuncu garajdaki tek canonical anakartı fiziksel captive fastener üzerinden sıkıp gevşetebilir; yanlış sıra, engel, pause, held edge veya replay duplicate mutation oluşturmaz.
- Assembly receipt lineage'ı ile görünür vida/tornavida pozu birbirini doğrular; presentation bozulursa invariant fail-closed olur ve authoritative kayıt korunur.
- Mevcut pickup/drop/recovery, stacking/cart, stok, müşteri, checkout, Economy ve NavMesh akışları regresyon testlerinde korunur.

## Bilinçli kapsam dışı

- CPU/socket, RAM, GPU, storage, PSU, cooler, thermal paste, cable routing, POST/BIOS/OS/driver ve benchmark score.
- Çoklu vida, torque grind/simülasyonu, sökülebilir tool Inventory authority'si, tool durability ve hasar/ESD.
- Save/journal/Guardian mutation, final model/texture/animasyon/ses/UI ve gerçek Windows/DirectX/Steam/IL2CPP doğrulaması.
- Genel `InventoryAuthority.Revision == long.MaxValue` hardening'i ayrı bounded P1 backlog işidir; Issue #54 kapanış bağımlılığı değildir.

## Yerel doğrulama

- EditMode `411/411`; gerçek Input System PlayMode `29/29`; failed/skipped `0`.
- Universal macOS Development/StrictMode Player; Mach-O `x86_64 + arm64`.
- Apple M4/Metal, 1280×720 native runtime; `garage-motherboard-fastener-r23-v1` readiness ve güçlendirilmiş exact assembly smoke başarılı.
- Ayrıntılı boyut, SHA-256, marker, commit, GitHub ve USB durumu tarihli Issue #54 evidence/checkpoint belgesinde tutulur.
