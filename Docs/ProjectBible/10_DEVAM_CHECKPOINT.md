# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** İlk fiziksel ürün pickup/drop tamamlandı; sıradaki iş hibrit kutu placement<br>
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
- Görünür eller lisanssız geometrik placeholder'dır; boş/hedef/tutuyor/engelli/recovery pozları çalışır.
- Garajdaki `prototype.garage-box-001`, `E / Gamepad South` ile alınır; `G / Gamepad East` ile güvenli yüzeye bırakılır.
- Stable ID, range+LOS, tek slot, physics snapshot/restore, blocked/no-support fail-closed ve son güvenli poz recovery çalışır.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ dış platform kapısıdır.

## Feature checkpoint

- Branch: `main`
- Commit: `44b816289f942e57fc176b26b203711090d0e61c`
- Tree: `56a08053037817158c6293fe235760105c2dd811`
- Kapsam: PhysicalItemProjection, resolver, carry controller, safe-drop, hand presenter, updated GarageGraybox/PlayerRig ve testler.
- Builder güvenliği: kaydedilmemiş sahne çalışması onaysız kaybolmaz; önceki scene setup geri yüklenir.
- Prefab bütünlüğü: PlayerRig origin'de tek kaynak prefab, garajda connected instance ve spawn override'dır.
- Build settings: önceki sahnelerin enabled/disabled durumu korunur.
- Input bütünlüğü: her runtime oyuncusu kendi Input Action kopyasını kullanır; prompt effective binding'den üretilir.
- Fizik bütünlüğü: carry sırasında collider kapalı/kinematic; drop'ta özgün parent/layer/body/collider durumu geri gelir.
- Recovery: player disable veya `y < -20` aynı nesneyi/kimliği son güvenli poza döndürür; engelli drop nesneyi elde tutar.
- Bağımsız inceleme sonrasında kritik/önemli bulgu kalmadı.

## Test ve build kanıtı

Ham çıktılar Git dışındaki `../TestResults` klasöründedir.

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `pickup-editmode-final2.xml` | 120/120 geçti | `14d2a91e8e38ce528225f9cda3ea172c7f9d80a2ddb1bb3090f2f6e6a6fb7c6a` |
| `pickup-playmode-final2.xml` | 6/6 geçti | `361a171b8c8741b88754824a8dc4850aa327bdf2191793acc4dabc9fc948052f` |
| `pickup-macos-build-checkpoint.log` | başarılı, 325.963.160 bayt | `0ee40752c4637e0dd6c9f88f869cb4715b7ae2a699f9603f54480c31bbab1474` |
| `pickup-macos-runtime-checkpoint.log` | Metal, 1920×1080, `carry=ok`, hata yok | `fd9ac367f6a8fdb51909d52d4275c0b3d774303fd9843c08de21b7f7d0f2ddb2` |

Play Mode; gerçek Input System device-state olaylarıyla hareket/kamera yanında keyboard `E/G`, gamepad South/East, pause engeli, disable recovery ve dünya-altı recovery'yi doğrular. macOS development player Universal `arm64+x86_64` üretildi; Apple M4/Metal üzerinde pencereli 1920×1080 smoke çalıştı. Bu Mac kanıtı Windows native doğrulamasının yerine geçmez.

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

## USB güvenlik katmanı

Korunan milestone'lar:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`

Pickup/drop checkpoint'i private GitHub'da tamamlanır. Ayrı USB milestone, pickup/drop + placement ile ilk anlamlı fiziksel etkileşim zinciri kapandığında alınacaktır; cache/build/credential dahil edilmeyecektir.

## Devam sırası

1. Issue #6'yı küçük kutu placement, döndürme/snap ve büyük kutu taşıma olarak bounded alt işlere böl.
2. Önce küçük kutuyu işaretli teslimat/stok alanına güvenli yerleştiren zinciri testle.
3. Ardından büyük kutu için hız/görüş bedeli ve taşıma profiline geç.
4. İlk gerçek Windows x64 cihazını Faz 1 kapanmadan devreye al.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test/build sonucu, remote CI ve devam sırası bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edilebilir.
