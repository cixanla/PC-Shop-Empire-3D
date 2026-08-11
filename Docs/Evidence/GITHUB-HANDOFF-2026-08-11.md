# GitHub ve Çok Bilgisayarlı Geliştirme Devir Raporu

**Tarih:** 11 Ağustos 2026<br>
**Durum:** Private collaboration altyapısı tamamlandı<br>
**Maliyet:** 0; ücretli plan, ödeme yöntemi veya Git LFS kotası alınmadı

## Erişim noktaları

- Authoritative private repository: [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D)
- Default branch: `main`
- Stage A etiketi: `stage-a-baseline-2026-08-11`
- Yol haritası: [PC Shop Empire 3D — Development Roadmap, Project #2](https://github.com/users/cixanla/projects/2)
- Epicler: [Issues #1–#22](https://github.com/cixanla/PC-Shop-Empire-3D/issues)
- Codex local Project: `Game`, `/Users/cixanla/Developer/PCShopEmpire3D/Game`, Git repository algısı `true`

Mevcut public `cixanla/PC-Shop-Empire` repository'si legacy release/indirme geçmişi olarak bırakıldı; silinmedi, force-push edilmedi ve yeni oyunun kaynağı yapılmadı. Daha önceden var olan boş/isimsiz GitHub Project #1 de sahipliği belirsiz olduğu için değiştirilmedi; yeni oyun için ayrılmış Project #2 oluşturuldu.

## Repository içeriği

- Unity 6000.3.21f1 + URP kaynakları, paket lock ve ProjectSettings.
- Saf `PSE.Core` temeli ve 42/42 geçen Edit Mode testleri.
- Root `PROJECT_BIBLE.md` ve 11 ayrıntılı `Docs/ProjectBible` belgesi.
- Developer handoff, governance, ADR, changelog, provenance ve teknik kanıtlar.
- Issue/PR şablonları, CODEOWNERS ve read-only Repository Guard workflow.
- Canonical PC Shop Empire 1.1.6 kaynağının 26/26 byte-exact private snapshot'ı ve SHA-256 manifesti.
- Build/cache/log/token/certificate/private key ve gerçek oyuncu verisi repository dışında.

## Yürütme modeli

22 üst seviye epic Project #2'ye bağlandı. Project alanları:

- `Status`: Todo, In Progress, Done
- `Phase`: Foundation, Retail, Assembly, Vertical Slice, AI, Service, Economy, Growth, Content, Alpha, Steam 1.0, macOS
- `Priority`: P0, P1, P2
- `Risk`: Low, Medium, High, Critical

Epicler kodlamadan önce küçük acceptance-odaklı issue'lara bölünür. GitHub Project günlük görünür durumdur; kalıcı tasarım ve mimari gerçeği `PROJECT_BIBLE.md`, `Docs/ProjectBible`, ADR ve provenance belgelerine geri yazılır.

## Doğrulama

- Private `main` normal push ile oluşturuldu; force-push ve history rewrite yapılmadı.
- `stage-a-baseline-2026-08-11` annotated etiketi remote'da doğrulandı.
- Remote Repository Guard workflow'u başarılı çalıştı.
- Private HTTPS fresh clone üzerinde repository guard geçti; çalışma ağacı temiz ve tracked kaynak bütünü bulundu.
- Legacy repository snapshot'ı 26/26 dosyada canonical manifestle eşleşiyor.
- Bilinen token/private-key kalıbı ve tracked cache/build yolu bulunmadı.
- GitHub CLI token değeri görüntülenmedi, loglanmadı veya repository/snapshot içine yazılmadı.
- GitHub Actions varsayılan workflow izni `read`; workflow pull request onaylayamaz.

## USB checkpoint

Stage A snapshot'ı değişmeden korunur. Bu collaboration kapanışı ayrı hedefe yazılır:

`/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_GITHUB_HANDOFF`

Snapshot `Game/` altında final Git commit'in bütün tracked kaynaklarını, `Planning/` altında güncel kullanıcı devir belgelerini taşır. Kesin dosya/bayt/hash kapsamı snapshot içindeki `MANIFEST.tsv` ve `MANIFEST.sha256` ile belirlenir; `.git`, cache, build, log ve credential dahil edilmez.

## Başka bilgisayarda devam

```bash
git clone https://github.com/cixanla/PC-Shop-Empire-3D.git
cd PC-Shop-Empire-3D
git switch main
./Tools/verify-repository.sh
```

Ardından tam `6000.3.21f1` Unity Editor sürümüyle proje açılır ve `Docs/DEVELOPER-HANDOFF.md` içindeki 42 testlik baseline çalıştırılır. İlk uygulama işi [Issue #2 — Deterministik RNG ve event bağlamı](https://github.com/cixanla/PC-Shop-Empire-3D/issues/2) olmalıdır; gameplay, asset, Steam/Apple ödemesi veya büyük binary migration aynı pakete alınmaz.
