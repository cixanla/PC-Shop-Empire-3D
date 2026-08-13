# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** Issue #31 küçük kutu kontrollü placement tamamlandı; Issue #6 büyük kutu dilimiyle sürecek<br>
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
- `E / Gamepad South` küçük kutuyu alır; `G / Gamepad East` placement modu kapalıyken mevcut güvenli drop'u korur.
- `Mouse Left / Gamepad RT` kontrollü placement önizlemesini açıp kapatır; mod açıkken `G / Gamepad East` geçerli pozu onaylar.
- İşaretli stock surface `0,25 m` grid ve yüzeye göre `90°` yaw snap kullanır. Tam taban desteği, eğim ve world/interactable/player overlap doğrulanmadan placement gerçekleşmez.
- Yeşil/kırmızı ghost yanında `GEÇERLİ/ENGELLİ` metni vardır. Geçersiz durumda aynı stable ID'li kutu elde kalır.
- Onaylı placement collider'ı aktif, gravity-off kinematic sabit dünya pozu üretir; normal drop özgün physics snapshot'ını geri yükler.
- Disable/world-floor recovery ve son güvenli poz davranışı korunur.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Feature commit: `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed`
- Tree: `27652c274c849a127a20b1f52960f435760111eb`
- Epic/issue: [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#31](https://github.com/cixanla/PC-Shop-Empire-3D/issues/31)
- Karar: `Docs/ADR-0010-CONTROLLED-SMALL-BOX-PLACEMENT.md`.
- Kapsam: `PlacementSurface`, deterministik solver/evaluation, collider taşımayan ghost, carry-controller mod akışı, stabil `PlaceAt`, güncel PlayerRig/GarageGraybox ve gerçek input testleri.
- Builder güvenliği, connected prefab, build-scene sırası, runtime Input Action kopyası, stable item ID ve pickup/drop/recovery invariantları korundu.
- Büyük kutu, kullanıcı rotation inputu, istifleme, taşıma arabası ve authoritative Inventory bu checkpoint'in dışında kaldı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `placement-editmode-final.xml` | 123/123 geçti | `694d5f139e019c83de36d6d3981965cd784f144738319060e7d1d075536aa8d6` |
| `placement-playmode-final.xml` | 8/8 geçti | `fb9ce67ddc62f6d603bd707c300de9c3c618dfed63eedaf37d0b0909181b1674` |
| `placement-macos-build-final.log` | Universal development build, 326.147.564 bayt | `5bab0c612bd756db2dba8f5183479aac3e08c00f75007b91ae17b15077351097` |
| `placement-macos-runtime-final.log` | Apple M4/Metal, 1280×720, `placement=ok` | `a669109c6638a428f89e1d7c87f743c711aac5aaa566624cd6b844e14d61bf4f` |
| `placement-macos-runtime-final.png` | Gerçek player'da kırmızı `ENGELLİ` ghost ve dinamik prompt | `c4208a2ea1227591ffa407f64ef0b6a3e5c12915648dc2dfe8389fad55a39122` |

EditMode; grid/yaw snap, işaretsiz yüzey ve obstruction sonuçlarını doğrular. PlayMode gerçek Input System device-state olaylarıyla keyboard/mouse ve gamepad placement zincirini, engelde fail-closed davranışı, stable ID'yi, eski doğrudan drop'u, recovery'yi ve fixed-step poz kararlılığını doğrular. Mac kanıtı Windows native doğrulamasının yerine geçmez.

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

## USB güvenlik katmanı

Korunan milestone kayıtları:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`

Küçük kutu pickup/drop + placement snapshot'ı `7794e2ab82c3b26c1149af526ed582f1cc406acb` source commit'inden alındı: 336 tracked dosya, 5.928.850 mantıksal bayt ve `b4df8efde544cbe3557bf67f67c13034733949821bdc7848ce612af1129be0fb` manifest SHA-256. İki tam manifest readback ve iki source→USB checksum dry-run geçti; `.git`, cache, build, log ve credential kapsam dışıdır. USB yeniden bağlanmadan önceki “snapshot bekliyor” notu kapanmıştır.

## Devam sırası

1. [Issue #32](https://github.com/cixanla/PC-Shop-Empire-3D/issues/32) ile büyük kutu hız/görüş bedeli ve güvenli taşıma profilini uygula.
2. Büyük kutu doğrulamasından sonra kullanıcı rotation inputu ve istiflemeyi ayrı acceptance dilimlerinde ele al.
3. Gerçek raf stoklama ve ekonomik Inventory authority'yi Issue #7/#8 bağımlılıklarına bağla; sahne projection'ını tek başına stok gerçeği sayma.
4. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
