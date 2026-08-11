# PC Shop Empire 3D – Canonical Kaynak ve Kesin Kurulum Planı

**Belge durumu:** Onaylanmış tarihsel teknik plan 0.2 — Stage A uygulanmıştır  
**Tarih:** 11 Ağustos 2026  
**Kural:** Bu belge, uygulanmadan önceki maliyet/kapsam/onay planını tarihsel olarak korur. Kullanıcı daha sonra kapsamı açıkça onaylamış ve Stage A uygulanmıştır; güncel gerçek sonuç [Stage A Teknik Kurulum Raporu](09_STAGE_A_KURULUM_RAPORU.md) içindedir.

> **Yürütme notu:** Yerel Unity temeli, 4/4 test, macOS ve Windows Mono development buildleri ile yeni USB snapshot tamamlandı. Unity Cloud/private UVCS repo oluşturuldu; fakat ilk check-in iki resmî istemci yolunda aynı uzak `connection reset by peer` hatası nedeniyle bekliyor. Bu istisna kurulum raporunda neden zinciriyle kayıtlıdır.

## 1. Yönetici sonucu

Kaynak güvenlik kapısı başarıyla tamamlandı:

- USB'deki canonical `KAYNAK_KODU` ile yerel inceleme kopyası **26/26 dosyada** eşleşir.
- Göreli yol, mantıksal dosya boyutu ve SHA-256 karşılaştırmasında eksik, fazla veya farklı dosya yoktur.
- Canonical doğrulama sırasında USB'deki legacy oyun yollarına, canlı kayıtlara ve Mac paketleme kopyasına yazılmadı; Stage A'da yalnız ayrı ve yeni `90_BACKUPS/PCShopEmpire3D` hedefi oluşturuldu.
- Mac paketleme çalışma kopyası canonical değildir; macOS hazırlıkları nedeniyle ayrışan ayrı bir türevdir.

Önerilen ilk kurulum paketi:

1. En güncel kararlı Apple Silicon **Unity Hub**.
2. Sabitlenmiş **Unity 6000.3.21f1 / Unity 6.3 LTS**, Apple Silicon Editor.
3. Yalnız **Windows Build Support (Mono)** hedef modülü.
4. Zaten kurulu VS Code 1.131.0 içine Microsoft'un Unity/C# geliştirme uzantıları ve gereken .NET 10 SDK.
5. Yeni proje için **Unity Version Control (UVCS)** ücretsiz cloud katmanı ve yerel workspace.
6. Sıfır üçüncü taraf asset/paket; yalnız resmî Unity paketleri ve boş URP teknik proje.

Bu ilk paketin lisans/abonelik maliyeti **0 EUR/USD** olmalıdır. Kredi kartı veya ücretli deneme başlatılmayacaktır.

## 2. Canonical legacy kaynak kararı

### 2.1 Canonical USB snapshot

`/Volumes/cixanla/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

### 2.2 Doğrulanmış yerel ayna

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/transfer_review/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

### 2.3 Doğrulama özeti

| Ölçüt | Sonuç |
|---|---|
| USB dosya sistemi | exFAT; yalnız okuma yöntemiyle incelendi |
| Dosya sayısı | 26 |
| Toplam mantıksal boyut | 4.762.385 bayt / yaklaşık 4,542 MiB |
| USB'de olup yerelde olmayan | 0 |
| Yerelde olup USB'de olmayan | 0 |
| SHA-256 farklılığı | 0 |
| Canonical karar | USB `KAYNAK_KODU` legacy snapshot; yerel klasör birebir doğrulanmış ayna |

Bu karar yalnız 11 Ağustos 2026'da ölçülen snapshot içindir. Kaynaklardan herhangi biri daha sonra değiştirilirse manifest yeniden üretilmeden “aynı” kabul edilmez.

### 2.4 Mac paketleme türevi

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/builds/pc-shop-empire-mac`

Ana oyun mantığı ve varlıkların çoğu canonical kaynakla eşleşir. Aşağıdaki beş dosya farklıdır:

- `THIRD_PARTY_NOTICES.txt`: daha ayrıntılı bağımlılık/lisans envanteri.
- `forge.config.js`: DMG maker, legal dosya kopyalama ve yerel ad-hoc imza hook'u.
- `package-lock.json`: DMG maker ve ilişkili bağımlılıklar.
- `package.json`: macOS DMG/ad-hoc imza script'leri.
- `styles.css`: Apple sistem fontu fallback'leri.

Bu farkların hiçbiri USB kaynağının üzerine uygulanmadı. Yeni 3D oyunda eski Electron kopyalarından kod port edilmeyecektir.

## 3. Canonical USB SHA-256 manifesti

Biçim: `SHA-256 | bayt | göreli yol`

```text
076905338d55e745bb7f8161722d97565ddc7e47e19f60137d2abd6f456dd8b3 |       3619 | CHANGELOG.md
18dead96491b7206374282bf41a43dd771c9dfcfbadd0862bbfd9f988535f5c6 |       2324 | GAME_LICENSE.txt
ae6e23e70ff2cade103545b56695d83083d2fa0cc691b866610002de18c3cf30 |       3352 | README.md
329ffaff96765eb361e931d4ff0b4dc69bd6bad6b9abe9837e6245be678cab09 |       1621 | THIRD_PARTY_NOTICES.txt
928e325190d513724eb44dd3d13f5b0b8a6bbcf5fd84622baada30569d09b8e0 |     270398 | assets/icon.ico
6f195ade7edc7a90d02b663662ae34fce8eb0e5776870440ef462f7fd24147dd |    1475786 | assets/icon.png
4e6726d75171181cc5f1174cb260e389f6ae66e7defcd2a974596ef4b52e63d4 |    1937884 | assets/main-menu-shop-1.1.6.png
7560423ba0692381293615674ae36f9f977b77d9121bc807bc6eddd94b805c1b |       2737 | docs/MACOS_BUILD_GUIDE.md
724ce0493c17b9bdce89db701757f2976051c84fb8a15a2c1efec72628ba893b |       5638 | docs/OWNERSHIP_AND_REGISTRATION.md
e0d81aa790d54d8171165fd46d741eb73a4d734d2ac0bd71af4d48588bd95b98 |       3492 | docs/RELEASE_NOTES_1.1.5.md
1b1d55c46e3454e89f4f06cb8556d620c8a71ebd7ec4c2ecda14d20935de35ed |       3608 | docs/RELEASE_NOTES_1.1.6.md
7203b40b2febcc65d303de66261030af21e7b45a1bca17c21dd6cf99bd34b4dd |       2732 | forge.config.js
089d012b2dba436e4591a582ac500e7f9567d030965763e215247c0efe38d85e |     511003 | game.js
7c536123714ce31fbe51da8d78f56949410531ca6a77bed12cf8350685ad6429 |      25488 | index.html
4598852dd5aebfe8ebab96a31dd6f58acc0f0774794a8b04b922742806537bd3 |       3400 | main.js
a43ca85d8e9d273b320365a578b3efac3ceacf45509fd011f383160d3a3f68ba |     252159 | package-lock.json
d16bfe86ba169bb24da17df24f163e570b8a0229ce0b94052af2b5fbfa458b52 |       1175 | package.json
27266d7a504f9e87a7edf2fbe9206eba8f33978ae552052d26f4252149e57af7 |        485 | preload.js
9ca40b15d4f517dd4c2cdfa8a497c280085d8a12d0bc3f969c1c545eeb2d4334 |      15152 | src/release-bootstrap.js
942c75c83f8402a733b248355f2aa22ac201193d9f0d96408fc79071b6ca753b |      34442 | src/release-data.js
c99973428927a39abeb3f4b7fe8bb415384c697672239cf902f7593ef87ce058 |      23874 | src/release-settings.js
c4d72c9bc73590f92f886264c1bdd4eb322de01e75df0bc1266aa1a60faaaa96 |      51780 | src/release-systems.js
5e151ebaceebf69d4de4000a1300fea09a42a21ba22e0a337912a1fca4273994 |      85767 | styles.css
07bc2e186b7d157af4677dde49dbd559bbafa2117f9d8f0c2e36a9fe1eea5a38 |      32303 | styles/release-1.1.6.css
62d6c87a328a5a480708ea46b8a5e5ad1c9939adb7b51460f488e463787efd05 |       2830 | tests/simulation-test.js
c9d51697aae78a9c494bc37c8f737413c9eee3dd3be32c91af2e7f80ba74c434 |       9336 | tests/smoke-test.js
```

## 4. Geliştirme bilgisayarı envanteri — kurulum öncesi tarihsel snapshot

Aşağıdaki tablo Stage A kurulmadan önceki envanteri korur; güncel kurulu durum `09_STAGE_A_KURULUM_RAPORU.md` içindedir.

Gizlilik nedeniyle donanım seri numarası, UUID ve cihaz kimlikleri belgeye kaydedilmedi.

| Alan | Doğrulanan durum |
|---|---|
| Bilgisayar | MacBook Air, Apple M4, 10 çekirdek |
| Bellek | 32 GB |
| İşletim sistemi | macOS 26.6 |
| İç SSD boş alan | Yaklaşık 799 GiB |
| USB boş alan | Yaklaşık 117 GiB; exFAT |
| Rosetta 2 | Kurulu |
| Apple Command Line Tools | 26.6; kurulu |
| Git | Apple Git 2.50.1; kurulu |
| VS Code | 1.131.0 ARM64; kurulu; uzantı yok |
| Unity Hub/Editor | Kurulu değil |
| Blender | Kurulu değil |
| Git LFS | Kurulu değil |
| Tam Xcode | Kurulu değil; macOS yayın aşamasına kadar gerekli değil |
| Time Machine | Hedef yapılandırılmamış |

macOS'ta Desktop/Documents iCloud optimizasyonu etkin görünüyor. Unity'nin yoğun değişen `Library` ve cache alanları iCloud veya exFAT üzerinde tutulmayacaktır.

## 5. İlk kurulumda seçilen araçlar — tarihsel plan, uygulandı

Disk rakamları üretici tarafından sabit garanti edilmez; sürüm, sıkıştırma ve ilk import cache'ine göre değişir. Aşağıdaki aralıklar planlama tahminidir ve kurulum ekranında görülen gerçek değer ayrıca raporlanacaktır.

| Bileşen | Kesin seçim | Neden | Maliyet | Tahmini yerel disk |
|---|---|---|---:|---:|
| Unity Hub | Kurulum günündeki en güncel **kararlı Apple Silicon** sürüm | Editor, modül ve lisans yönetimi; beta kanalına girilmez | 0 | 0,5–1,5 GB |
| Unity Editor | **6000.3.21f1, macOS ARM64** | 29 Temmuz 2026 tarihli güncel 6.3 LTS yaması; proje sürümü sabitlenebilir | Unity Personal koşullarında 0 | 8–12 GB |
| Render pipeline | **URP** boş 3D template | Hedef görsel kalite ile orta sınıf Windows performansı dengesi | 0 | Editor/proje içinde |
| Windows modülü | **Windows Build Support (Mono)** | Mac'ten erken Windows x64 build; IL2CPP değildir | 0 | 3–6 GB |
| Kod editörü | Mevcut **VS Code 1.131.0 ARM64** | Yeniden IDE indirmeye gerek yok | 0 | Zaten kurulu |
| VS Code Unity desteği | Microsoft `VisualStudioToolsForUnity.vstuc` + C# Dev Kit/C#/.NET Install Tool | IntelliSense, analiz ve Unity debugger | Bireysel/takım ≤5 koşulunda 0 | 1–3 GB; .NET ile |
| .NET | Uzantının istediği **.NET 10 SDK** | Güncel VS Code Unity uzantısı gereksinimi | 0 | Yukarıdaki aralığa dahil |
| Sürüm kontrolü | **Unity Version Control cloud + yerel workspace** | Büyük binary, kilitleme ve Unity uyumu; 2026 ücretsiz katmanda 25 GB | Kota içinde 0 | Client/metadata yaklaşık 0,5–2 GB + repo |
| Yerel geliştirme araçları | Mevcut Git ve Apple CLT | Hash, diff ve genel yardımcı komutlar | 0 | Zaten kurulu |

### Kurulmayacak Unity modülleri

İlk aşamada Android, iOS, tvOS, visionOS, Web, Linux, dedicated server, offline documentation ve dil paketleri seçilmeyecek. Tam Xcode kurulmayacak. Böylece onlarca GB gereksiz yük ve güncelleme yüzeyi oluşmayacak.

### İlk projede izin verilen resmî paketler

Paket sürümleri Unity 6000.3.21f1'in kendi registry/lock çözümünden seçilip `packages-lock.json` ile sabitlenir:

- Universal Render Pipeline.
- Input System.
- Unity Test Framework.
- AI Navigation.
- ProBuilder — yalnız graybox/ölçek kanıtı için.
- Visual Studio Editor package 2.0.20 veya Unity 6.3 ile uyumlu daha yeni kararlı sürüm.

Localization, Addressables, Steam entegrasyonu ve diğer resmî paketler ihtiyaç doğduğu milestone'da eklenir. İlk projeye Asset Store paketi, reklam/analytics/IAP, Unity AI Agent/Muse, üçüncü taraf model, networking veya crash SDK eklenmez.

## 6. Şimdilik ertelenen araçlar

| Araç/aşama | Karar | Neden | Tahmini etki |
|---|---|---|---:|
| Blender | İlk kurulumda yok | 11 Ağustos 2026'da resmî Blender kanalında 5.2 LTS hâlâ Release Candidate olarak görünür; ilk teknik prototip Unity primitive/ProBuilder ile yapılabilir | Kararlı sürümde yaklaşık 1–3 GB + kaynak assetler |
| Krita/GIMP | Ertelendi | İlk graybox için gereksiz | Sonra yaklaşık 1–3 GB |
| Audacity/REAPER | Ertelendi | Ses prototipine kadar gereksiz; REAPER ücretli karar ister | Sonra 1 GB altı + ses kaynakları |
| Steamworks SDK/Steam client | Ertelendi | AppID ve Steam özellikleri vertical slice'a yaklaşınca | SDK küçük; client/build cache ayrıca |
| Xcode | Windows 1.0 sonrası macOS portuna ertelendi | Şimdi Windows hedefi için gerekli değil | Yaklaşık 15–30+ GB |
| Rider | Alınmayacak | VS Code yeterli; ancak ölçülmüş verim darboğazında ayrı ücret/onay kapısı olabilir | Ücret + birkaç GB |
| Sentry/native crash SDK | Alınmayacak | Guardian kapsamından ayrı lisans/gizlilik kararı gerekir | Daha sonra ayrı kapı |

Blender 5.2 LTS resmî kararlı sürüme geçtiğinde sürüm, indirme boyutu ve lisans tekrar gösterilmeden kurulmaz. RC/beta ile üretim asset'i başlatılmaz.

## 7. Neden UVCS; alternatifler

### Önerilen: Unity Version Control

- Unity sahne/prefab ve büyük binary varlıklar için kilitleme akışı sunar.
- 2026 fiyat güncellemesi ücretsiz cloud depolamasını organizasyon başına 25 GB'a, ücretsiz egress'i ayda 100 GB'a çıkarır ve public-cloud seat ücretlerini kaldırır.
- GitHub Free Git LFS'nin 10 GiB depolama/10 GiB aylık bant kotasına göre vertical slice'a daha fazla alan bırakır.
- Unity hesabı/bulutu bağımlılığı yaratır; bu nedenle tarihli USB snapshot'ları ve geri yükleme deneyi zorunludur.

Kota uyarıları:

- 15 GB: asset büyüme incelemesi.
- 20 GB: temizleme ve dış arşiv planı.
- 23 GB: yeni büyük asset alımını durdur; ücret/migrasyon kararını kullanıcıya getir.
- Ödeme yöntemi, otomatik ücretlendirme veya ücretli katman kullanıcı onayı olmadan etkinleştirilmez.

### Alternatif: Git + Git LFS

Avantajı açık standart, yaygın araç ve kolay kod incelemesidir. Ancak GitHub Free LFS 10 GiB storage ve 10 GiB/ay bandwidth içerir; değiştirilmiş büyük binary sürümleri depolamayı hızlı tüketir. Güncel güvenlik düzeltmeli Git LFS 3.7.1 ayrıca kurulmalıdır. Büyük görsel proje için ücretsiz kotası UVCS'den daha erken daralır.

### Alternatif: Perforce

Binary/locking tarafında güçlüdür, fakat küçük solo başlangıçta sunucu, bakım, yedek ve erişim yönetimi gereksiz operasyon yükü yaratır.

### Kural

Aynı Unity çalışma ağacında UVCS ve Git birlikte “iki ana sürüm kontrolü” olarak çalıştırılmaz. İlk seçim UVCS'dir. Kaynak dışa aktarma ve platformdan ayrılma imkânı milestone yedekleriyle korunur.

## 8. Yeni projenin fiziksel konumu

Önerilen repo/workspace kökü:

`/Users/cixanla/Developer/PCShopEmpire3D`

Bu yol iCloud Documents ve USB'nin dışındadır. Çalışma adı final marka kararı değildir.

```text
PCShopEmpire3D/
├── Game/                 # Unity proje kökü
│   ├── Assets/
│   ├── Packages/
│   └── ProjectSettings/
├── SourceAssets/         # İleride .blend, doku ve ses kaynakları
├── Docs/                 # Proje içi karar, ADR ve üretim notları
├── Tools/                # Yalnız kaynak script/config; üçüncü taraf binary yok
└── Builds/Local/         # Sürüm kontrolü dışında yerel build çıktıları
```

Legacy Electron kaynak bu ağaca kopyalanmaz. Yalnız canonical manifest, kaynak yolu ve davranış/dönüşüm belgeleri referans gösterilir.

UVCS dışında bırakılacak başlıca Unity alanları:

- `Library/`
- `Temp/`
- `Logs/`
- `obj/`
- `UserSettings/`
- `MemoryCaptures/`
- `Builds/Local/`
- IDE cache ve işletim sistemi geçici dosyaları

`Assets`, `Packages`, `ProjectSettings`, `.meta` dosyaları, kaynak assetler ve lisans/provenans defteri sürüm kontrolünde olacaktır. Unity asset serialization **Force Text**, meta dosyaları **Visible Meta Files** yapılır.

## 9. Yedekleme düzeni

Time Machine hedefi bulunmadığı için UVCS tek başına yeterli kabul edilmez.

Planlanan nihai düzen üç katmanlıdır; fakat bugün fiilen doğrulanmış kaynak koruması yerel çalışma kopyası ile USB snapshot'ıdır. Private UVCS repo oluşturulmuş olsa da ilk check-in yapılmadığından henüz kaynak yedeği veya sürüm geçmişi değildir.

1. **Çalışma kopyası:** İç APFS SSD'deki `/Users/cixanla/Developer/PCShopEmpire3D`.
2. **Hedeflenen offsite/sürüm geçmişi:** Güvenli ilk check-in başarıyla tamamlandıktan sonra kullanıcının private UVCS cloud reposu.
3. **Harici snapshot:** USB'de yalnız yeni ve ayrı bir hedef:

   `/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/`

USB exFAT olduğu için canlı Unity workspace veya canlı VCS veritabanı burada çalıştırılmaz. Yalnız milestone bazlı, tarihli kaynak snapshot + SHA-256 manifest saklanır. `Library`, temp ve build cache snapshot'a alınmaz. Eski `02_PC_SHOP_EMPIRE...` klasörüne hiçbir dosya eklenmez.

Her önemli milestone'da en az bir geri yükleme deneyi yapılır. Dosyanın USB'de bulunması, açılıp hash'i doğrulanmadan “yedek başarılı” sayılmaz.

## 10. Tarihsel uygulama sırası — 11 Ağustos 2026'da yürütüldü

Onay öncesinde planlanan işlem sırası aşağıdaydı; fiili sonuç ve UVCS istisnası §19'da kayıtlıdır:

1. USB canonical manifestini tekrar hızlı doğrula ve sonuç değişmediyse devam et.
2. Resmî Unity kaynağından kararlı Apple Silicon Hub kurulum dosyasını indir.
3. Apple code signature/Gatekeeper doğrulamasını yap; doğrulanmayan installer çalıştırma.
4. Hub'ı kur; kullanıcı Unity hesabına kendisi giriş yapar veya şartları okuyarak hesap oluşturur.
5. Unity 6000.3.21f1 ARM64 ile yalnız Windows Build Support (Mono) modülünü kur.
6. VS Code'a yalnız Microsoft Unity uzantısını ve otomatik gereken C#/C# Dev Kit/.NET bileşenlerini kur; telemetry düzeyini `off` yap.
7. Kullanıcının sahipliğinde private UVCS organizasyonu/repo oluştur; ödeme yöntemi ekleme.
8. `/Users/cixanla/Developer/PCShopEmpire3D/Game` altında boş URP proje oluştur.
9. Force Text, Visible Meta Files, ignore kuralları ve izin verilen resmî paketleri sabitle.
10. Boş proje baseline'ını check-in yap; repo temizliğini doğrula.
11. Editor aç/kapat, script derleme, Edit Mode boş test, Mac development player ve basit import sürelerini ölç.
12. Mac'ten Windows Mono boş build üret; gerçek Windows x64 testini Windows cihaz erişimi sağlandığında yap.
13. Ayrı USB yedek hedefini oluşturup ilk kaynak snapshot ve manifesti yaz; geri okuma/hash deneyi yap.
14. Gerçek indirilen/kurulu boyutları, sürümleri, hesap/kota durumunu ve tüm kontrolleri raporla.

Kullanıcının parolası, ödeme bilgisi, recovery code'u veya özel anahtarı belgeye/repo'ya yazılmaz.

## 11. Windows cihazında ileride gerekenler

Gerçek Windows x64 cihaz bulunduğunda aynı Unity proje sürümü kullanılacaktır:

- Windows 10/11 x64 üzerinde Unity 6000.3.21f1 Windows Editor.
- Windows Build Support (IL2CPP).
- Visual Studio 2019+ gereksinimini karşılayan güncel Visual Studio 2022 Build Tools/IDE; **Desktop development with C++** workload.
- Windows SDK 10.0.19041.0 veya daha yeni desteklenen sürüm.
- Steam client ve Steamworks araçları yalnız ilgili milestone'da.

Unity'nin platform kuralı nedeniyle final Windows IL2CPP build Mac'te yapılmaz. Mac'ten Mono build yalnız erken işlev testi içindir.

## 12. Maliyet kapıları

| Zaman | Zorunlu/olası maliyet | Bugünkü karar |
|---|---:|---|
| İlk kurulum ve teknik prototip | 0 | Ücretsiz katmanlar; ödeme yöntemi yok |
| UVCS 25 GB sınırına yaklaşma | Değişken | 23 GB'da dur ve ayrı onay iste |
| Steam'e ürün kaydı | 100 USD + geçerli vergi; uygulama başına | Vertical slice/pazar kararı öncesi ödeme yok |
| Steam gelir eşiği | Direct fee, en az 1.000 USD Adjusted Gross Revenue sonrası geri kazanılabilir | Gelir garantisi değildir |
| macOS ticari dağıtım | Apple Developer Program 99 USD/yıl, yerel para/vergi değişebilir | Windows 1.0 ve bütçeden sonra |
| Ücretli asset/IDE/ses | Değişken | Ölçülmüş darboğaz ve ayrı kullanıcı onayı olmadan 0 |

Unity Personal için güncel finansal eşik son 12 ayda 200.000 USD **Total Finances** değeridir. Eşik veya kullanım biçimi değişirse lisans yeniden değerlendirilir. Bu belge hukuki veya mali danışmanlık değildir.

## 13. Disk ve performans bütçesi

### İlk kurulum tahmini

- İndirme: yaklaşık 12–22 GB.
- Kurulu uygulamalar ve ilk cache: yaklaşık 25–40 GB.
- Güvenli geçici/çalışma rezervi: en az 80 GB.
- Mevcut iç SSD boş alanı: yaklaşık 799 GiB; yeterli.

### Milestone rezervi

- Teknik prototip: 50–80 GB.
- Vertical slice: proje + cache + build + kaynak assetlerle 120–200 GB.
- Her milestone'da `Library`, source assets, build, UVCS workspace ve USB snapshot ayrı ölçülür.

### Fansız MacBook için kurallar

- Büyük import/bake/build işleri küçük batch'lere ayrılır.
- Unity ve Blender aynı anda ağır render/import çalıştırmaz.
- MacBook düz, hava akışını engellemeyen yüzey/stand üzerinde kullanılır.
- Soğutucu stand yardımcı olabilir; cihazın içine müdahale edilmez.
- Apple Silicon'da Unity CPU lightmapping desteklenmediği için GPU lightmapping veya ileride Windows bake makinesi kullanılır.
- İlk benchmark sonuçları hedefleri karşılamazsa kalite ayarı veya iş bölümü değiştirilir; donanım hakkında varsayım yapılmaz.

## 14. Gizlilik, lisans ve AI sınırları

- Unity AI Agent/Muse, Copilot veya başka AI uzantısı bu kurulum paketine dahil değildir.
- Oyuna dış model, API anahtarı veya üretken AI runtime eklenmez.
- Guardian bu aşamada yalnız tasarım kararıdır; boş projeye henüz kodlanmaz.
- VS Code telemetry ayarı kapatılır; Unity/UVCS için zorunlu hizmet verileri ilgili güncel privacy metinlerine tabidir.
- UVCS repo private olur; gizli anahtar, parola, token ve kişisel belge repo içine girmez.
- Blender ile üretilen sanat çıktısı Blender'ın GPL lisansı yüzünden otomatik GPL olmaz; kullanılan asset/add-on lisansları ayrıca izlenir.
- Her indirilen asset/package için kaynak, sürüm, lisans, tarih ve hash provenans defterine girer.

## 15. Geri alma planı

Kurulum başarısız olursa:

- Eski oyun ve USB kaynak değişmediği için etkilenmez.
- Hub üzerinden yalnız kurulan 6000.3.21f1 Editor/modül kaldırılabilir.
- VS Code uzantıları ayrı kaldırılabilir; mevcut VS Code korunur.
- Yeni Unity proje klasörü ve UVCS repo kendiliğinden silinmez; silme için ayrıca açık kullanıcı onayı gerekir.
- Unity patch değiştirmek gerekirse mevcut proje önce doğrulanmış USB snapshot ile, ilk check-in sonrasında ayrıca cloud geçmişiyle korunur ve ayrı test workspace'inde açılır.
- Ücretsiz cloud repo silinmez; yalnız arşivlenir veya kullanıcı kararı beklenir.

## 16. Kurulumun kabul ölçütleri

İlk teknik kurulum ancak aşağıdakilerin tümü sağlanırsa tamamlanmış sayılır:

- Unity Hub ve Editor resmi imza doğrulamasından geçer.
- Editor sürümü tam olarak `6000.3.21f1`, mimari ARM64'tür.
- Yalnız onaylı modüller kurulmuştur.
- Yeni proje legacy klasörlerin ve iCloud Documents'ın dışındadır.
- Unity project serialization ve meta ayarları sürüm kontrolüne uygundur.
- UVCS private repo kullanıcının hesabındadır; ödeme yöntemi/ücretli plan yoktur.
- Boş URP proje üç temiz açılışta derleme hatası vermez.
- Resmî paket listesi ve lock dosyası kayıtlıdır.
- İlk Mac player ve Windows Mono build üretilebilir.
- USB legacy kaynağının 26/26 manifesti değişmemiştir.
- İlk yeni-proje snapshot'ı geri okunmuş ve hash ile doğrulanmıştır.
- Gerçek disk kullanımı ve kurulum raporu yaşayan Proje Hafızasına işlenmiştir.

## 17. Birincil kaynaklar

- [Unity 6000.3.21f1 sürüm ve modül sayfası](https://unity.com/releases/editor/whats-new/6000.3.21f1)
- [Unity 6 sürüm desteği — 6.3 LTS Aralık 2027'ye kadar](https://unity.com/releases/unity-6/support)
- [Unity 6.3 sistem gereksinimleri](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html)
- [Unity Hub macOS/Windows kurulumu](https://docs.unity.com/hub/install-hub-win-mac)
- [Unity Editor Software Terms — Personal 200.000 USD Total Finances](https://unity.com/legal/editor-terms-of-service/software)
- [Unity 2026 DevOps fiyat/kota değişiklikleri](https://unity.com/products/pricing-updates)
- [Unity IL2CPP platforma özgü derleme açıklaması](https://docs.unity3d.com/6000.3/Documentation/Manual/il2cpp-introduction.html)
- [VS Code ile Unity geliştirme](https://code.visualstudio.com/docs/other/unity)
- [C# Dev Kit lisans SSS](https://code.visualstudio.com/docs/csharp/cs-dev-kit-faq)
- [GitHub Free Git LFS kotası](https://docs.github.com/en/billing/concepts/product-billing/git-lfs?apiVersion=2022-11-28)
- [Git LFS 3.7.1 ve güvenlik güncellemesi](https://git-lfs.com/)
- [Blender resmî indirme sayfası](https://www.blender.org/download/)
- [Blender'ın resmî video kanalı — 5.2 Release Candidate durumu](https://video.blender.org/c/blender_channel/videos)
- [Blender lisans açıklaması](https://docs.blender.org/manual/en/3.2/getting_started/about/license.html)
- [Steam Direct ücreti](https://partner.steamgames.com/doc/gettingstarted/appfee)
- [Steamworks SDK](https://partner.steamgames.com/doc/sdk?language=english)
- [Apple Developer Program yıllık ücret ve üyelik](https://developer.apple.com/help/account/membership/program-enrollment/)
- [Apple macOS notarization](https://developer.apple.com/documentation/security/notarizing_macos_software_before_distribution)

## 18. Tarihsel kesin onay kapsamı — 11 Ağustos 2026'da onaylandı

Verilmiş olan Stage A onayı aşağıdaki işlemlere izin verdi:

- Resmî kaynaklardan Stage A indirmeleri ve kurulumları.
- `/Users/cixanla/Developer/PCShopEmpire3D` altında yeni boş URP proje/workspace oluşturulması.
- Kullanıcının kendi hesabında ücretsiz/private UVCS repo kurulması; ödeme yöntemi eklenmemesi.
- USB'de yalnız yeni `CIXANLA/90_BACKUPS/PCShopEmpire3D` hedefinin oluşturulması ve ilk doğrulanmış snapshot'ın yazılması.
- Kurulum/boş proje smoke testleri ve Windows Mono boş build.

Bu onay; gameplay kodlaması, asset indirme/üretme, Steam ödemesi, Apple üyeliği, ücretli plan, Blender kurulumu, eski kaynakta değişiklik veya gerçek Windows IL2CPP release build anlamına gelmez.

## 19. Uygulama sonucu

Kullanıcı bu bölümde tanımlanan Stage A kapsamını 11 Ağustos 2026'da onayladı. Uygulama sonucu:

- Unity Hub 3.20.1, Unity 6000.3.21f1 ARM64 ve Windows Build Support (Mono) resmî kanaldan kuruldu.
- Mevcut VS Code'a Microsoft Unity/C# geliştirme uzantıları kuruldu.
- `/Users/cixanla/Developer/PCShopEmpire3D/Game` altında legacy'den bağımsız URP proje oluşturuldu.
- Force Text, Visible Meta Files, paket kilidi, ignore ve provenans temeli uygulandı.
- Edit Mode testleri 4/4 geçti.
- macOS Universal development player üretildi ve headless smoke testi geçti.
- Windows x64 Mono development player üretildi; native Windows çalıştırma/IL2CPP doğrulaması henüz yapılmadı.
- Unity Cloud projesi ve ücretsiz/private UVCS repo oluşturuldu; kart veya ücretli plan eklenmedi.
- UVCS token exchange geçti; cloud dosya-protokolü uzak uç tarafından sıfırlandığı için ilk check-in tamamlanmadı ve `.plastic` workspace oluşmadı.
- Canonical legacy kaynak tekrar 26/26 SHA-256 eşleşti.
- Yeni proje için ayrı USB Stage A kaynak snapshot'ı ve readback manifest doğrulaması uygulandı.

Kurulum öncesi tahminler ile gerçek sürüm, boyut, test, build ve bağlantı sonuçlarının authoritative kaydı `09_STAGE_A_KURULUM_RAPORU.md` dosyasıdır.
