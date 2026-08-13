# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** İlk oynanabilir birinci şahıs garaj graybox tamamlandı; sıradaki iş görünür el + alma/bırakma<br>
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
- Kontroller: Keyboard&Mouse ve Gamepad; Move/Look/PrimaryAction/Interact/Sprint/Drop/Pause.
- FOV, mouse/gamepad hassasiyeti, invert-Y, motion-reduce, cursor/pause ve rebind override store var.
- Görünür eller şimdilik lisanssız geometrik placeholder'dır; animasyon ve fiziksel etkileşim sıradadır.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Commit: `c7a3a26075998252d9ae8b88824d8285e5067069`
- Tree: `6d63d724e40b18efdc29269c5b5d305ccf5a4373`
- Kapsam: 72 dosya; GarageGraybox, PlayerRig, input/motor/view, prototype materials/hands, scene builder ve testler.
- Builder güvenliği: kaydedilmemiş sahne çalışması onaysız kaybolmaz; önceki scene setup geri yüklenir.
- Prefab bütünlüğü: PlayerRig origin'de tek kaynak prefab, garajda connected instance ve spawn override'dır.
- Build settings: önceki sahnelerin enabled/disabled durumu korunur.
- Input bütünlüğü: her runtime oyuncusu kendi Input Action kopyasını kullanır; mouse binding `<Mouse>/delta` ile masaüstüne sınırlıdır.
- Bağımsız iki inceleme sonrasında kritik/önemli bulgu kalmadı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `editmode_garage_checkpoint_20260813.xml` | 114/114 geçti | `88b966dfabd0c6d8fa749d3eb08299498270130e0d082eac1cef28e64b9d625a` |
| `playmode_garage_input_final2_20260813.xml` | 2/2 geçti | `f8e918b408c16bb8a6a6a0a2dcbe4544af77e52b68c184a3a9b516a2f6da0a00` |
| `garage_mac_build_final_20260813.log` | başarılı, 325.932.692 bayt | `a0bcd99f43d34468b7de119bf7c7875f0e98f0c9674fc9b5f8d9f8eb00a57766` |
| `garage_mac_smoke_graphical_1080p_20260813.log` | Metal, 1920×1080 `GARAGE_GRAYBOX_RUNTIME_READY`, hata yok | `213b23b02b782d54d3e8a094d36e10e384ed03cf862847a2ec50b0e164c1bded` |

Play Mode; sanal fakat gerçek Input System device-state olaylarıyla W+Shift, mouse delta, gamepad left/right stick, hareket mesafesi ve yaw/pitch değişimini doğrular. macOS development player Universal `arm64+x86_64` üretildi; Apple M4/Metal üzerinde pencereli 1920×1080 smoke çalıştı. Bu Mac kanıtı Windows native doğrulamasının yerine geçmez.

## Korunan geçmiş

- Stage A: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`, tag `stage-a-baseline-2026-08-11`.
- Core assembly: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`.
- Stable identity/result: `4cd2d928dbfda1886632bacce4a141c2a43161df`.
- Deterministic time/event: `8af2ad3d05906839c4b607e4958650e723060465`.
- PCG32: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`.
- Seed derivation: `43e92174ca3866dfde436fb180785a615772a886`.
- Event dispatcher hardening: `3d819e533fd3635bc9b32787730d6dd9be110875`.
- First playable garage: `c7a3a26075998252d9ae8b88824d8285e5067069`.

## USB güvenlik katmanı

Korunan milestone'lar:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`

Garaj checkpoint'i önce private GitHub'da tamamlanır. Ayrı USB milestone, alma/bırakma + placement ile ilk anlamlı fiziksel etkileşim zinciri kapandığında alınacaktır; cache/build/credential dahil edilmeyecektir.

## Devam sırası

1. Issue #5'i bounded alt işlere böl: interactable sözleşmesi, hedef çözümleme, görünür el durumu, pickup/drop.
2. Erişim mesafesi, line-of-sight, tek taşıma slotu, collider/rigidbody sahipliği ve güvenli drop fallback'ini testle.
3. Issue #6 ile küçük/büyük kutu taşıma, hız/görüş bedeli ve snap placement'a geç.
4. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
