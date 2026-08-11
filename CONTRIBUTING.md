# Katkı ve İş Birliği Kuralları

Bu depo private ve proprietary bir projedir. Erişim verilmesi kaynak kodunu, belgeleri, görselleri veya oyun fikirlerini başka yerde kullanma/yayımlama hakkı vermez.

## Başlamadan önce

1. `PROJECT_BIBLE.md` ve `Docs/DEVELOPER-HANDOFF.md` dosyalarını okuyun.
2. [GitHub Development Roadmap Project](https://github.com/users/cixanla/projects/2) içindeki atanmış issue ve kabul ölçütünü doğrulayın.
3. Aynı alan üzerinde açık branch/PR olup olmadığını kontrol edin.
4. `main` branch'te doğrudan deneysel çalışma yapmayın.

## Branch ve commit

- Branch: `feature/<issue>-kisa-ad`, `fix/<issue>-kisa-ad`, `docs/<issue>-kisa-ad`.
- Bir branch tek ana problemi çözer.
- Generated Unity klasörleri, buildler ve credential commit edilmez.
- Legacy snapshot doğrudan düzenlenmez; değişiklik gerekiyorsa yeni canonical snapshot ve ADR gerekir.
- Force-push yalnız kişisel feature branch'te ve ortak çalışan uyarıldıktan sonra; `main` üzerinde yasaktır.

## Kod sınırları

- Domain kuralları mümkün olduğunca saf C# assembly'lerinde kalır.
- `PSE.Core`, `UnityEngine` veya `UnityEditor` referansı alamaz.
- Beklenen iş kuralı başarısızlıkları stable failure code kullanır.
- Kalıcı kimlikler stable ve tür kapsamlıdır; görünen ad save anahtarı değildir.
- Wall-clock, rastgelelik ve platform API'ları interface/adaptör arkasındadır.
- Silent recovery, para/ürün yaratma veya insan onayı olmadan kod değiştiren AI eklenmez.

## Test ve doğrulama

Her PR en az şunları içerir:

- Değişen davranış için otomatik test.
- Mevcut Edit Mode testlerinin tamamının geçmesi.
- `./Tools/verify-repository.sh` sonucu.
- Fizik/UI değişiminde kısa manuel test senaryosu ve sonuç.
- Save şeması etkileniyorsa migration/fault-injection planı.

Unity test raporları ve build çıktıları kalıcı kaynak değildir; Git'e eklenmez. PR açıklamasında sonuç ve kullanılan commit yazılır.

## Yaşayan belge zorunluluğu

Material değişiklikte:

- `PROJECT_BIBLE.md` güncel durum/sıradaki iş/risk bilgisi güncellenir.
- Kalıcı teknik karar ADR'ye yazılır.
- Oyun tasarımı değişiyorsa ilgili `Docs/ProjectBible` belgesi güncellenir.
- Paket/asset/veri eklendiyse `Docs/PROVENANCE.md` güncellenir.
- Repo yapısı veya kullanıcıya görünen sonuç değiştiyse `CHANGELOG.md` güncellenir.

Kapsamı etkilemeyen küçük refactor için Bible değişikliği gerekmiyorsa PR'da gerekçe yazılır.

## Asset ve lisans

- Kaynak, üretici, sürüm, lisans, edinme tarihi ve mümkünse SHA-256 bilinmeden asset kabul edilmez.
- Marketplace lisansının GitHub collaborator erişimine izin verdiği doğrulanır.
- Gerçek marka/logo ve rakip oyundan kopya içerik kullanılmaz.
- AI üretimi içerik varsa kullanılan araç, tarih, girdi kaynağı ve ticari kullanım koşulu provenansa yazılır.

## İnceleme kapıları

Şunlar proje sahibi onayı olmadan merge edilmez:

- Motor/LTS veya render pipeline değişimi.
- Yeni ücretli araç/asset/servis.
- Save schema kırılması veya veri migration'ı.
- Telemetry, crash SDK, online servis veya yeni veri toplama.
- Steam/Apple entegrasyonu ve mağaza/yayın ayarı.
- Lisans, marka, oyun adı, fiyat/monetization veya platform kapsamı.
- Büyük binary ve Git LFS politikası.
