# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 13 Ağustos 2026<br>
**Durum:** Deterministik PRNG ve root-seed/context derivation epic'i tamamlandı; sıradaki iş event dispatcher/correlation<br>
**Son kullanıcı bildirimi:** Kalan kullanım %100; uzun fakat checkpoint'li geliştirme adımları onaylandı

## Kullanım güvenliği protokolü

- Hesapta kalan yüzde model tarafından doğrudan ve güvenilir biçimde okunamaz; Codex kullanım paneli veya kullanıcının bildirdiği değer authoritative kabul edilir.
- İşler daha büyük olabilir, fakat her paket test → Git commit → private push → CI → gerektiğinde USB milestone sırasıyla kapanır.
- Kullanıcı/panel yeniden düşük kullanım bildirirse yarım büyük işe başlanmaz; en yakın güvenli commit sınırında checkpoint oluşturulur.
- Büyük indirme, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset ve proje/motor migration'ı ayrı açıklama/onay kapısıdır.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity: `6000.3.21f1`, URP `17.3.0`, C#.
- Authoritative remote: private `https://github.com/cixanla/PC-Shop-Empire-3D`, default branch `main`.
- Unity/Editor bağımsız `PSE.Core` içinde stable ID, result/failure, integer simulation clock, immutable event envelope, sürümlü PCG32 ve SHA-256 framed bağlamsal stream derivation var.
- Son Edit Mode sonucu: **85/85 geçti**, başarısız 0, atlanan 0 (`stage_b_seed_derivation_editmode_20260813_final.xml`).
- Repository Guard: geçti; Unity sürümü doğru, legacy snapshot 26/26, Project Bible 11 belge, secret/cache/build ihlali yok.
- macOS Universal development build/headless smoke ve Windows x64 Mono cross-build önceki Stage A kanıtı olarak geçerli.
- Gerçek Windows x64 runtime/DirectX/Steam/IL2CPP testi hâlâ ilk oynanabilir öncesi dış bağımlılıktır.
- UVCS bağlantısı beklemededir; Git/GitHub tek authoritative VCS, USB ise ayrı hash snapshot katmanıdır.
- Yanlışlıkla oluşturulan ayrı Codex `Game` proje kaydı kaldırıldı. Unity klasörü, `.git`, GitHub remote'u, Issues ve Project #2 etkilenmedi; Codex'te yalnız mevcut ana `PC Shop Empire Similator` kaydı kalır.

## Son feature checkpoint

- Branch: `main`
- Feature commit: `43e92174ca3866dfde436fb180785a615772a886`
- Feature tree: `0ec21c6eb99d005ab4c33554c7b40c43f59de7e4`
- Kapsam: 16-hex canonical `RandomRootSeed`, `sha256-framed-be-pcg32-v1`, typed stable domain/context ve save metadata sürüm doğrulaması.
- Güvenlik: eksik/bozuk/bilinmeyen metadata için wall-clock/device entropy fallback yok; tüketilmiş uzun akış yalnız persisted `Pcg32State` ile devam eder.
- İki digest/state/selector/altı-draw golden vector; çağrı sırası, kültür, save/load ve snapshot devam testleriyle sabittir.
- Bağımsız code review ve ayrı Python SHA-256+PCG32 uygulaması iki vektörü birebir doğruladı; kritik veya önemli açık bulgu yok.
- GitHub Issue #24 ve parent Epic #2 kapandı; Project #2 durumları `Done` oldu.
- Remote Repository Guard run `31667925884` başarıyla tamamlandı.

## Test kanıtı

Ham test çıktıları repository dışında `../TestResults` altında tutulur:

| Dosya | Boyut | SHA-256 |
|---|---:|---|
| `stage_b_seed_derivation_editmode_20260813_final.xml` | 73.578 bayt | `0807daaa1800b77113141753701fff1aeb3032be355971b0c70c9cf48d2c96b6` |
| `stage_b_seed_derivation_editmode_20260813_final.log` | 41.318 bayt | `26f35c42b4a8c393e52038dc375dac1296d3bad47a339cba48b76ab9af8aca62` |

Son iki seed-derivation Unity turu 85/85 geçti. Golden digest ve draw değerleri ayrıca bağımsız Python uygulamasıyla doğrulandı. Kalan platform kapısı, ilk Windows/macOS IL2CPP player testinde aynı golden vektörlerin yeniden çalıştırılmasıdır.

## Korunan geçmiş

- Stage A root commit: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`
- Tag: `stage-a-baseline-2026-08-11`
- Core assembly: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`
- Stable identity/result: `4cd2d928dbfda1886632bacce4a141c2a43161df`
- Deterministic time/event: `8af2ad3d05906839c4b607e4958650e723060465`
- PCG32 core: `bbb3648c6e34eedd77e1bec948d5ee630f89679c`
- Root seed/context derivation: `43e92174ca3866dfde436fb180785a615772a886`
- GitHub devir temeli: `d79f85b2b201483dc58ddfdb6929f8afb6179010`
- Eski public `cixanla/PC-Shop-Empire` legacy release geçmişi olarak değiştirilmeden kalır.

## USB güvenlik katmanı

Korunan mevcut milestone'lar:

- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`
- `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`

RNG paketi ve checkpoint yeni, ayrı hedefe alındı:

`/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-13_STAGE_B_RNG`

- Snapshot source commit: `29522445a69a6869b1ce3de3fe9a00e93b40a2e9`
- Payload: 202 dosya, 5.504.984 bayt.
- `MANIFEST.tsv` SHA-256: `85a2a80927bf4bbab51645ce86564fcc69e970843a6841597a8810fd0f2312a5`
- 201 tracked dosyanın source→USB hash/size karşılaştırması ve bütün manifest readback kontrolü geçti.
- ExFAT üzerinde oluşan 246 yeniden üretilebilir `._*` AppleDouble metadata yan dosyası temizlendi; payload veya eski snapshot silinmedi.
- `.git`, `Library`, `Temp`, build, log, credential ve token snapshot'a girmedi.

## Devam sırası

1. Issue #3 event correlation/causation + deterministik in-memory dispatcher paketini tamamla.
2. Issue #4 ile gerçek birinci şahıs garaj graybox, input, kamera ve hareket prototipini kur.
3. Issue #5–#6 ile görünür eller, alma/bırakma, hibrit kutu taşıma ve güvenli placement akışına geç.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım düşük seviyeye indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test sonucu, remote CI ve USB manifesti bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edebiliriz.
