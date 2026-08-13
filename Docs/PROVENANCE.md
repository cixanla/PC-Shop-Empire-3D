# Lisans ve provenans defteri

Bu proje çalışma ağacı, projeye giren her paket, asset, model, doku, ses, font ve dış kaynağın kökenini izler. Kaynak, sürüm, lisans, edinme tarihi ve mümkünse hash kaydedilmeden üretim içeriği kabul edilmez.

## Stage A kayıtları

| Öğe | Sürüm | Kaynak | Tür / lisans notu | Tarih |
|---|---:|---|---|---|
| Unity Editor | 6000.3.21f1 ARM64 | Unity Hub, resmî Unity CDN | Unity Personal koşulları | 2026-08-11 |
| 3D URP blank template | 17.0.14 | Editor ile gelen resmî proje şablonu | Unity resmî şablonu | 2026-08-11 |
| Universal Render Pipeline | 17.3.0 | Unity built-in package | Resmî Unity paketi | 2026-08-11 |
| Input System | 1.20.0 | packages.unity.com | Resmî Unity paketi | 2026-08-11 |
| AI Navigation | 2.0.14 | packages.unity.com | Resmî Unity paketi | 2026-08-11 |
| ProBuilder | 6.1.2 | packages.unity.com | Resmî Unity paketi | 2026-08-11 |
| Test Framework | 1.6.0 | Unity built-in package | Resmî Unity paketi | 2026-08-11 |
| Visual Studio Editor | 2.0.27 | packages.unity.com | Resmî Unity paketi | 2026-08-11 |
| Unity Version Control integration | 2.13.6 | packages.unity.com | Resmî Unity paketi | 2026-08-11 |

Stage A'da üçüncü taraf oyun asset'i, font, ses, model veya oynanış kodu eklenmedi.

## GitHub devir paketi

| Öğe | Kaynak | Hak / kullanım notu | Tarih |
|---|---|---|---|
| Legacy PC Shop Empire 1.1.6 kaynak snapshot'ı | Canonical USB `KAYNAK_KODU`; 26/26 SHA-256 doğrulaması | cixanla proprietary materyalleri `GAME_LICENSE.txt`; Electron/Chromium/Node ve diğer üçüncü taraflar `THIRD_PARTY_NOTICES.txt` koşullarını korur | 2026-08-11 |
| Project Bible araştırma/tasarım belgeleri | Bu projenin yaşayan Codex çalışma çıktıları; kaynak defteri `Docs/ProjectBible/07_KAYNAKLAR.md` | Rakip içerikleri kopyalanmaz; olgu, kaynak ve tasarım çıkarımı ayrılır | 2026-08-11 |
| Repository governance/handoff metinleri | Proje için özgün hazırlanmış belgeler | cixanla proje materyali | 2026-08-11 |

Legacy snapshot içindeki `assets/` dosyaları yalnız private tarihsel kanıt/refereans bağlamında tutulur. Yeni oyunun production asset'i sayılmaz ve yeni Unity sahnelerinde kullanılmaz.

## Stage B algoritma referansları

| Öğe | Kaynak | Hak / kullanım notu | Tarih |
|---|---|---|---|
| PCG XSH-RR 64/32 set-sequence algoritması ve golden vector | <https://www.pcg-random.org/using-pcg-c-basic.html>, <https://www.pcg-random.org/download.html> | Resmî minimal C referansı Apache License 2.0; C# uygulaması proje için özgün yazıldı, davranış `ADR-0007` ile sürümlendi | 2026-08-13 |
| SHA-256 bağlamsal stream derivation | NIST FIPS 180-4 ve .NET `System.Security.Cryptography.SHA256` API | .NET BCL uygulaması kullanıldı; repository'ye üçüncü taraf hash kodu/paketi kopyalanmadı, binary sözleşme `ADR-0008` ile sürümlendi | 2026-08-13 |
