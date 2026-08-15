# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #42 authoritative shelf offer/fiyat etiketi feature'ı tamamlandı; Epic #8 müşteri rezervasyon/checkout alt işleriyle devam ediyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #42 / Epic #8

- Feature commit `7a23cd92be6ff1169ff49530319b0759965cadf5`, tree `623c2f52839847c098162371bb6f7c1073f4852d`.
- Unity bağımsız `PSE.Retail`, stable offer/product/shelf kimliği ve iki ondalıklı pozitif integer minor-unit fiyatın tek authority'sidir.
- Currency tam üç büyük ASCII harftir; price float/double kullanmaz ve bounded `long` minor-unit değeridir.
- Exact aynı `SetOffer` idempotent başarıdır; fiyat update'i tek offer/authority revision üretir. Bütün validation failure yolları no-mutation kalır.
- Inventory yalnız exact container'ın `Shelf` olduğunu doğrular; quantity/world projection fiyat state'i değildir.
- RAF A ürünü fiyatlanmamışken `E / Gamepad South` etkin binding prompt'uyla `549,99 EUR` teklifini kasıtlı yayınlar. Etiket yalnız başarıdan sonra `FİYAT YOK` → `549,99 EUR` değişir.
- Publish exact item'ı Shelf/world konumunda bırakır; Inventory quantity/revision ve Orders revision değişmez.
- EditMode `207/207`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal `shelf-offer=ok` runtime smoke geçti.
- Karar: `Docs/ADR-0020-AUTHORITATIVE-SHELF-OFFER-PRICE.md`; kanıt: `Docs/Evidence/AUTHORITATIVE-SHELF-OFFER-CHECKPOINT-2026-08-15.md`.
- Sayısal fiyat düzenleme UI'si, müşteri/sepet/checkout, transaction snapshot, vergi/indirim, ledger, save ve final sanat sonraki bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → gerektiğinde ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core: stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministik event dispatcher tamam.
- Catalog/Inventory/Orders/Retail: immutable ürün, authoritative container/transfer, exact purchase-order receiving ve shelf offer authority tamam.
- Explicit Presentation adaptörü: Arrived → acceptance/Receiving → parcel open → ActorHands → Shelf/WorldFloor → offer publish zinciri tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; marker `garage-shelf-offer-r11-v1`.
- Kapalı dış parcel, görünür perakende ürün kutusu ve dünyada kalan açık parcel kabuğu ayrı projection durumlarıdır.
- Küçük kutu `Mouse Left / RT` placement, `R / Right Shoulder` 90° rotation ve `G / Gamepad East` placement/drop kullanır; full support/overlap/stack/zone kontrolleri korunur.
- Büyük kutu ayrı carry profili; platform arabası tek LargeBox hands→cart→hands ve fail-closed recovery kullanır.
- Tek slot, stable item ID, physics snapshot, domain-first transfer/rollback ve world-floor recovery korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `7a23cd92be6ff1169ff49530319b0759965cadf5`
- Tree: `623c2f52839847c098162371bb6f7c1073f4852d`
- USB snapshot source/docs checkpoint commit: `6ae294ea97571921d1296b72ab86e458235f9c22`.
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#42](https://github.com/cixanla/PC-Shop-Empire-3D/issues/42)
- Repository Guard: [31866681324](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31866681324), başarılı.

## Test, build ve runtime kanıtı

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `shelf-offer-scene-build.log` | GarageGraybox r11 builder kapısı geçti | `9257a9dc11c9425c0441fb5a0b6c0ed81b4b445ca92a3abab9cc51b7dd15bbfc` |
| `shelf-offer-editmode.xml` | 207/207 geçti; failed/skipped 0 | `6b00fcd3ff3e12bf89a02b9a4e2a4b02a6fb954fafe70cb0953bbee5ae64bfd6` |
| `shelf-offer-playmode.xml` | 17/17 geçti; failed/skipped 0 | `61dcd25d269d85deb9e41712b40015cbfe4c9e10561f44df575c4bc16564065d` |
| `shelf-offer-macos-build.log` | Universal development build, 327.511.689 bayt | `8663033c052da2f3129be3160c4ef330edcb2410ec8c832d957e510f601223bc` |
| Player executable | Mach-O `x86_64 + arm64` | `517be2d1584c85a46570a948d781fb32d860edb8664d306b5b9bcbeafee792d3` |
| `shelf-offer-macos-runtime.log` | Apple M4/Metal 1280×720; `accepted=ok parcel-open=ok carry=ok world-floor=ok shelf-offer=ok price-minor=54999 currency=EUR stable=ok quantity=1` | `45b124ad8f314ca98ebab631982ce993f485d69c0ac2c4469087afd804ee95f0` |

Klavye ve gamepad acceptance→open→pickup→RAF A placement→offer publish zincirini gerçek device state ile doğrular. Publish item/world ownership'ini veya Inventory/Orders revision'ını değiştirmez. Kilitli macOS oturumu nedeniyle yeni pencere ekran görüntüsü yoktur; sahne, test, build ve native runtime log kanıtı başarılıdır.

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

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_AUTHORITATIVE_SHELF_OFFER`, source/docs checkpoint `6ae294ea97571921d1296b72ab86e458235f9c22` taşır. İçerik 488 tracked kaynak + 5 scene/test/build/runtime kanıtı + source kaydıdır; 494 manifest satırı ve manifest SHA-256 `a95d8457e2b52732a35c28d49fa51dfd4981ca0af2b585947a20743e8e10de7a` tam readback/source checksum ile doğrulandı. Hash/boyut mismatch, source path/checksum mismatch, forbidden dir, credential filename ve AppleDouble `0`; büyük `.app`, cache ve credential dışarıdadır. USB güvenle çıkarılabilir.

## Sıradaki bounded paket

1. Epic #8 altında müşteri/sepet talebi ile serialized item rezervasyonu arasındaki Unity-bağımsız bounded retail sözleşmesini kur.
2. Aynı item'ın iki müşteri/sepet tarafından rezervasyonunu engelle; bütün failure yollarında Retail/Inventory revision no-mutation kalsın.
3. Teklif fiyatını checkout başlangıcında immutable snapshot olarak donduracak bir sonraki atomik sınırı açık tut.
4. Fiziksel müşteri AI, Economy ledger, vergi/indirim, Save/Guardian ve final UI'ı bu ilk rezervasyon paketine ekleme.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır.

## Güvenli devam komutu

> Authoritative RAF A shelf-offer checkpointinden devam et. Önce yaşayan belgeleri, temiz `origin/main` eşitliğini ve Epic #8'i doğrula. Sıradaki bounded paket müşteri/sepet talebi ile serialized stok rezervasyonu arasındaki Unity-bağımsız retail sözleşmesidir. Offer fiyat snapshot'ı, ledger ve müşteri AI ayrı kalsın. Test, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.
