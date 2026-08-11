# PC Shop Empire 3D — Stage A Teknik Kurulum Raporu

**Tarih:** 11 Ağustos 2026  
**Durum:** Yerel teknik temel ve Git checkpoint tamamlandı; UVCS ilk check-in beklemede  
**Maliyet:** 0 EUR/USD; ödeme yöntemi ve ücretli plan eklenmedi

## Yönetici sonucu

Yeni 3D oyun için legacy Electron projesinden tamamen ayrı, sıfırdan bir Unity teknik tabanı kuruldu. Proje Unity 6000.3.21f1, URP ve C# üzerinde açılıyor; dört Edit Mode temel testi geçiyor; macOS ve Windows x64 Mono development buildleri üretiliyor. macOS player komut satırı smoke testinde motor, giriş ve fizik katmanlarını başlatıp temiz biçimde kapandı.

Unity Cloud projesi ve ücretsiz/private UVCS deposu oluşturuldu. Unity hesabından UVCS kimlik değişimi başarıyla tamamlandı; ancak dosya protokolü bağlantısı hem bağımsız resmî istemcide hem Unity Editor entegrasyonunda uzak uç tarafından sıfırlandı. Bu yüzden yanlış veya yarım bir workspace/check-in oluşturulmadı. Sonraki güvenlik adımında mevcut Apple Git ile yerel `main` geçmişi başlatıldı; doğrulanmış USB snapshot off-device katmandır, cloud geçmişi henüz başlamamıştır.

Gameplay, 3D model, görsel içerik, Steam entegrasyonu ve Guardian runtime kodlaması bu aşamada başlatılmadı.

## Kurulan ve doğrulanan araçlar

| Bileşen | Sürüm / durum | Yerel etki |
|---|---|---:|
| Unity Hub | 3.20.1, resmî macOS uygulaması | yaklaşık 471 MiB |
| Unity Editor | 6000.3.21f1, Apple Silicon; revizyon `c02631ffc030` | Editor ağacı yaklaşık 10 GiB |
| Windows Build Support | Mono, `WindowsStandaloneSupport` | yaklaşık 973 MiB; Editor ağacına dahil |
| VS Code | 1.131.0 ARM64 | Önceden kurulu |
| VS Code for Unity | `visualstudiotoolsforunity.vstuc` 1.3.1 | Ücretsiz Microsoft uzantısı |
| C# Dev Kit | 3.20.199 | Ücretsiz kullanım koşulları içindeki geliştirme aracı |
| C# | 2.140.9 | Microsoft uzantısı |
| .NET Install Tool | 3.1.0 | Microsoft uzantısı |
| UVCS entegrasyonu | Unity paketi 2.13.6; `unityplastic` 11.0.16.10262 | Proje paketi; cloud check-in bağlantısı bekliyor |
| UVCS tanı istemcisi | `cm` 11.0.16.10330, client-only çalışma kopyası | Tam masaüstü/daemon kurulmadı |
| Yerel sürüm geçmişi | Apple Git 2.50.1; `main`, root commit `b7ac8c36`, etiketli Stage A baseline | Remote ve Git LFS yok; metadata etkisi yaklaşık 1 MiB'den küçük |

Blender, Steamworks SDK, Xcode, ücretli asset, ücretli IDE, crash SDK veya üçüncü taraf oyun paketi kurulmadı.

## Yeni proje

Proje kökü:

`/Users/cixanla/Developer/PCShopEmpire3D/Game`

Yerel build kökü:

`/Users/cixanla/Developer/PCShopEmpire3D/Builds/Local`

Temel ayarlar:

- Unity asset serialization: `Force Text`.
- Meta dosyaları: `Visible Meta Files`.
- Render pipeline: URP.
- Unity proje sürümü ve paket bağımlılıkları kilitli.
- `Library`, `Temp`, `Logs`, `UserSettings`, IDE çıktıları, `*.slnx` ve buildler ignore kapsamındadır.
- Legacy Electron kaynak yeni Unity ağacına kopyalanmadı.
- `Builds/Local`, planlandığı gibi Unity Git çalışma ağacının dışındadır.

Sabitlenmiş doğrudan paketler:

| Paket | Sürüm |
|---|---:|
| Universal Render Pipeline | 17.3.0 |
| Input System | 1.20.0 |
| AI Navigation | 2.0.14 |
| ProBuilder | 6.1.2 |
| Test Framework | 1.6.0 |
| Visual Studio Editor | 2.0.27 |
| Unity Version Control | 2.13.6 |

## Doğrulama sonuçları

### Otomatik test

11 Ağustos 2026 tarihli son Edit Mode koşusu:

- Toplam: 4
- Geçen: 4
- Başarısız: 0
- Atlanan: 0

Testler `Force Text`, `Visible Meta Files`, URP ataması ve `packages-lock.json` varlığını doğrular. UVCS denemesi proje modunu geçici olarak değiştirdiğinde test bunu yakaladı; ayar geri alındı ve temiz tekrar koşusu 4/4 geçti. Bu, test kapısının gerçek bir yapılandırma regresyonunu yakaladığını da kanıtladı.

### macOS development build

- Çıktı: `macOS/PC Shop Empire 3D.app`
- Tür: Universal Mach-O (`arm64` + `x86_64`)
- Unity rapor boyutu: 325.608.373 bayt
- Disk kullanımı: yaklaşık 311 MiB
- Ana binary SHA-256: `667db19ec9d71e1493ed412fc006a7323ae56834bb27e4e5e11f803a075254b5`
- Sonuç: Build başarılı; headless smoke açılışı ve temiz shutdown başarılı.

Bu paket imzalı/notarize edilmiş ticari macOS dağıtımı değildir.

### Windows development build

- Çıktı: `Windows-Mono-x64/PC Shop Empire 3D.exe`
- Tür: PE32+ GUI, Windows x86-64, Mono
- Unity rapor boyutu: 166.141.340 bayt
- Disk kullanımı: yaklaşık 159 MiB
- Ana EXE SHA-256: `c8b0d73dc40e4f2cddbf656cfb7257fcb8273da22e44e12a8694cd8e275c6fb2`
- Sonuç: Mac üzerinde cross-build başarılı.

Bu sonuç gerçek Windows çalıştırma, DirectX/GPU, sürücü, Steam veya IL2CPP kanıtı değildir. İlk oynanabilirden önce gerçek Windows 10/11 x64 bilgisayarda native Unity Editor + IL2CPP toolchain kurulmalı ve test edilmelidir.

## Unity Cloud ve UVCS durumu

| Alan | Sonuç |
|---|---|
| Unity Cloud proje adı | `PC Shop Empire 3D` |
| Project ID | `e429f8cf-ea69-43d6-8850-5505ac0c4edf` |
| Organization ID | `9072433432281` |
| UVCS organizasyon/server | `cixanlas@cloud` |
| Private repo | `PC Shop Empire 3D/pc-shop-empire-3d` |
| Plan | Ücretsiz Unity DevOps; kart/ücretli yükseltme yok |
| Kimlik değişimi | Başarılı; token yalnız süreç belleğinde programatik olarak işlendi, insan tarafından görüntülenmedi ve log/rapor/proje/snapshot içine yazılmadı |
| İlk check-in | Başarısız; cloud dosya-protokolü oturumu uzak uç tarafından sıfırlandı |
| Yerel `.plastic` workspace | Oluşmadı |

Hatanın neden zinciri:

1. Unity hesabı erişim bilgisi Hub üzerinden Editor'a ulaştı.
2. UVCS credential exchange `cixanlas@cloud` için başarıyla token üretti ve süreç bunu programatik olarak işledi; değer görüntülenmedi veya raporlanmadı.
3. Bölgesel UVCS hostuna TCP erişimi vardı; ilgili portlar açılabildi.
4. Repository sorgusu/Plastic protokolü başlarken uzak taraf soketi kapattı: `Unable to read data from the transport connection: Connection reset by peer.`
5. Aynı sonuç resmî standalone `cm` 11.0.16.10330 ve Unity paketindeki `unityplastic` yolunda tekrarlandı.
6. Bu nedenle sorun proje yolu, repo adı veya kimlik değişiminin ilk aşaması değil; ağ aracı/rota, hizmet bölgesi ya da protokol oturumu katmanındadır. Kesin dış neden yalnız istemci logundan belirlenemez.

UVCS için tekrarlı bağlantı denemesi yapılmaz. Yerel Git şimdilik tek authoritative geçmiş olarak kullanılır; UVCS workspace/changeset oluşturulmaz. Uzak Git remote'u ve Git LFS, büyük binary asset kabulünden önce maliyet/kota/erişim etkisiyle ayrı kapıda seçilir.

Editor içindeki ilk check-in yardımcısı otomatik çalışmaz ve klavye kısayolu taşımaz. Çalıştırıldığında repo ile proje yolunu gösteren açık insan onayı ister; uzak reponun var ve boş olduğunu, mevcut workspace/yol çakışması bulunmadığını, oluşturulan eşlemenin doğru repoya gittiğini ve check-in öncesi uzak durumun değişmediğini doğrular. Başarısızlık süreç tarafından başarı gibi yutulmaz.

## USB ve legacy bütünlüğü

Canonical legacy kaynak 11 Ağustos 2026'da gömülü 26 dosyalık manifestle tekrar karşılaştırıldı:

- Dosya: 26/26
- Göreli yol farkı: 0
- Boyut farkı: 0
- SHA-256 farkı: 0
- Kasıtlı legacy kaynak yazma işlemi: yapılmadı; güncel içerik 26/26 dosyada manifestle aynıdır

Yeni Stage A kaynak snapshot hedefi:

`/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`

Snapshot yalnız tekrar üretilemeyen/kaynak niteliğindeki `Game` dosyalarını ve yaşayan planlama belgelerini içerir. `Library` (~2,5 GiB), temp/log/cache, IDE çıktıları, canlı VCS metadata, iş klasörü ve yaklaşık 470 MiB'lik yeniden üretilebilir buildler dahil edilmez.

USB hedefi yeni ve ayrı olarak oluşturuldu. `MANIFEST.tsv`, her kaynak dosya için SHA-256, mantıksal bayt ve göreli yol kaydeder. Kaynak iki ardışık senkron/hash geçişinde sabit kaldı; USB geri okuma hash/size doğrulaması ile kaynak karşılaştırmalı iki `rsync --dry-run --checksum --delete` kontrolü geçti. Kesin dosya sayısı ve mantıksal toplam snapshot içindeki manifestte bulunur.

## Güvenlik ve kapsam doğrulaması

- USB'deki eski oyun klasörü değiştirilmedi.
- Legacy kaynak yeni projeye port edilmedi veya üzerine yazılmadı.
- Credential/token değeri log, rapor, repo veya snapshot içine alınmadı.
- Cloud repo private; ücretli plan ve ödeme yöntemi yok.
- Tam UVCS masaüstü uygulaması/daemon kurulmadı; yalnız resmi Editor entegrasyonu ve client-only tanı aracı kullanıldı.
- Asset Store/üçüncü taraf oyun içeriği eklenmedi.
- Gameplay üretimi başlamadı; Stage A yalnız teknik boş temel ve doğrulamadır.

## Kabul özeti

| Kapı | Durum |
|---|---|
| Resmî Unity Hub/Editor ve doğru sürüm | Geçti |
| Onaylı Windows Mono modülü | Geçti |
| Proje yolu ve legacy ayrımı | Geçti |
| Force Text / Visible Meta Files / URP / lock | Geçti |
| Edit Mode testleri | Geçti — 4/4 |
| macOS development build + smoke | Geçti |
| Windows x64 Mono cross-build | Geçti |
| Legacy 26/26 bütünlük | Geçti |
| Yeni USB snapshot + readback hash | Geçti — `MANIFEST.tsv` ve kaynak dry-run kontrolleri |
| Private UVCS ilk check-in | Bekliyor — uzak bağlantı reseti |
| Gerçek Windows x64 runtime/IL2CPP testi | İlerideki zorunlu dış bağımlılık |

Stage A'nın yerel teknik temeli sağlamdır ve etiketli Git checkpoint ile geri alınabilir durumdadır. UVCS cloud ilk check-in'i bekleyen bir altyapı istisnasıdır; bugün yerel Git + doğrulanmış USB snapshot ile maskelenmeden izlenir.
