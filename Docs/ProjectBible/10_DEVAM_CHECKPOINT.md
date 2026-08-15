# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #41 idempotent fiziksel koli açma tamamlandı; Epic #8 fiyat/etiket ve satış alt işleriyle devam ediyor<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #41 / Epic #8

- Feature commit `3766f3f06df624093f4774ef8fa4e7f1286d1c01`, tree `3b03406dc9e9d6cd9323261664735900fe6b1f83`.
- GarageGraybox dış teslimat kolisi ile exact Northstar A60 ürün kutusunu görünür ve davranışsal olarak ayırır.
- İlk `E / Gamepad South` acceptance yapar; item authoritative Receiving'de oluşur fakat koli kapalı kalır.
- İkinci `E / Gamepad South` accepted exact manifest/item/container sözleşmesini doğrular, ürünü görünür yapar ve açık dış koli kabuğunu Receiving'de bırakır.
- Opening Inventory quantity/revision veya Orders revision değiştirmez. Repeated open başarıyla idempotenttir; transition count `1`, duplicate world item `0`.
- Üçüncü `E / Gamepad South` aynı item'ı Receiving → ActorHands taşır; RAF A placement ve `G / Gamepad East` WorldFloor drop önceki transactional akışı korur.
- Invalid order state, binding identity ve Receiving dışı item durumları parcel'ı kapalı/no-mutation bırakır. Açılmamış item alınamaz.
- HUD, dünya panosu ve prompt acceptance → open → pickup durumunu dinamik klavye/gamepad binding'leriyle gösterir.
- EditMode `192/192`, gerçek Input System PlayMode `17/17`, Universal macOS build ve Apple M4/Metal runtime smoke geçti.
- Karar: `Docs/ADR-0019-IDEMPOTENT-DELIVERY-PARCEL-REVEAL.md`; kanıt: `Docs/Evidence/DELIVERY-PARCEL-UNPACKING-CHECKPOINT-2026-08-15.md`.
- Çoklu line/quantity unpack, claim, fiyat/para, müşteri satış, save ve final sanat sonraki bounded paketlerdir.

## Kullanım güvenliği protokolü

- Kullanıcı durdurana veya ortam kapanana kadar bağımlılık sırasındaki küçük paketler soru gerektirmeden sürdürülür.
- Her bounded paket kod/test → Git commit → private push → CI → yaşayan belgeler → gerektiğinde ayrı USB milestone sırasıyla kapanır.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.
- Kullanıcı değişikliği, token/credential, legacy canonical kaynak ve ilgisiz çalışma ağacı değişiklikleri korunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core: stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministik event dispatcher tamam.
- Catalog/Inventory/Orders: immutable ürün, authoritative container/transfer ve exact purchase-order receiving tamam.
- Explicit Presentation adaptörü: Arrived → acceptance/Receiving → parcel open → ActorHands → Shelf/WorldFloor zinciri tamam.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`; marker `garage-delivery-unpacking-r10-v1`.
- Kapalı dış parcel, görünür perakende ürün kutusu ve dünyada kalan açık parcel kabuğu ayrı projection durumlarıdır.
- Küçük kutu `Mouse Left / RT` placement, `R / Right Shoulder` 90° rotation ve `G / Gamepad East` placement/drop kullanır; full support/overlap/stack/zone kontrolleri korunur.
- Büyük kutu ayrı carry profili; platform arabası tek LargeBox hands→cart→hands ve fail-closed recovery kullanır.
- Tek slot, stable item ID, physics snapshot, domain-first transfer/rollback ve world-floor recovery korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `3766f3f06df624093f4774ef8fa4e7f1286d1c01`
- Tree: `3b03406dc9e9d6cd9323261664735900fe6b1f83`
- USB snapshot source/docs checkpoint commit: `756547f42298b7aeaf01075cbf6d000cbb97ddaa`.
- Epic/issue: [#8](https://github.com/cixanla/PC-Shop-Empire-3D/issues/8) / [#41](https://github.com/cixanla/PC-Shop-Empire-3D/issues/41)
- Repository Guard: [31865403562](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31865403562), başarılı.

## Test, build ve runtime kanıtı

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `parcel-scene-build.log` | GarageGraybox r10 üretildi | `334e5837a2600e78435a7b31d97e584ea2be01ec946cf94d683b69933039117c` |
| `parcel-editmode-final.xml` | 192/192 geçti; failed/skipped 0 | `e21ab3813bd3630e48c356941d9e438fe3f7ec88f3b5ea2491b6f8bfeff2b2c1` |
| `parcel-playmode-final.xml` | 17/17 geçti; failed/skipped 0 | `df13a3f83434f33307c082dc1b1c488c776a2bff911a07a7c032f2719b062a4c` |
| `parcel-macos-build.log` | Universal development build, 327.475.393 bayt | `bb44ef92621554c4391cff38d12387f740a2254646ddebd46c8f1055690971b0` |
| Player executable | Mach-O `x86_64 + arm64` | `8d011cb9ede0fb847fdd1fa3696e94390b726dc4388c94ba47bc85de666c0da3` |
| `parcel-macos-runtime-1.log` | Apple M4/Metal 1280×720; `accepted=ok parcel-open=ok carry=ok world-floor=ok stable=ok quantity=1` | `eeb1fd8cf84bc92b5e1caef3f6a435c3badad5b68b08630a8557ced6c91ba6f7` |

Klavye ve gamepad acceptance→open→pickup zincirini gerçek device state ile doğrular. Full ActorHands failure fiziksel ownership'i değiştirmez; opening öncesi/identity/location failure'ları no-reveal/no-mutation kalır. Kilitli macOS oturumu nedeniyle yeni pencere ekran görüntüsü yoktur; sahne, test, build ve native runtime log kanıtı başarılıdır.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core/time/event ve RNG/dispatcher: `8af2ad3`, `bbb3648`, `43e9217`, `3d819e5`.
- İlk oynanabilir garaj ve fiziksel akış: `c7a3a26`, `44b8162`, `720e6d4`, `e944198`, `661f2dc`.
- Lookdev, stacking ve loaded cart: `c7214af`, `2e11e30`, `82bf74f`.
- Catalog + Inventory: `71935f11b80d02d03f9dcc1a3f08cafca7e301ff`.
- Atomic purchase-order receiving: `e596e079d90b6d5b9d94714d7821502574eba3c9`.
- Authoritative stock-flow projection: `9d75573a86e395d2fa74f3808d43310e4d65f760`.
- Idempotent delivery parcel reveal: `3766f3f06df624093f4774ef8fa4e7f1286d1c01`.

## USB güvenlik katmanı

Korunan milestone kayıtları `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D` altındadır. Güncel snapshot `2026-08-15_STAGE_B_DELIVERY_PARCEL_UNPACKING`, source/docs checkpoint `756547f42298b7aeaf01075cbf6d000cbb97ddaa` taşır. İçerik 471 tracked kaynak + 5 scene/test/build/runtime kanıtı + source kaydıdır; 477 manifest satırı ve manifest SHA-256 `37f95b3ccc6fd8cca19d2e1068d06d2b3072223c6bbb8a13b5e856ce768e58ac` tam readback/source checksum ile doğrulandı. Hash/boyut mismatch, source mismatch, forbidden dir, credential filename ve AppleDouble `0`; büyük `.app`, cache ve credential dışarıdadır. USB güvenle çıkarılabilir.

## Sıradaki bounded paket

1. Epic #8 altında raf ürünü için Unity-bağımsız authoritative satış teklifi/fiyat etiketi sözleşmesi kur.
2. Fiyatın product/shelf offer kimliği, para birimi, pozitif minor-unit değeri, revision ve no-mutation failure invariantlarını test et.
3. GarageGraybox RAF A etiketini yalnız başarılı offer komutundan sonra görünür fiyatla bağla; Inventory quantity ve dünya projection'ını fiyat authority'si yapma.
4. Ardından müşteri seçim/rezervasyon ve checkout/satış zincirine geç; Save/Guardian sınırlarını kendi issue'larında tut.

Her adım ayrı issue, test, commit, CI ve checkpoint ile kapanır. İlk fiyat paketi dinamik ekonomi, vergi, indirim, müşteri AI veya ledger debit eklemez.

## Güvenli devam komutu

> Delivery parcel unpacking checkpointinden devam et. Önce yaşayan belgeleri, temiz `origin/main` eşitliğini ve Epic #8'i doğrula. Sıradaki bounded paket RAF A için Unity-bağımsız authoritative satış teklifi/fiyat etiketi sözleşmesi ve görünür label projection'ıdır. Inventory/world quantity fiyat gerçeği olmasın. Test, uygun build/runtime kanıtı, commit/push/CI ve yaşayan kayıt olmadan tamamlandı sayma.
