# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** Issue #32 büyük kutu taşıma profili tamamlandı; Issue #6 küçük-kutu rotation dilimiyle sürecek<br>
**Authoritative kaynak:** private GitHub `cixanla/PC-Shop-Empire-3D`, `main`

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
- Turuncu bantlı büyük kutu ayrı stable kimlik/boyut ve carry profili taşır. Alındığında iki-el pozu, `0,65×` hareket ve sprint kilidi uygulanır.
- Büyük kutunun istenen FOV bedeli `6°`, üst sınırı `8°`dir. Varsayılan `motionReduced` açıkken lens değişmez; görünür kutu/eller görüş maliyetini taşır. Ayar kapalıysa FOV yumuşak geçişle uygulanır.
- Büyük kutu küçük-kutu placement moduna giremez. Etkin `G / Gamepad East` promptuyla gerçek yarı boyutlarına göre güvenli bırakılır; obstruction durumunda `BIRAKMA ENGELLİ` gösterir ve elde kalır.
- Tek slot, stable item ID, physics snapshot, disable/world-floor recovery ve küçük-kutu davranışları korunur.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `e94419862b04f6f03f97ef2e43c9da393c5d30a9`
- Tree: `da877668c89850e4d384c30aefe7e5cc175d317d`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#32](https://github.com/cixanla/PC-Shop-Empire-3D/issues/32)
- Karar: `Docs/ADR-0011-LARGE-BOX-CARRY-PROFILE.md`.
- Kanıt: `Docs/Evidence/LARGE-BOX-CARRY-CHECKPOINT-2026-08-13.md`.
- Kapsam: ayrı carry profile, büyük kutu graybox'ı, iki-el durumu, bounded hız/FOV maliyeti, fail-closed drop, dinamik prompt, güncel PlayerRig/GarageGraybox ve gerçek input testleri.
- Builder güvenliği, connected prefab, build-scene sırası, stable item ID, tek slot ve pickup/drop/placement/recovery invariantları korundu.
- Büyük-kutu placement/rotation/stacking, taşıma arabası ve authoritative Inventory bu checkpoint'in dışında kaldı.
- Remote Repository Guard: [31680394879](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31680394879), başarılı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `large-box-editmode-final.xml` | 126/126 geçti | `aabf0920d0105fe78e1ea55275360e8d7a17b74c5d8d9c510a18995fb7562812` |
| `large-box-playmode-final.xml` | 10/10 geçti | `d5be5ce304af1dc95c59b1b2ba44e068c819a00a02dfa3f692b5b6fb761cd5fb` |
| `large-box-macos-build.log` | Universal development build, 326.157.117 bayt | `984122a4028f633667f76548917989421dbeb9659b570c5fb352785b803e4f0c` |
| Player executable | Mach-O `x86_64 + arm64` | `571b84ed43da87f2bd0c348771f8ff97e10180e4e3bcc5a35fcf4a7a744ffe11` |
| `large-box-macos-runtime.log` | Apple M4/Metal, 1280×720, `large-carry=ok` | `d4807fd9112a7f2c29774db0ca2f0b7d188876b99ab832b3f0f94d636c51bb41` |
| `large-box-macos-runtime.png` | Gerçek player'da küçük ve büyük kutu görünür | `15da3b5de9078298368e8dd21711020cc32dc2199e59b1722df80190fcf89ec1` |

EditMode profil sınırlarını, motor hesabını, stable kimliği ve sahne sözleşmesini doğrular. PlayMode gerçek Input System device-state olaylarıyla keyboard/gamepad büyük-kutu taşıma/bırakma zincirini, sprint/FOV/iki-el geri bildirimini, engelde fail-closed davranışı, büyük-kutu recovery'sini ve küçük-kutu regresyonlarını doğrular. Mac kanıtı Windows native doğrulamasının yerine geçmez.

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

## USB güvenlik katmanı

Korunan milestone kayıtları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`

Küçük kutu pickup/drop + placement snapshot'ı `7794e2ab82c3b26c1149af526ed582f1cc406acb` source commit'inden alındı: 336 tracked dosya, 5.928.850 mantıksal bayt ve `b4df8efde544cbe3557bf67f67c13034733949821bdc7848ce612af1129be0fb` manifest SHA-256. İki tam manifest readback ve iki source→USB checksum dry-run geçti; `.git`, cache, build, log ve credential kapsam dışıdır. USB bu paket için artık gerekli değildir ve güvenle çıkarılabilir.

## Devam sırası

1. [Issue #33](https://github.com/cixanla/PC-Shop-Empire-3D/issues/33) ile küçük-kutu placement moduna kasıtlı clockwise `90°` rotation ve etkin binding promptu ekle.
2. Küçük-kutu üstü istiflemeyi rotation'dan ayrı tam-destek/overlap acceptance paketi olarak ele al.
3. Taşıma arabasını Issue #6'nın ayrı graybox dilimi olarak doğrula.
4. Gerçek raf stoklama ve ekonomik Inventory authority'yi Issue #7/#8 bağımlılıklarına bağla; sahne projection'ını tek başına stok gerçeği sayma.
5. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
