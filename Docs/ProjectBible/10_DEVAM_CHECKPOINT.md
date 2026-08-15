# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #37 yüklü taşıma arabası ve Epic #6 tamamlandı; sıradaki bounded paket Issue #7 Catalog + Inventory çekirdeğidir<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #37 / Epic #6

- Feature commit `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`, tree `1d48b75c74e5ae14ee92d4f0687a68ec35182ddd`.
- GarageGraybox tek stable platform arabası taşır; eldeki `LargeBox` `E / Gamepad South` ile yüklenir veya yeniden ellere alınır.
- `Mouse Left / Gamepad RT` arabayı tutar/bırakır; yüklü hız `0,85×`, boş hız `0,90×`, sprint kapalıdır.
- Dört noktalı zemin desteği, hedef overlap ve swept bounds obstruction kontrolü başarısızsa hareket uygulanmaz; araba/yük son güvenli durumda kalır.
- EditMode `136/136`, gerçek Input System PlayMode `14/14`, Universal macOS build ve Apple M4/Metal `transport-cart=ok`, `cart-flow=ok loaded=ok stable=ok` player smoke geçti.
- Pickup/drop/placement/rotation/stacking/large-carry/recovery ve stable-ID invariantları korundu.
- Kanıt: `Docs/Evidence/LOADED-TRANSPORT-CART-CHECKPOINT-2026-08-15.md`; feature Repository Guard [31859948692](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31859948692) başarılı.
- Sıradaki bounded iş Issue #7 altında Unity'den bağımsız Catalog + Inventory çekirdeğidir; fiziksel projection henüz authoritative stok değildir.

## Kullanım güvenliği protokolü

- Kalan kullanım yüzdesini model doğrudan okuyamaz; kullanıcı/panel bildirimi authoritative kabul edilir.
- Her bounded paket test → Git commit → private push → CI → gerektiğinde ayrı USB milestone sırasıyla kapanır.
- Kullanıcı yeniden çok düşük kullanım bildirirse uzun işe başlanmaz; en yakın temiz commit sınırında bu belge güncellenir.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve motor/proje migration'ı ayrı açıklama/onay kapısıdır.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity `6000.3.21f1`, URP `17.3.0`, C#.
- Core: stable ID/result/time, sürümlü PCG32, SHA-256 stream derivation ve deterministik event dispatcher tamam.
- Gameplay sınırları: `PSE.World` ve `PSE.Presentation`.
- İlk oynanabilir sahne: `Assets/Scenes/Prototypes/GarageGraybox.unity`.
- Connected oyuncu prefabı: `Assets/Prefabs/Prototype/PlayerRig.prefab`.
- Küçük kutu `E / Gamepad South` ile alınır; `Mouse Left / Gamepad RT` placement önizlemesini açar; `G / Gamepad East` yerleştirir veya mod kapalıyken güvenli bırakır.
- Küçük kutu placement'ı işaretli stock surface üzerinde `0,25 m` grid/`90°` yaw snap, tam destek ve overlap doğrulaması kullanır; geçerli sonuç stabil kinematic pozdur.
- Küçük kutu placement modunda `R / Right Shoulder` ile clockwise `90°` döner. Etkin binding ve mevcut açı prompt'ta görünür; ghost/confirm aynı pozu, döndürülmüş footprint aynı fail-closed güvenlik kontrolünü kullanır.
- GarageGraybox iki küçük kutu taşır. Stable kinematic küçük kutu üstünde `İSTİF GEÇERLİ` önizlemesi, merkez/90° snap, beş noktalı tam destek, tek kat/tek üst ilişki ve dolu taban pickup kilidi çalışır.
- Turuncu bantlı büyük kutu ayrı stable kimlik/boyut ve carry profili taşır. Alındığında iki-el pozu, `0,65×` hareket ve sprint kilidi uygulanır.
- Büyük kutunun istenen FOV bedeli `6°`, üst sınırı `8°`dir. Varsayılan `motionReduced` açıkken lens değişmez; görünür kutu/eller görüş maliyetini taşır. Ayar kapalıysa FOV yumuşak geçişle uygulanır.
- Büyük kutu küçük-kutu placement moduna giremez. Etkin `G / Gamepad East` promptuyla gerçek yarı boyutlarına göre güvenli bırakılır; obstruction durumunda `BIRAKMA ENGELLİ` gösterir ve elde kalır.
- Platform arabası tek büyük kutuyu aynı stable ID ve ilk physics snapshot'ıyla taşır. Dört teker desteği ve swept obstruction geçmeden pose uygulanmaz; yük world-parent kinematic olarak anchor'a senkronlanır.
- Yüklü/boş araba etkin binding prompt'u, ayrı iki-el tutuş pozu, sprint kilidi ve `0,85×`/`0,90×` hareket profili taşır. Engel, driver/controller veya cart disable recovery'si fail-closed davranır.
- Tek slot, stable item ID, physics snapshot, disable/world-floor recovery ve küçük-kutu davranışları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir: gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`
- Tree: `1d48b75c74e5ae14ee92d4f0687a68ec35182ddd`
- Checkpoint docs commit: `148c6d1f2936307268237ae2c484743146f7e639`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#37](https://github.com/cixanla/PC-Shop-Empire-3D/issues/37)
- Karar: `Docs/ADR-0015-LOADED-TRANSPORT-CART-GRAYBOX.md`.
- Kanıt: `Docs/Evidence/LOADED-TRANSPORT-CART-CHECKPOINT-2026-08-15.md`.
- Kapsam: tek `LargeBox` slotu, hands→cart→hands ownership, tam destek/swept obstruction, yüklü/boş hareket profili, dinamik prompt, gerçek keyboard/gamepad akışı ve recovery.
- Builder güvenliği, connected prefab, build-scene sırası, stable item ID, physics snapshot ve pickup/drop/placement/rotation/stacking/large-carry invariantları korundu.
- Çoklu slot/palet, büyük-kutu placement/istif, gerçek raf container'ı ve authoritative Inventory bu checkpoint'in dışında kaldı.
- Remote feature Repository Guard: [31859948692](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31859948692), başarılı.
- Remote checkpoint Repository Guard: [31860208560](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31860208560), başarılı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `cart-editmode-final.xml` | 136/136 geçti | `6de78a3e7be6d47e9780962bdabef4a64d5efe153bf3b572fee90c5da98c9bca` |
| `cart-playmode-final.xml` | 14/14 geçti | `87b3c4e42d73186191740b69971b482bb49ab68c882105f48e7aee39628ccea3` |
| `cart-macos-build.log` | Universal development build, 327.282.300 bayt | `1511e285a0cb051b1216c11d455efba7334fdda22ef75456e627451a7677f347` |
| Player executable | Mach-O `x86_64 + arm64` | `d6d5e7afdf5cae9d39c6696507bf9ea8c181b22a58299de31b8968889739ba27` |
| `cart-macos-runtime-final.log` | Apple M4/Metal, `transport-cart=ok`, `cart-flow=ok loaded=ok stable=ok` | `e2a5c113f28db09d4746182bb062031b29b601b0e188d8737fe5967ca5ef2a56` |
| `cart-macos-runtime-final.png` | 1280×748 yüklü araba ve HUD | `816fec72ed909be4a5ab9244a888adbddce0743b1b42450ec27cabcf72bfc5d2` |

EditMode ownership/physics snapshot, kapasite/profil, grip menzili, dört noktalı destek ve obstruction sözleşmesini doğrular. PlayMode gerçek Input System device-state olaylarıyla keyboard/mouse ve gamepad yükle→sür→engel→bırak→geri al→recovery zincirini ve bütün önceki fiziksel etkileşim regresyonlarını doğrular. Mac kanıtı Windows native doğrulamasının yerine geçmez.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core assembly: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`.
- Stable identity/result: `4cd2d928dbfda1886632bacce4a141c2a43161df`.
- Deterministic time/event: `8af2ad3d05906839c4b607e4958650e723060465`.
- PCG32: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`.
- Seed derivation: `43e92174ca3866dfde436fb180785a615772a886`.
- Event dispatcher hardening: `3d819e533fd3635bc9b32787730d6dd9be110875`.
- First playable garage: `c7a3a26075998252d9ae8b88824d8285e5067069`.
- Safe physical pickup/drop: `44b816289f942e57fc176b26b203711090d0e61c`.
- Controlled small-box placement: `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed`.
- Safe large-box carry: `e94419862b04f6f03f97ef2e43c9da393c5d30a9`.
- Controlled small-box rotation: `661f2dcc64246a8282fd63fbf303454ec856ea40`.
- Readable lookdev benchmark: `c7214afab81a360a3ca10a88cbdd29f67e741994`.
- Controlled small-box stacking: `2e11e30a1a4b3435046ae18001004cacc170079e`.
- Loaded transport cart: `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`.

## USB güvenlik katmanı

Korunan milestone kayıtları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_SMALL_BOX_STACKING`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_LOADED_TRANSPORT_CART`

Güncel yüklü taşıma arabası snapshot'ı `148c6d1` tracked kaynağını ve 6 test/build/runtime kanıtını ayrı `SOURCE`/`EVIDENCE` dizinlerinde tutar. 396 kaynak dosyası birebir karşılaştırıldı; 403 manifest satırında hash/boyut hatası `0`, manifest SHA-256 `a9e1d8e5188d85503dbff923127ac3bd71c6d9e023acf17003beddadfe0444c3`, yasak cache/build/credential ve AppleDouble sayısı `0`dır. USB bu paket tamamlandıktan sonra güvenle çıkarılabilir.

## Devam sırası

1. Issue #7 Catalog + Inventory çekirdeğini saf domain sözleşmeleri ve invariant testleriyle küçük bir ilk pakete böl.
2. Issue #8 sipariş/teslimat/raf akışını yalnız authoritative Inventory hazır olduktan sonra dünya projection'ına bağla.
3. Benchmark görsel dilini tamamlanan gameplay alanlarına kademeli yay; sahneyi final art ilan etme.
4. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
