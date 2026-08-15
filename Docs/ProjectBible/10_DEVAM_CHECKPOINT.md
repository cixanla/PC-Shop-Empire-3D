# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #44 immutable checkout price snapshot feature'ı tamamlandı; Epic #8 atomik satış commit alt işiyle devam ediyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #44 / Epic #8

- Feature commit `294999f6ad48d4831f56031cc542cf43cac09d3e`, tree `2f524430e2a3bf03ad3880ab29eb44a0b8120a25`.
- Unity bağımsız `RetailCheckoutAuthority`, stable checkout/basket/customer kimlikleri ve deterministic immutable line snapshot listesi taşır.
- Begin checkout, basket'ın bütün aktif satırlarını exact offer, serialized item, Inventory reservation/claim, product ve shelf kimlikleriyle mutation öncesi doğrular.
- Snapshot integer minor-unit unit price, tek currency, overflow-safe total ve source offer revision taşır. Başarı yalnız Checkout revision'ını bir kez ilerletir; Basket, Inventory, ShelfOffer ve Orders sabit kalır.
- Exact tekrar idempotenttir. Duplicate basket transaction, identity conflict, empty/unknown basket, mixed currency, missing/stale reservation ve cross-authority drift failure yolları no-mutation kalır.
- Checkout sonrasında raf fiyatı `549,99 → 599,99 EUR` güncellense bile açık checkout `549,99 EUR` snapshot'ını korur.
- Reserved RAF A ürününde `Mouse Left / Gamepad RT` checkout başlatır; shelf etiketi, HUD ve prompt `549,99 EUR • DONDURULDU` gösterir. Aktif checkout `G / East` release ve `E / South` pickup'ı fail-closed kilitler.
- EditMode `233/233`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal 1280×720 `checkout-snapshot=ok price-frozen=ok` runtime smoke geçti.
- Karar: `Docs/ADR-0022-IMMUTABLE-CHECKOUT-PRICE-SNAPSHOT.md`; kanıt: `Docs/Evidence/IMMUTABLE-CHECKOUT-PRICE-SNAPSHOT-CHECKPOINT-2026-08-15.md`.
- Reservation consume/sold transition, ödeme/Economy ledger, fiziksel müşteri AI, vergi/indirim, Save ve final UI sonraki bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`; Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministic event dispatcher tamam.
- Catalog/Inventory/Orders/Retail: immutable product, authoritative container/transfer/reservation, exact purchase-order receiving, shelf offer, customer basket ve immutable checkout snapshot authority'leri tamam.
- Explicit Presentation adaptörü: Arrived → acceptance/Receiving → parcel open → ActorHands → Shelf/WorldFloor → offer publish → customer reserve/release → checkout begin zinciri tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; marker `garage-checkout-snapshot-r13-v1`.
- Küçük kutu placement/rotation/stacking, büyük kutu carry, yüklü platform arabası, stable item ID, domain-first rollback ve recovery invariantları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `294999f6ad48d4831f56031cc542cf43cac09d3e`
- Tree: `2f524430e2a3bf03ad3880ab29eb44a0b8120a25`
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#44](https://github.com/cixanla/PC-Shop-Empire-3D/issues/44)
- Feature Repository Guard: [31869105555](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31869105555), başarılı.
- USB source/docs checkpoint commit: `0936cc00b9f06264061ebe31893e53b3e8af2950`.
- Docs Repository Guard: [31869313985](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31869313985), başarılı.
- Issue #44 Completed olarak kapatıldı ve Roadmap item'ı Done yapıldı; Epic #8 In Progress kaldı.

## Test, build ve runtime kanıtı

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r13 görünür checkout label/HUD/prompt sözleşmesi | `6764c560b793598eb3d89bf2016028d4b5f9cf2aaa0a707c62ddc1490b489aac` |
| `editmode-checkout44-integration.xml` | 233/233 geçti; failed/skipped 0 | `a8df3068a0680a494b8e957e28d4708c18b814686940549491ad50c3a51ac618` |
| `playmode-checkout44.xml` | 17/17 geçti; failed/skipped 0 | `b6b6a478efd599dcf1679277a233195a453b0457550fb37d68d7a4cc9586a679` |
| `build-checkout44-macos.log` | Universal development build, 327.551.161 bayt | `e83dfc95e56b3e8ca527b809671fdb2360b4ce2aa6d3454d0658a7b65bd568c6` |
| Player executable | Mach-O `x86_64 + arm64` | `1efa5d0dfe88e74fcae15570cb07c51c0d132baf28b30b15476721f2a7dddbdf` |
| `runtime-checkout44-macos-windowed.log` | Apple M4/Metal 1280×720; checkout/fiyat dondurma | `0ada440a9c02746921664f30e8cfd8f05f1d0ec7ac8024816317848ce3db86ff` |

Gerçek klavye/fare ve gamepad, bütün physical stock-flow zincirinden sonra reserve → checkout begin → dondurulmuş fiyat → aktif checkout release/pickup engelini doğrular. Kilitli macOS oturumu nedeniyle yeni ekran görüntüsü yoktur; sahne, test, build ve native runtime log kanıtı başarılıdır.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core/time/event ve RNG/dispatcher: `8af2ad3`, `bbb3648`, `43e9217`, `3d819e5`.
- Oynanabilir garaj, lookdev, stacking ve loaded cart: `c7a3a26`, `44b8162`, `720e6d4`, `e944198`, `661f2dc`, `c7214af`, `2e11e30`, `82bf74f`.
- Catalog + Inventory: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff`.
- Purchase-order receiving, stock projection ve parcel reveal: `e596e079`, `9d75573a`, `3766f3f0`.
- Shelf offer ve customer basket reservation: `7a23cd92`, `45c2cdc4`.
- Immutable checkout price snapshot: `294999f6ad48d4831f56031cc542cf43cac09d3e`.

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_IMMUTABLE_CHECKOUT_SNAPSHOT`, source/docs checkpoint `0936cc00b9f06264061ebe31893e53b3e8af2950` taşır. İçerik 508 tracked kaynak + 4 test/build/runtime kanıtı + source kaydıdır; 513 manifest satırı ve manifest SHA-256 `30c1e7fa3703bfb84ebf89f8b5b3ba3c6a7ad0be044a6e502434f55256616efa` tam readback/source checksum ile doğrulandı. Hash/boyut/source mismatch, forbidden dir, credential filename/kalıbı ve AppleDouble `0`; büyük `.app`, cache ve credential dışarıdadır. USB güvenle çıkarılabilir.

## Sıradaki bounded paket

1. Epic #8 altında exact checkout basket/reservation bağını yeniden preflight eden atomik completion sözleşmesini kur.
2. Başarıda reservation'ı bir kez consume et, basket satırını tamamla ve serialized item için stable sold/fulfilled sonucu üret; tekrar idempotent olsun.
3. Kısmi Retail/Inventory mutation'ını rollback veya mutation-before-preflight ile üretme; stale/drift/conflict yolları no-mutation kalsın.
4. Ödeme, Economy ledger/COGS/nakit, vergi/indirim, fiziksel müşteri AI, Save/Guardian ve final UI'ı ayrı authority/paket olarak tut.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır.

## Güvenli devam komutu

> Immutable checkout price snapshot checkpointinden devam et. Önce yaşayan belgeleri, temiz `origin/main` eşitliğini ve Epic #8'i doğrula. Sıradaki bounded paket exact checkout reservation'ını atomik tüketip serialized item için stable satış/fulfilled sonucu üretir. Ödeme/Economy ledger, müşteri AI ve Save ayrı kalsın; cross-authority failure no-mutation ve idempotency korunsun. Test, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.
