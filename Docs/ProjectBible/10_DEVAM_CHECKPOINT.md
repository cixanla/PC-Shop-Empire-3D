# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #43 customer basket serialized reservation feature'ı tamamlandı; Epic #8 checkout transaction alt işiyle devam ediyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #43 / Epic #8

- Feature commit `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce`, tree `788e9a016a692a9e558d8fb3903e32830b3a8b08`.
- Unity bağımsız `RetailBasketAuthority`, stable customer/basket/line kimliklerini exact shelf offer, serialized item ve Inventory claim/reservation ile bağlar.
- Reserve yalnız item offer ürünüyle eşleşiyor ve exact offer Shelf container'ında duruyorsa başarılıdır. Aynı serialized item ikinci müşteri/sepet için ayrılamaz.
- Başarılı reserve Retail basket ve Inventory revision'ını birer kez ilerletir; exact tekrar cross-authority durum tutarlıysa idempotenttir. Bütün validation/conflict/drift failure yolları no-mutation kalır.
- Reservation available quantity'yi `1 → 0` yapar, total quantity `1` kalır. Release iki authority'yi birer kez ilerletir ve available quantity'yi `1`e döndürür.
- RAF A ürünü fiyatlandıktan sonra `G / Gamepad East` demo müşteri için ayırır; etiket/pano `1 ÜRÜN • AYRILDI` gösterir. Ayrılmış ürün `E / Gamepad South` pickup'a fail-closed yanıt verir; aynı `G / East` rezervasyonu kaldırır.
- Basket satırı fiyat snapshot'ı taşımaz; immutable checkout snapshot ve reservation consume sonraki atomik sınırdır.
- EditMode `220/220`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal `basket-reservation=ok release=ok` runtime smoke geçti.
- Karar: `Docs/ADR-0021-CUSTOMER-BASKET-SERIALIZED-RESERVATION.md`; kanıt: `Docs/Evidence/CUSTOMER-BASKET-RESERVATION-CHECKPOINT-2026-08-15.md`.
- Fiziksel müşteri AI/gerçek sepet transferi, checkout/ödeme, transaction snapshot, vergi/indirim, ledger, save ve final sanat sonraki bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → gerektiğinde ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core: stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministik event dispatcher tamam.
- Catalog/Inventory/Orders/Retail: immutable ürün, authoritative container/transfer/reservation, exact purchase-order receiving, shelf offer ve customer basket authority tamam.
- Explicit Presentation adaptörü: Arrived → acceptance/Receiving → parcel open → ActorHands → Shelf/WorldFloor → offer publish → customer reserve/release zinciri tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; marker `garage-customer-reservation-r12-v1`.
- Kapalı dış parcel, görünür perakende ürün kutusu ve dünyada kalan açık parcel kabuğu ayrı projection durumlarıdır.
- Küçük kutu `Mouse Left / RT` placement, `R / Right Shoulder` 90° rotation ve `G / Gamepad East` placement/drop kullanır; full support/overlap/stack/zone kontrolleri korunur.
- Büyük kutu ayrı carry profili; platform arabası tek LargeBox hands→cart→hands ve fail-closed recovery kullanır.
- Tek slot, stable item ID, physics snapshot, domain-first transfer/rollback ve world-floor recovery korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce`
- Tree: `788e9a016a692a9e558d8fb3903e32830b3a8b08`
- USB snapshot source/docs checkpoint commit: `109237a1d862c3a43be8b13ec4756fb0f4bf45a1`.
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#43](https://github.com/cixanla/PC-Shop-Empire-3D/issues/43)
- Repository Guard: [31867913964](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31867913964), başarılı.
- Docs Repository Guard: [31868148943](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31868148943), başarılı.

## Test, build ve runtime kanıtı

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `Assets/Scenes/Prototypes/GarageGraybox.unity` | r12 görünür shelf/customer label sözleşmesi | `1e945021980d17d000d223afd65c29ae125fa679f13bc591d250c4bf27e4582f` |
| `editmode-issue43.xml` | 220/220 geçti; failed/skipped 0 | `e7445a284e829861cc57675d57d8404500f85b741b49198bff82467f32edce71` |
| `playmode-issue43.xml` | 17/17 geçti; failed/skipped 0 | `3f78fa76c41ec2efbfba5b0e4f26401958b3429f9a1079ab4b09f65d68d40674` |
| `build-issue43.log` | Universal development build, 327.531.969 bayt | `971c6e941d64e38a12c826b41f4f6220ebcfeedf982ffaa323e2acb5fdb1e1f0` |
| Player executable | Mach-O `x86_64 + arm64` | `98a7d104383137bc74099f214d30923d13e3bd9d05e90d922524a9d4350d1add` |
| `basket-reservation-macos-runtime.log` | Apple M4/Metal 1280×720; `basket-reservation=ok release=ok` | `4a98c8937d034b38a4e84e7fb2c3572e45af8c8a84ae2c573d0764087d78f6ba` |

Klavye ve gamepad acceptance→open→pickup→RAF A placement→offer publish→customer reserve/release zincirini gerçek device state ile doğrular. Reserved pickup fail-closed, exact repeat idempotent ve bütün failure yolları no-mutation kalır. Kilitli macOS oturumu nedeniyle yeni pencere ekran görüntüsü yoktur; sahne, test, build ve native runtime log kanıtı başarılıdır.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core/time/event ve RNG/dispatcher: `8af2ad3`, `bbb3648`, `43e9217`, `3d819e5`.
- İlk oynanabilir garaj ve fiziksel akış: `c7a3a26`, `44b8162`, `720e6d4`, `e944198`, `661f2dc`.
- Lookdev, stacking ve loaded cart: `c7214af`, `2e11e30`, `82bf74f`.
- Catalog + Inventory: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff`.
- Atomic purchase-order receiving: `e596e079d90b6d5b9d94714d7821502574eba3c9`.
- Authoritative stock-flow projection: `9d75573a86e395d2fa74f3808d43310e4d65f760`.
- Idempotent delivery parcel reveal: `3766f3f06df624093f4774ef8fa4e7f1286d1c01`.
- Authoritative shelf offer ve RAF A etiketi: `7a23cd92be6ff1169ff49530319b0759965cadf5`.
- Customer basket serialized reservation: `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce`.

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_CUSTOMER_BASKET_RESERVATION`, source/docs checkpoint `109237a1d862c3a43be8b13ec4756fb0f4bf45a1` taşır. İçerik 498 tracked kaynak + 4 test/build/runtime kanıtı + source kaydıdır; 503 manifest satırı ve manifest SHA-256 `ff868e4c999a11c994719fa5cd3695abb63ab4f6cdb20de26510a4819b0d20d7` tam readback/source checksum ile doğrulandı. Hash/boyut mismatch, source path/checksum mismatch, forbidden dir, credential filename/kalıbı ve AppleDouble `0`; büyük `.app`, cache ve credential dışarıdadır. USB güvenle çıkarılabilir.

## Sıradaki bounded paket

1. Epic #8 altında checkout başlangıcında basket line + exact offer + Inventory reservation'ı doğrulayan Unity-bağımsız transaction sözleşmesini kur.
2. Offer fiyatını integer minor-unit immutable snapshot olarak dondur; checkout başladıktan sonraki raf fiyatı değişikliği transaction sonucunu değiştirmesin.
3. Exact tekrar idempotent; stale/missing reservation, drift, unknown line/offer ve invalid state bütün authority'lerde no-mutation kalsın.
4. İlk checkout paketinde ödeme/ledger/vergi/indirim, fiziksel müşteri AI, Save/Guardian ve final UI ekleme; reservation consume/sale commit sınırını ayrıca belirt.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır.

## Güvenli devam komutu

> Customer basket serialized reservation checkpointinden devam et. Önce yaşayan belgeleri, temiz `origin/main` eşitliğini ve Epic #8'i doğrula. Sıradaki bounded paket checkout başlangıcında basket line + exact offer + Inventory reservation'ı doğrulayıp integer fiyatı immutable snapshot olarak donduran Unity-bağımsız transaction sözleşmesidir. Ödeme/ledger, müşteri AI ve Save ayrı kalsın. Test, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.
