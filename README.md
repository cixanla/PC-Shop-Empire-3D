# PC Shop Empire 3D

PC Shop Empire 3D; küçük bir garajdan başlayıp fiziksel teknoloji perakendesi, PC toplama, servis ve işletme yönetimini birinci şahıs oynanışta birleştiren yeni bir 3D simülasyon projesidir.

Bu depo, eski Electron oyununun doğrudan portu değildir. Yeni oyun Unity 6000.3.21f1 + URP + C# ile sıfırdan geliştirilmektedir; legacy sürüm yalnız doğrulanmış referans ve tasarım kaynağı olarak korunur.

## İlk okunacak dosyalar

1. [PROJECT_BIBLE.md](PROJECT_BIBLE.md) — vizyon, güncel durum, mimari, bütün sistemler, yol haritası ve devam protokolü.
2. [Docs/DEVELOPER-HANDOFF.md](Docs/DEVELOPER-HANDOFF.md) — farklı bilgisayarda veya yeni geliştiriciyle devam adımları.
3. [Docs/ProjectBible/00_OKU_BENI.md](Docs/ProjectBible/00_OKU_BENI.md) — ayrıntılı tasarım/araştırma paketinin dizini.
4. [CONTRIBUTING.md](CONTRIBUTING.md) — çalışma, test, belge ve pull request kuralları.
5. [Docs/REPOSITORY-GOVERNANCE.md](Docs/REPOSITORY-GOVERNANCE.md) — authoritative kaynak, yedek, branch ve erişim politikası.
6. [Docs/Evidence/GITHUB-HANDOFF-2026-08-11.md](Docs/Evidence/GITHUB-HANDOFF-2026-08-11.md) — GitHub, Project, Codex ve farklı bilgisayardan devam kanıtı.

## Güncel teknik durum

- Unity: `6000.3.21f1` ARM64 editör, URP `17.3.0`.
- Hedef: önce Windows x64 + Steam 1.0; bütçe uygunsa daha sonra ayrı macOS port/QA turu.
- Stage A: proje, paket, macOS/Windows Mono smoke build ve private GitHub temeli tamamlandı.
- Stage B: saf `PSE.Core` sözleşmeleri ile ilk oynanabilir birinci şahıs garaj graybox'ı tamamlandı.
- Son doğrulama: Edit Mode `114/114`, Play Mode `2/2`; başarısız/atlanan test yok.
- Yürütme panosu: [PC Shop Empire 3D — Development Roadmap](https://github.com/users/cixanla/projects/2), 22 epic.
- GarageGraybox sahnesinde klavye/fare ve gamepad hareketi, kamera, sprint, pause, görünür prototip eller ve Mac development player çalışıyor; sıradaki iş fiziksel alma/bırakmadır.

## Dizinler

- `Assets/`, `Packages/`, `ProjectSettings/` — authoritative Unity projesi.
- `Docs/ProjectBible/` — tam Game Design Bible, araştırma, dönüşüm matrisi, mimari, roadmap, karar hafızası ve checkpointler.
- `Docs/` — ADR'ler, handoff, governance, provenans ve teknik kayıtlar.
- `LegacyReference/PC-Shop-Empire-1.1.6/` — hash doğrulanmış, değiştirilmeyen legacy kaynak snapshot'ı.
- `SourceAssets/` — ileride düzenlenebilir sanat/ses kaynakları.
- `Tools/` — repo doğrulama ve proje yardımcıları; üçüncü taraf binary tutulmaz.
- `../Builds/Local/` — yerel buildler; Git dışında kalır.

## Hızlı doğrulama

```bash
./Tools/verify-repository.sh
```

Unity testleri için Editor içindeki Test Runner veya `Docs/DEVELOPER-HANDOFF.md` içindeki batch komutu kullanılmalıdır.

## Haklar

Proje özeldir ve [LICENSE.md](LICENSE.md) koşullarına tabidir. Depoya erişim, kaynakları yayımlama veya başka projede kullanma hakkı vermez. Üçüncü taraf bileşenler kendi lisanslarını korur.
