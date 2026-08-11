# Repository Governance ve Kaynak Gerçeği

**Durum:** Aktif  
**Tarih:** 11 Ağustos 2026

## Authoritative sistemler

| Alan | Authoritative kaynak |
|---|---|
| Yeni Unity oyunu | Private `cixanla/PC-Shop-Empire-3D`, `main` branch |
| Oyun vizyonu ve güncel durum | Root `PROJECT_BIBLE.md` |
| Ayrıntılı tasarım/araştırma | `Docs/ProjectBible/` |
| Kalıcı teknik kararlar | `Docs/ADR-*` |
| Görev ve görünür ilerleme | GitHub Issues + PC Shop Empire 3D Development Roadmap Project |
| Legacy 1.1.6 | `LegacyReference/.../Source` + `CANONICAL-MANIFEST.tsv`; salt okunur |
| Eski public indirme alanı | `cixanla/PC-Shop-Empire`; yeni oyunun development kaynağı değildir |
| Off-device milestone | Private GitHub remote + tarihli SHA-256 USB snapshot |

UVCS repo oluşturulmuş olsa da ilk check-in uzak bağlantı reseti nedeniyle tamamlanmamıştır. `.plastic` workspace/changeset yoktur; UVCS ikinci authoritative sistem olarak çalıştırılmaz.

## Branch politikası

- `main`: doğrulanmış, devredilebilir checkpoint.
- Feature/fix/docs branch: issue'ya bağlı, küçük ve geri alınabilir paket.
- `main` force-push ve history rewrite yok.
- Merge öncesi test, repo guard, belge ve provenans kontrolü.
- Repository tek geliştiriciliyken branch protection operasyonu kilitlemeyecek şekilde hafif tutulur; ilk collaborator eklendiğinde required review/ruleset ayrıca etkinleştirilir.

## Yaşayan bilgi politikası

Kod ve plan ayrı gerçekliklere dönüşemez. Her material push:

1. GitHub issue/kabul ölçütü taşır.
2. Test/manuel doğrulama sonucunu kaydeder.
3. `PROJECT_BIBLE.md` içindeki tamamlanan, sıradaki veya risk bölümünü günceller.
4. Gerekiyorsa ayrıntılı ProjectBible belgesi ve ADR'yi günceller.
5. Asset/paket/veri değişiminde `Docs/PROVENANCE.md` kaydı ekler.
6. Repo/yayın/kullanıcı görünür sonucu için `CHANGELOG.md` günceller.

Issue yorumu kalıcı mimari kararın tek kaynağı olamaz. Karar kapandığında repository belgesine taşınır.

## Dahil edilenler

- Unity kaynakları, `.meta` dosyaları, paket manifest/lock ve ProjectSettings.
- Testler ve proje doğrulama araçları.
- Bütün yaşayan tasarım, araştırma, yol haritası, karar ve checkpoint belgeleri.
- Hash doğrulanmış legacy kaynak snapshot'ı ve hak/third-party notice kayıtları.
- Düzenlenebilir kaynak assetler; ancak provenans ve lisans kaydıyla.

## Hariç tutulanlar

- `Library`, `Temp`, `Logs`, `UserSettings`, `Obj`, IDE çıktıları.
- Yerel ve release build binary'leri; GitHub Releases veya dış artifact alanı kullanılır.
- Token, password, API key, certificate, provisioning profile ve private key.
- Gerçek oyuncu telemetry/crash ham verisi.
- Lisansı veya kaynağı doğrulanmamış asset.

## Binary ve Git LFS kapısı

Git LFS şu anda kurulmamıştır. Mevcut legacy görselleri 2 MB altında ve toplam repo küçük olduğundan normal Git içinde tutulur.

Tek bir yeni binary yaklaşık 10 MB'a yaklaşmadan veya toplu asset üretimi başlamadan önce:

- Kaynak/işlenmiş artifact ayrımı,
- LFS storage/bandwidth etkisi,
- collaborator erişimi,
- marketplace lisansının private repo paylaşım sınırı,
- yedek ve migration planı

ayrı kararla kilitlenir. Git geçmişine büyük binary girdikten sonra sonradan LFS migration yapılması normal akış sayılmaz.

## Erişim ve collaborator

- Collaborator yalnız gerekli GitHub rolüyle eklenir; varsayılan write erişimi sınırsız yayın yetkisi değildir.
- Katkı başlamadan proprietary hak, gizlilik ve katkının kullanım hakkı yazılı olarak netleştirilir.
- Ayrılan collaborator erişimi kaldırılır; credential/secret erişimi varsa rotate edilir.
- Public'e açma ayrı marka, lisans, secret-history ve release readiness incelemesi ister.

## Checkpoint ve geri alma

Her milestone için:

- Temiz Git commit ve mümkünse annotated tag.
- Edit Mode/test/build kanıtı.
- GitHub remote push doğrulaması.
- USB snapshot için kaynak manifesti, geri-okuma hash'i ve cache/secret dışlama kontrolü.
- `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md` güncellemesi.

Bir sorun olduğunda history silinmez; yeni fix/revert commit'i veya güvenli branch kullanılır. Destructive reset/force-push yalnız açık sahip onayı ve ayrıca yedekle mümkündür.
