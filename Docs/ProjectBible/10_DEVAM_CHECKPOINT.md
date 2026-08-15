# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 15 Ağustos 2026<br>
**Durum:** Issue #35 kontrollü küçük-kutu istifleme tamamlandı; sıradaki bounded paket taşıma arabası grayboxıdır<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #35

- Feature commit `2e11e30a1a4b3435046ae18001004cacc170079e`, tree `e2cb49e318ea84b4a8db08ab3dd79d9b833b2483`.
- Stable dünya durumundaki küçük kutu, eldeki küçük kutu için merkez/90° snap ve beş noktalı tam destek yüzeyi olabilir.
- Geçersiz rotation/footprint, dinamik destek ve overlap fail-closed kalır; tek üst kutu ve dolu tabanı alma kilidi vardır.
- EditMode `131/131`, gerçek Input System PlayMode `12/12`, Universal macOS build ve Apple M4/Metal `rotation=ok stacking=ok lookdev=ok` player smoke geçti.
- Pickup/drop/placement/rotation/large-carry/recovery ve stable-ID invariantları korundu.
- Kanıt: `Docs/Evidence/SMALL-BOX-STACKING-CHECKPOINT-2026-08-15.md`; Repository Guard [31856764087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31856764087) başarılı.
- Sıradaki bounded iş taşıma arabası grayboxıdır; Inventory authority Issue #7/#8'e bağlı kalır.

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
- Tek slot, stable item ID, physics snapshot, disable/world-floor recovery ve küçük-kutu davranışları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir: gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `2e11e30a1a4b3435046ae18001004cacc170079e`
- Tree: `e2cb49e318ea84b4a8db08ab3dd79d9b833b2483`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#35](https://github.com/cixanla/PC-Shop-Empire-3D/issues/35)
- Karar: `Docs/ADR-0014-CONTROLLED-SMALL-BOX-STACKING.md`.
- Kanıt: `Docs/Evidence/SMALL-BOX-STACKING-CHECKPOINT-2026-08-15.md`.
- Kapsam: stable küçük-kutu desteği, merkez/90° snap, beş noktalı footprint, obstruction, tek kat/tek üst ilişki, dolu taban kilidi, dinamik prompt ve gerçek keyboard/gamepad testleri.
- Builder güvenliği, connected prefab, build-scene sırası, stable item ID, tek slot ve pickup/drop/placement/rotation/large-carry/recovery invariantları korundu.
- Çok katlı/palet istifi, büyük-kutu placement/istif, taşıma arabası ve authoritative Inventory bu checkpoint'in dışında kaldı.
- Remote Repository Guard: [31856764087](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31856764087), başarılı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `stacking-editmode-final.xml` | 131/131 geçti | `b1b7039bf5ff42f8f82b1f9575bdb34b5c4ce306fbaed711a184ccb563fea737` |
| `stacking-playmode-final.xml` | 12/12 geçti | `0993555d45b11392c748fd7ef5d355d6887eaec449d40d1dfffc663b3351911e` |
| `stacking-macos-build.log` | Universal development build, 327.217.897 bayt | `51ad3ad892c6ef91f42494fbdff7c3db2bf2a1ac855d1d66314e9ab6234a7de7` |
| Player executable | Mach-O `x86_64 + arm64` | `7ee0d3418135a381f6751a026d4c58e2eabfb78ba3fe86fa1cf1acc8c661a356` |
| `stacking-macos-runtime.log` | Apple M4/Metal, 1280×720, `stacking=ok` | `13e030c4a45b8bd9e782e4dd63cf892814a5da2a5e32d4a3c457a1c92bd33472` |

EditMode stabil/dinamik destek, tam footprint, rotation, ilişki ve pickup kilidini doğrular. PlayMode gerçek Input System device-state olaylarıyla keyboard/mouse ve gamepad istifleme, etkin prompt, invalid rotation fail-closed davranışı, stable kimlik ve pickup/drop/büyük-kutu/recovery regresyonlarını doğrular. Mac kanıtı Windows native doğrulamasının yerine geçmez.

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

## USB güvenlik katmanı

Korunan milestone kayıtları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_SMALL_BOX_STACKING`

Güncel küçük-kutu istifleme snapshot'ı final tracked kaynak ile test/build/runtime kanıtlarını ayrı `SOURCE` ve `EVIDENCE` dizinlerinde tutar. SHA-256 manifest readback ve source→USB checksum dry-run doğrulandı; `.git`, cache, build uygulaması ve credential kapsam dışıdır. USB bu paket tamamlandıktan sonra güvenle çıkarılabilir.

## Devam sırası

1. Taşıma arabasını Issue #6'nın ayrı graybox dilimi olarak doğrula; ağır kutunun elde taşıma sözleşmesini bozma.
2. Benchmark görsel dilini yalnız tamamlanan gameplay alanlarına kademeli yay; sahneyi final art ilan etme.
3. Gerçek raf stoklama ve ekonomik Inventory authority'yi Issue #7/#8 bağımlılıklarına bağla; sahne projection'ını tek başına stok gerçeği sayma.
4. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
