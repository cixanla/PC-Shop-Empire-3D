# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** Issue #34 okunaklı yarı gerçekçi tek-köşe benchmarkı tamamlandı; düşük kullanım nedeniyle yeni uzun paket başlatılmadı<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

## En yeni checkpoint — Issue #34

- Feature commit `c7214afab81a360a3ca10a88cbdd29f67e741994`, tree `cb734bdc31069f584999558c8d8bdb78e2c968cc`.
- Tek referans köşesinde bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom ve reflection probe çalışır; bütün garajın final sanatı değildir.
- EditMode `128/128`, gerçek Input System PlayMode `10/10`, Universal macOS build ve Apple M4/Metal `rotation=ok lookdev=ok` player smoke geçti.
- Pickup/drop/placement/rotation/recovery ve collider/stable-ID invariantları korundu.
- Kanıt: `Docs/Evidence/READABLE-LOOKDEV-CHECKPOINT-2026-08-13.md`; Repository Guard [31688852779](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31688852779) başarılı.
- Sıradaki bounded iş küçük-kutu üstü tam destek/overlap doğrulamalı istiflemedir.

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
- Turuncu bantlı büyük kutu ayrı stable kimlik/boyut ve carry profili taşır. Alındığında iki-el pozu, `0,65×` hareket ve sprint kilidi uygulanır.
- Büyük kutunun istenen FOV bedeli `6°`, üst sınırı `8°`dir. Varsayılan `motionReduced` açıkken lens değişmez; görünür kutu/eller görüş maliyetini taşır. Ayar kapalıysa FOV yumuşak geçişle uygulanır.
- Büyük kutu küçük-kutu placement moduna giremez. Etkin `G / Gamepad East` promptuyla gerçek yarı boyutlarına göre güvenli bırakılır; obstruction durumunda `BIRAKMA ENGELLİ` gösterir ve elde kalır.
- Tek slot, stable item ID, physics snapshot, disable/world-floor recovery ve küçük-kutu davranışları korunur.
- Görsel hedef okunaklı yarı gerçekçiliktir: gerçek oran, PBR yüzey, zemine oturan ışık ve doğal ağırlık; mevcut primitive garaj/kutular/eller final sanat değildir.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `661f2dcc64246a8282fd63fbf303454ec856ea40`
- Tree: `d841329fcc351db4c9053a43ce5403855ffb57a0`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#33](https://github.com/cixanla/PC-Shop-Empire-3D/issues/33)
- Kararlar: `Docs/ADR-0012-CONTROLLED-SMALL-BOX-ROTATION.md` ve `Docs/ADR-0013-READABLE-SEMI-REALISTIC-VISUAL-DIRECTION.md`.
- Kanıt: `Docs/Evidence/SMALL-BOX-ROTATION-CHECKPOINT-2026-08-13.md`.
- Kapsam: küçük-kutu placement modunda ayrı `RotatePlacement`, clockwise 90° adım, etkin binding/açı promptu, döndürülmüş footprint doğrulaması, dikdörtgen kutu/yön işareti ve gerçek keyboard/gamepad testleri.
- Builder güvenliği, connected prefab, build-scene sırası, stable item ID, tek slot ve pickup/drop/placement/recovery invariantları korundu.
- Serbest rotation, büyük-kutu placement, stacking, taşıma arabası ve authoritative Inventory bu checkpoint'in dışında kaldı.
- Remote Repository Guard: [31683991075](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31683991075), başarılı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `rotation-editmode.xml` | 127/127 geçti | `8fd8c1245fcbf106bddf20a51196a58298619d19a36d0ed3e2cdded9501a569b` |
| `rotation-playmode-final.xml` | 10/10 geçti | `e858cd0c42b7fa94a7a826d3823dc6c1d515fafe2f4fdc40228a2069d827e8a2` |
| `rotation-macos-build.log` | Universal development build, 326.160.273 bayt | `33a4cd4e2ab11229be86e3b5587d6a708365bd51909e656ed0e57312d55aa2e7` |
| Player executable | Mach-O `x86_64 + arm64` | `258483b14034f9043298ae07635f6f50ca629a923e82e65eeafd9fa1c741f743` |
| `rotation-macos-runtime.log` | Apple M4/Metal, 1280×720, `rotation=ok` | `7ffbd6e3847c8df022a70983718359ef51f122dc3f6246b2a1eedcd308661e7a` |
| `rotation-macos-runtime.png` | Dikdörtgen kutu, eller ve `R / RB ... [90°]` promptu | `6f07afe2daf4b9bb2543c0d719511490dc4d3811660d23842a9bc1310c1b67d1` |

EditMode rotation normalizasyonunu, action/binding sözleşmesini, stable kimliği ve sahne ölçüsünü doğrular. PlayMode gerçek Input System device-state olaylarıyla keyboard/gamepad rotation, etkin prompt, ghost/confirm poz eşitliği, engelde fail-closed davranış ve pickup/drop/büyük-kutu/recovery regresyonlarını doğrular. Mac kanıtı Windows native doğrulamasının yerine geçmez.

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

## USB güvenlik katmanı

Korunan milestone kayıtları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`

Küçük kutu pickup/drop + placement snapshot'ı `7794e2ab82c3b26c1149af526ed582f1cc406acb` source commit'inden alındı: 336 tracked dosya, 5.928.850 mantıksal bayt ve `b4df8efde544cbe3557bf67f67c13034733949821bdc7848ce612af1129be0fb` manifest SHA-256. İki tam manifest readback ve iki source→USB checksum dry-run geçti; `.git`, cache, build, log ve credential kapsam dışıdır. USB bu paket için artık gerekli değildir ve güvenle çıkarılabilir.

## Devam sırası

1. [Issue #34](https://github.com/cixanla/PC-Shop-Empire-3D/issues/34) ile yalnız tek referans garaj köşesinde okunaklı yarı gerçekçi PBR/ışık benchmarkı üret; gameplay collider ve interaction sözleşmelerini değiştirme.
2. Küçük-kutu üstü istiflemeyi rotation'dan ayrı tam-destek/overlap acceptance paketi olarak ele al.
3. Taşıma arabasını Issue #6'nın ayrı graybox dilimi olarak doğrula.
4. Gerçek raf stoklama ve ekonomik Inventory authority'yi Issue #7/#8 bağımlılıklarına bağla; sahne projection'ını tek başına stok gerçeği sayma.
5. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
