# PC Shop Empire 3D – Kaynak Kayıt Defteri

**Araştırma kesiti:** 10–11 Ağustos 2026  
**Belge sürümü:** 0.2 — 0.1 araştırma kesiti + Stage A teknik kaynak güncellemesi  
**Amaç:** Tasarım ve teknik kararların hangi kanıta dayandığını, kaynağın türünü ve sınırlarını kaybetmemek.

## 1. Kaynak kullanma ilkesi

Araştırma, kaynağın barındığı siteye göre değil iddia sahibine ve kanıt işlevine göre dört sınıfa ayrıldı:

1. **Birincil/resmî olgu:** Motor/platform/kurum belgesi, resmî mağaza veya geliştirici sayfası ve geliştiricinin kendi yama notu.
2. **Resmî gösterim/gözlem:** Geliştiricinin trailer'ı veya oynanış sunumu; gösterilen özelliği destekler, oyuncu memnuniyetini kanıtlamaz.
3. **Oyuncu raporu:** Steam yorumu veya doğrudan topluluk başlığı; yeniden üretilmedikçe kesin ürün kusuru değildir.
4. **İkincil analiz:** Basın incelemesi, röportaj, Reddit tartışması ve oyuncu rehberi.

SteamDB, resmî yama metinlerini koruyabilen üçüncü taraf bir **yama notu aynasıdır**; resmî Valve/geliştirici kaynağı sayılmadı. Bir oyuncu yorumu tek başına genel gerçek sayılmadı. Bu 0.1 araştırmasında bütün yorumlar kodlanmış istatistiksel bir örnekleme dönüştürülmedi. Resmî yama veya en az üç bağımsız doğrudan raporla desteklenmeyen bulgu “yaygın/kronik sorun” değil, **tekil ya da nitel risk sinyali** olarak kaydedildi. İnceleme puanları ve sayıları yalnız **10 Ağustos 2026 tarihli oynak anlık görüntülerdir**.

### İnceleme anlık görüntüsü

| Oyun | 10 Ağustos 2026 Steam alıcısı incelemesi | Olumlu | Durum notu |
|---|---:|---:|---|
| PC Building Empire | 164 | %84 | Early Access |
| PC Store Simulator | 40 | %35 | Çıkış: 30 Mart 2026 |
| Electronics Store Simulator | 75 | %66 | Çıkış: 6 Kasım 2025 |
| Computer Store Simulator | 0 | — | 2026 planlı; çıkmamış |

Tablodaki sayılar yalnız Steam satın alıcısı kapsamıdır; Steam dışı anahtar/diğer aktivasyon kaynağı incelemeleri aynı paydaya karıştırılmadı. Bu ilk kesitte tam saat, seçili mağaza dili ve her tema için kaç yorum kodlandığı ayrı dataset olarak arşivlenmedi; bu bir yöntem sınırlamasıdır. Bu nedenle tablo pazar büyüklüğü veya temaların yaygınlığı için istatistiksel örneklem sayılmaz. Bir sonraki rakip güncellemesinde oyun, tarih-saat ve saat dilimi, dil/filtre, inceleme kapsamı, incelenen olumlu/olumsuz yorum sayısı, doğrudan URL ve tema sayısı satır bazında kaydedilecektir.

Hiçbir rakibin kodu, dosyası, marka varlığı, modeli, arayüzü, sesi veya metni alınmadı. Mağaza sayfalarında gömülü resmî trailer/oynanış sunumları yalnız mekanik ve sunum analizi için izlendi; tasarım ilkeleri özgün sisteme çevrildi.

## 2. Yerel eski proje kaynakları

### Ana yerel inceleme kaynağı

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/transfer_review/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

Bu kopya owner-writable'dır; “salt-okunur” ifadesi dosya izni değil, inceleme sırasında hiçbir değişiklik yapılmayan yöntemdir.

İncelenen ana dosyalar:

| Dosya | Kullanım |
|---|---|
| `package.json` | Electron/Forge bağımlılıkları ve proje script'leri |
| `forge.config.js` | Paketleme, güvenlik fuse'ları, Windows/macOS hedefleri |
| `main.js` | Electron pencere, CSP, IPC ve navigasyon sınırları |
| `preload.js` | Sınırlı masaüstü köprüsü |
| `index.html` | 14 Dashboard sekmesi, başlangıç, üst bar, ayarlar ve modal yapısı |
| `styles.css` | Ana UI ve temsili mağaza stilleri |
| `game.js` | 17.639 satırlık monolitik oyun durumu ve davranışları |
| `src/release-data.js` | Pazar, servis, kariyer ve içerik tanımları |
| `src/release-systems.js` | Servis, marka/pazar, iş zekâsı, kariyer ve analitik |
| `src/release-bootstrap.js` | Sonradan bağlanan sayfa/fonksiyon wrapper'ları |
| `src/release-settings.js` | Ayar, dil, erişilebilirlik ve veri dışa/içe aktarma |
| `tests/smoke-test.js` | Temel fonksiyon smoke testi |
| `tests/simulation-test.js` | 45 günlük sayısal simülasyon testi |
| `README.md`, `CHANGELOG.md` | Sürüm ve çalışma niyeti |

### Canonical USB legacy kaynağı

`/Volumes/cixanla/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

11 Ağustos 2026'da yalnız okuma ile çıkarılan envanter 26 dosyadır. Bu 26 dosyanın tamamı ana yerel inceleme kaynağıyla göreli yol, boyut ve SHA-256 düzeyinde eşleşmiştir; eksik, fazla veya farklı dosya yoktur. USB kaynağı canonical legacy snapshot, yerel kaynak onun doğrulanmış aynası olarak kaydedildi.

### Karşılaştırılan macOS çalışma kopyası

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/builds/pc-shop-empire-mac`

Ana `game.js`, `index.html`, `main.js`, `preload.js`, `src/*`, test ve görsel dosyaları canonical kaynakla SHA-256 düzeyinde eşleşti. `THIRD_PARTY_NOTICES.txt`, `forge.config.js`, `package-lock.json`, `package.json` ve `styles.css`; macOS paketleme, bağımlılık/lisans envanteri ve font çalışmaları nedeniyle ayrıştı.

### Git deposu

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/git/PC-Shop-Empire`

Bu depo gerçek kaynak kodu içermiyor; yalnız README/release yönlendirmesi var. Bu nedenle canonical kaynak veya geri dönüş sistemi olarak kabul edilmedi.

### Canlı uygulama verisi

`/Users/cixanla/Library/Application Support/PC Shop Empire`

Salt-okunur varlık tespiti yapıldı. Kullanıcı kayıtları değiştirilmedi.

### USB doğrulama durumu

USB ilk inceleme kesitinde bağlı değildi; 11 Ağustos 2026'da yeniden bağlandı. Dosya manifesti ve hash karşılaştırması hiçbir dosya değiştirilmeden tamamlandı ve 26/26 eşleşme elde edildi.

## 3. Rakip oyun kaynakları

### PC Building Empire

- **[Resmî mağaza]** [Steam mağaza sayfası](https://store.steampowered.com/app/3588630/PC_Building_Empire/): Early Access kapsamı/tahmini, geliştirici açıklaması, özellikler, ekran/trailer ve inceleme kesiti.
- **[Oyuncu raporu platformu]** [Steam topluluk merkezi](https://steamcommunity.com/app/3588630): tekil/nitel oyuncu sorun ve geri bildirim sinyalleri.
- **[Yama notu aynası]** [2 Eylül güncellemesi](https://steamdb.info/patchnotes/19828286/): PC toplama mini oyunu ve tutorial'ın eklenmesi; ayrı bir madde olarak Computer Expedition süresinin 375'ten 150 saniyeye indirilmesi.
- **[Yama notu aynası]** [Offline ilerleme güncellemesi](https://steamdb.info/patchnotes/20107028/): offline ilerlemenin eklenmesi.
- **[Yama notu aynası]** [Offline hesap düzeltmeleri](https://steamdb.info/patchnotes/20133533/): ertesi düzeltme örneği.
- **[Yama notu aynası]** [17 Ağustos stabilite yaması](https://steamdb.info/patchnotes/19630604/): uzun save ve çalışan davranışı düzeltmeleri.
- **[Yama notu aynası]** [Sezon sistemi yaması](https://steamdb.info/patchnotes/20482991/): ana endless kayıttan ayrı, 30 günlük Season Mode sıralama/reset yapısı.
- **[İkincil geliştirici röportajı]** [GameSpark](https://www.gamespark.jp/article/2025/08/17/156091.html): şirket içi playable gözleminde kişiselleştirmeye beklenenden yüksek ilgi aktarımı; bağımsız pazar ölçümü değil.

Kullanım: izometrik/idle üretim döngüsü, oda/departman büyümesi, çalışan farklılıkları, grind ve reset riskleri.

### PC Building Simulator 1

- **[Resmî mağaza]** [Steam mağaza sayfası](https://store.steampowered.com/app/621060/PC_Building_Simulator/): özellikler, kariyer/free build ve trailer'lar.
- **[Oyuncu yorumu]** [En yararlı Steam yorumları](https://steamcommunity.com/app/621060/reviews/?browsefilter=toprated): öğreticilik ve gerçek PC kurma cesareti sinyalleri.
- **[Oyuncu yorumu]** [En yararlı negatif yorumlar](https://steamcommunity.com/app/621060/negativereviews/?browsefilter=toprated&l=english): tekrar ve envanter UX sinyalleri.
- **[Oyuncu yorumu]** [Yakın dönem yorumları](https://steamcommunity.com/app/621060/reviews/?browsefilter=trendyear&filterLanguage=default&l=english&p=1): katalog eskimesi ve destek algısı.
- **[Resmî geliştirici destek yazısı]** [Common Issues and Solutions](https://steamcommunity.com/app/621060/discussions/1/3114773449973523049/): çözülemez fiziksel kombinasyon üreten iş örneği.
- **[Oyuncu raporu]** [Tanı/kabul ölçütü tartışması](https://steamcommunity.com/app/621060/discussions/0/2959417653802400976/): doğru görünen çözümün neden kabul edilmediği.
- **[Oyuncu raporu; eski/sınırlı örnek]** [Save/cloud sorun örneği](https://steamcommunity.com/app/621060/discussions/0/3124867196129726342/): kayıt güveni riski.

Kullanım: fiziksel PC eğitimi, sipariş–iş–benchmark döngüsü, uyumluluk, envanter ve tekrar sorunları.

### PC Building Simulator 2

- **[Resmî geliştirici]** [Site](https://www.pcbuildingsim.com/) ve [çıkış özellikleri](https://www.pcbuildingsim.com/news/pc-building-simulator-2-is-out-now): 1.200+ parça, 40+ marka, kariyer, mağaza ve ileri araçlar.
- **[Resmî video]** [Çıkış trailer'ı](https://www.youtube.com/watch?v=CFkBvEVO9hk): montaj, özelleştirme ve benchmark sunumu.
- **[Resmî mağaza]** [Epic Games Store](https://store.epicgames.com/p/pc-building-simulator-2): özellik listesi.
- **[Resmî yama]** [v1.8 kalite geliştirmeleri](https://www.pcbuildingsim.com/news/pc-building-simulator-2-update-v1-8): termal pad, kablo ve su bloğu vidası gibi tekrarlanan sökme eylemlerinde otomatik QoL; açılma zamanlamasını kanıtlamaz.
- **[Resmî yama]** [v1.15](https://www.pcbuildingsim.com/news/pc-building-simulator-2-update-115-out-now-on-pc-xbox-ps5): tutorial/onboarding düzeltmeleri.
- **[Resmî yama]** [v1.16](https://www.pcbuildingsim.com/news/pc-building-simulator-2-update-116-out-now-on-pc-xbox-ps5): rigid pipe, sonsuz yükleme ve iş üretme sorunları.
- **[Resmî yama]** [v1.17](https://www.pcbuildingsim.com/news/pc-building-simulator-2-update-117-out-now-on-pc-xbox-ps5): onboarding bloklayıcıları, save/load ve özel fan satışı ekonomi açığı.
- **[Resmî yama]** [v1.00.14](https://www.pcbuildingsim.com/news/pc-building-simulator-2-update-v1-00-14): erken tutorial ve save/load düzeltmeleri.
- **[İkincil inceleme]** [NME](https://www.nme.com/reviews/game-reviews/pc-building-simulator-2-review-3327300): özellik ve tekrar değerlendirmesi.

Kullanım: ileri PC ölçümü/özelleştirme, iş rezervasyonu, fiziksel kombinasyon regresyonu ve tutorial softlock riski.

### Doğrudan PC/teknoloji mağazası rakipleri

- **[Resmî mağaza]** [PC Store Simulator – Steam](https://store.steampowered.com/app/3451560/PC_Store_Simulator/): birinci şahıs mağaza + PC toplama/onarım vaadi; 10 Ağustos 2026 inceleme kesiti.
- **[Değişken keşif sayfası]** [PC Store Simulator – topluluk](https://steamcommunity.com/app/3451560/): belirli bir iddianın kalıcı kanıtı olarak kullanılmadı.
- **[Çıkış sonrası oyuncu raporları]** [Tutorial sıra/softlock](https://steamcommunity.com/app/3451560/discussions/0/802343553907879267/), [performans değerinin güncellenmemesi](https://steamcommunity.com/app/3451560/discussions/0/802344041053331799/), [kayıt/depo/fiyat etiketi](https://steamcommunity.com/app/3451560/discussions/0/802343728582889735/), [çalışan sistemi anlaşılabilirliği](https://steamcommunity.com/app/3451560/discussions/0/802344041053182668/): yeniden üretim kanıtı değil.
- **[Çıkış öncesi demo oyuncu raporları]** [UI/ayar erişimi](https://steamcommunity.com/app/3451560/discussions/0/591783706468037199/), [21:9](https://steamcommunity.com/app/3451560/discussions/0/756178376553788862/), [montaj etkileşimi](https://steamcommunity.com/app/3451560/discussions/0/756178031216815466/), [mağaza–atölye/itibar](https://steamcommunity.com/app/3451560/discussions/0/765184781366570251/): tam çıkış sürümünde devam ettiği varsayılmadı.
- **[Resmî mağaza]** [Tech Store Simulator – Steam](https://store.steampowered.com/app/3076400/Tech_Store_Simulator/): sipariş, raf, fiyat, kasa, çalışan ve hırsızlık döngüsü.
- **[Değişken keşif sayfası]** [Tech Store Simulator – topluluk](https://steamcommunity.com/app/3076400): tekil başlıkları bulmak için kullanıldı.
- **[Oyuncu raporları]** [0 fiyatlı satış](https://steamcommunity.com/app/3076400/discussions/0/7404569763078569234/), [çok sık hırsız 1](https://steamcommunity.com/app/3076400/discussions/0/4849903351688233821/), [çok sık hırsız 2](https://steamcommunity.com/app/3076400/discussions/0/4692280570914363656/), [depo çalışanı AI](https://steamcommunity.com/app/3076400/discussions/0/4633736089165664474/), [takılan stokçu](https://steamcommunity.com/app/3076400/discussions/0/4692280867918216597/), [elle karşılama yükü](https://steamcommunity.com/app/3076400/discussions/0/4847652197980788139/): yeniden üretilmiş hata kanıtı değildir.
- **[Platform verisi]** [Tech Store Simulator – başarımlar](https://steamcommunity.com/stats/3076400/achievements): hacim odaklı ilerleme örnekleri.
- **[Resmî mağaza]** [Electronics Store Simulator – Steam](https://store.steampowered.com/app/3988670/Electronics_Store_Simulator/): teslimat, trend, büyüme ve franchise.
- **[Değişken keşif sayfası]** [Electronics Store Simulator – topluluk](https://steamcommunity.com/app/3988670): tekil başlıkları bulmak için kullanıldı.
- **[Çıkış sonrası oyuncu raporları]** [çalışan talebi](https://steamcommunity.com/app/3988670/discussions/0/660467372238310223/), [başarım sorunu](https://steamcommunity.com/app/3988670/discussions/0/658215953538367011/), [güncelleme sürekliliği kaygısı](https://steamcommunity.com/app/3988670/discussions/0/734784298132146813/): nitel sinyal; bağımsız yeniden üretim değil.
- **[Playtest oyuncu raporları]** [Girdi, NPC ve kutu fiziği hata başlığı](https://steamcommunity.com/app/3988670/discussions/0/592911760736044597/): tam sürüm kronik kusuru sayılmadı.
- **[Resmî mağaza; çıkmamış vaat]** [Computer Store Simulator – Steam](https://store.steampowered.com/app/3520620/Computer_Store_Simulator/): henüz doğrulanmamış raf/onarım/çalışan/LAN parti vaatleri.

Kullanım: Hedeflenen iki sütunun piyasada var olduğu, fakat save, tutorial, çalışan, açıklanabilirlik ve kalite açığının farklılaşma fırsatı yarattığı sonucunu destekledi.

## 4. Mağaza, tycoon ve simülatör referansları

### Supermarket Simulator

- [Steam topluluk merkezi](https://steamcommunity.com/app/2670630): satış, çalışan, save ve pathfinding geri bildirimleri.
- [Resmî 1.0 trailer](https://www.youtube.com/watch?v=fr5OBDfoXuc): birinci şahıs raf, kasa, fiyat, çalışan ve güvenlik döngüsü.
- [Resmî site](https://supermarketsimulator.com/): oyun/trailer ve güncel özellik sunumu.
- [Pathfinding yama notu](https://steamdb.info/patchnotes/16848960/): müşteri/çalışan rota düzeltmesi.
- [Yakın dönem Steam yorumları](https://steamcommunity.com/app/2670630/reviews/?filterLanguage=english): save ve uzun dönem operasyon örüntüsü.

### TCG Card Shop Simulator

- [Steam sayfası](https://store.steampowered.com/app/3070070/TCG/): raf/kasa yanında koleksiyon ve sosyal etkinlik döngüsü; 10 Ağustos 2026'da Early Access.
- [Steam topluluğu](https://steamcommunity.com/app/3070070): çalışan, performans ve save konularındaki nitel sinyallerin keşfi; bu turda tema sıklığı kodlanmadığı için “yaygın/kronik örüntü” kanıtı değildir.

### Big Ambitions

- Oyun 10 Ağustos 2026'da hâlâ Early Access'tir; aşağıdaki yorumlar gelişmekte olan build'e aittir.
- [En yararlı yorumlar](https://steamcommunity.com/app/1331550/reviews/?browsefilter=toprated): fiziksel başlangıçtan HQ/lojistiğe büyüme.
- [Negatif yorumlar](https://steamcommunity.com/app/1331550/negativereviews/?browsefilter=toprated&l=english): geç oyun mikro yönetim ve avatar amacı.

### King of Retail

- [İflas/campaign tartışması](https://steamcommunity.com/app/968250/discussions/0/3768984714327147208/): büyüme yatırımı sonrası işletme sermayesi ve save kaybı kaygısı; aynı başlık kredi/yönetici yoluyla kurtarma imkânını da anıyor.

### Gas Station Simulator

- [Restock çalışanı tartışması](https://steamcommunity.com/app/1149620/discussions/0/565869956174973059/): görev ve erişim bloklanması.
- [Yama örneği](https://steamdb.info/patchnotes/17107675/): auto-restock/save-load engeli düzeltmeleri.

### Car Mechanic Simulator 2021

- [Steam sayfası](https://store.steampowered.com/app/1190000/Car_Mechanic_Simulator_2021/): fiziksel sökme-takma ve iş döngüsü.
- [Steam yorumları](https://steamcommunity.com/app/1190000/reviews/?browsefilter=toprated): dokunsallık, tekrar ve teşhis beklentisi.

### Software Inc.

- [Steam yorumları](https://steamcommunity.com/app/362620/reviews/?browsefilter=toprated): takım, bina, proje ve delegasyon derinliği.

Kullanım: görünür büyüme, “bir gün daha” ritmi, otomasyon, görev/pathfinding, fiziksel tekrar, yatırım riski ve geç oyun mikro yönetimi.

## 5. Servis ve onarım referansları

### ReStory: Chill Electronics Repairs

- [Steam sayfası](https://store.steampowered.com/app/3812600/ReStory_Chill_Electronic_Repairs/): sökme, temizleme, onarım, müşteri hikâyesi ve mağaza yönetimi.
- [Steam topluluğu](https://steamcommunity.com/app/3812600): çok yeni çıkıştaki oyuncu geri bildirimi; uzun dönem kanıt sayılmadı.
- [PC Gamer ön izlemesi](https://www.pcgamer.com/games/sim/repair-shop-simulator-restory-will-finally-let-me-live-out-my-dream-of-being-an-electronic-tinkerer-without-having-to-destroy-my-own-belongings-first/): tinkering fantezisi, cihaz çeşitliliği ve hikâye bağlamı.
- [GameSpark gameplay trailer haberi](https://www.gamespark.jp/article/2026/05/25/166891.html): yayımlanan oynanış trailer'ının kapsamı.
- [Marketplace geri bildirimi](https://steamcommunity.com/app/3812600/discussions/0/567037624436399525/) ve [lehim/ileri teşhis talebi](https://steamcommunity.com/app/3812600/discussions/0/750542551166045457/): **demo/playtest dönemi oyuncu sinyalleri**; tam sürüm kronik sorunu sayılmadı.

Not: Oyun 6 Ağustos 2026'da çıktığı için ilk yüksek olumlu tepki “balayı etkisi” taşıyabilir. Uzun süreli tekrar, save ve destek kalitesi daha sonra yeniden araştırılmalıdır.

### Computer Repair Shop

- [Steam sayfası](https://store.steampowered.com/app/2479290/Computer_Repair_Shop/): onarım, mağaza ve yan etkinlik kapsamı.
- [Belirsiz iş sonucu tartışması](https://steamcommunity.com/app/2479290/discussions/0/7091547412713782475/): açıklanabilir kabul ölçütü riski.

Kullanım: dokunsal tamirin güçlü olduğu, fakat karar derinliği ve insan bağının tekrarın önüne geçmesi gerektiği; kopuk mini oyun yığınının odağı sulandırdığı sonucu.

### It Works: Electronics Repair Simulator

- [Steam sayfası](https://store.steampowered.com/app/3787050/It_Works_Electronics_Repair_Simulator/): 2026 için planlanan multimetre, lehim, bileşen düzeyi teşhis ve atölye büyütme vaatleri.

Henüz kullanıcı incelemesi veya bitmiş sürüm kanıtı yoktur. Yalnız ileri servis katmanı için izlenecek doğrulanmamış mağaza vaadi olarak kaydedildi.

## 6. Pazar doygunluğu ve ikincil tasarım analizi

- [PC Gamer: Steam'deki perakende simülatörü seli](https://www.pcgamer.com/gaming-industry/steam-week-in-review-a-torrent-of-janky-retail-sims-continues-to-flood-steam-and-theres-no-end-in-sight/): benzer temalı, düşük cilalı perakende simülatörlerinin doygunluğu.
- [GameSpark – PC Building Empire röportajı](https://www.gamespark.jp/article/2025/08/17/156091.html): oyuncuların kişiselleştirmeye ilgisi.
- [NME – PC Building Simulator 2](https://www.nme.com/reviews/game-reviews/pc-building-simulator-2-review-3327300): kariyer/özellik değerlendirmesi.

İkincil kaynaklar mekaniklerin varlığı için tek kanıt yapılmadı; resmî sayfa, görüntü veya topluluk davranışıyla çaprazlandı.

## 7. Gerçek PC uyumluluğu, montaj ve teşhis kaynakları

- [AMD boxed processor warranty/troubleshooting guide](https://www.amd.com/en/resources/support-articles/warranty/PIB.html): işlemci–anakart soketi, bellek türü, BIOS, CPU Support List, QVL, PSU yeterliliği/kabloları, known-good swap ve fiziksel montaj hasarı ilişkisi.
- [AMD common boot failures](https://www.amd.com/en/resources/support-articles/faqs/PIBRMATS1.html): no power/no display/no boot ayrımı; POST, güç konektörleri, front-panel, BIOS, DIMM yerleşimi, soğutucu ve kontrollü parça değişimi.
- [AMD CPU performance and temperature troubleshooting](https://www.amd.com/en/resources/support-articles/faqs/PIBRMATS3.html): stok ayarla başlangıç, sıcaklık, QVL bellek, firmware ve soğutma etkileri.
- [Intel desktop processor compatibility](https://www.intel.com/content/www/us/en/support/articles/000092149/processors.html): yalnız fiziksel soketin değil işlemci nesli ve chipset eşleşmesinin de belirleyici olması.
- [Intel desktop processor package guide](https://www.intel.com/content/www/us/en/support/articles/000005670/processors.html): paket/soket, yönlendirme ve BIOS desteği ilişkisi.
- [Intel ATX12VO desktop power supply design guide](https://www.intel.com/content/www/us/en/content-details/613768/atx12vo-12v-only-desktop-power-supply-design-guide.html): PSU'nun elektriksel ve mekanik form faktörü; oyunda güç, konektör ve fiziksel sığma alanlarının ayrılması için birincil teknik referans.
- [NVM Express specifications](https://nvmexpress.org/specifications/): NVMe'nin PCIe dahil farklı taşıma ve SSD form faktörü ilişkileri.
- [AMD socket/chipset example](https://www.amd.com/en/products/processors/chipsets/am5.html): aynı platform ailesinde bellek, PCIe ve chipset özelliklerinin birlikte değerlendirilmesi.

Oyundaki karşılığı: kural motoru soket, chipset/firmware, bellek türü/QVL benzeri doğrulanmış profil, form faktörü, fiziksel açıklık, PSU kapasitesi/konektör, depolama arayüzü, termal kapasite ve sürücüyü ayrı nedenler olarak işler. Servis; belirti → POST/güç/görüntü ayrımı → görsel inceleme → ölçüm/log → known-good swap → doğrulama zincirini kullanır.

Gerçek marka ve güncel nesil adları oyun içeriğine taşınmaz. Bu kaynaklar yalnız gerçek teknik bağımlılıkların hangi veri boyutlarına ayrılması gerektiğini doğrular; kurgusal katalog kendi açık ve test edilebilir kurallarını kullanır.

## 8. Gerçek perakende, garanti, servis ve veri işleyişi

### Stok ve izlenebilirlik

- [GS1 Global Traceability Standard](https://www.gs1.org/standards/gs1-global-traceability-standard/current-standard): ürün/lojistik birimi kimliği, olay ve izlenebilirlik ilkeleri.

Oyundaki karşılığı: seri/batch, tedarikçi, teslimat, konum, rezervasyon, satış ve iade zinciri. Gerçek standardı birebir uygulamak yerine izlenebilirlik ilkesinden yararlanılır.

### Tüketici garantisi ve onarım

- [AB tüketici garantileri](https://europa.eu/youreurope/business/selling-in-eu/consumer-contracts-guarantees/consumer-guarantees/index_en.htm): yasal garanti ve satıcı sorumluluğu çerçevesi.
- [FTC Warranties](https://consumer.ftc.gov/articles/warranties): garanti türleri ve tüketici bilgilendirmesi.
- [Avrupa Komisyonu Right to Repair](https://commission.europa.eu/law/law-topic/consumer-protection-law/directive-repair-goods_en): onarımın teşviki ve güncel AB çerçevesi.

Oyunun şehri kurgusaldır; bu kurallar doğrudan hukuk simülasyonu olarak kopyalanmaz. Şeffaf teklif, garanti kaydı, onarım seçeneği ve doğru bilgilendirme ilkeleri alınır.

### Veri silme ve mahremiyet

- [NIST SP 800-88 Rev. 2 – Guidelines for Media Sanitization](https://csrc.nist.gov/News/2025/guidelines-for-media-sanitization-rev-2): medya sanitization ve veri imha yaklaşımı.
- [Avrupa Komisyonu GDPR veri işleme ilkeleri](https://commission.europa.eu/law/law-topic/data-protection/rules-business-and-organisations/principles-gdpr/overview-principles/what-data-can-we-process-and-under-which-conditions_en): amaç sınırlaması, veri minimizasyonu ve hukuki temel.
- [Avrupa Komisyonu GDPR uygulama kapsamı](https://commission.europa.eu/law/law-topic/data-protection/information-business-and-organisations/application-gdpr_en): IP adresi ve pseudonymous verinin kişisel veri kapsamına girebilmesi.

Oyundaki karşılığı: servis intake izni, okunabilir kişisel içerik göstermeme, erişim kaydı, yedekleme tercihi ve ikinci elde sanitize zinciri. Guardian telemetry'si ayrı opt-in ve allowlist kullanır.

## 9. Unity ve teknik platform kaynakları

### Motor ve destek

- [Unity 6.3 LTS duyurusu](https://unity.com/blog/unity-6-3-lts-is-now-available): sürüm ve LTS zamanlaması.
- [Unity 6000.3.21f1 sürüm/modül sayfası](https://unity.com/releases/editor/whats-new/6000.3.21f1): 29 Temmuz 2026 yaması ve macOS ARM64/Windows Mono modül seçimi.
- [Unity 6 destek politikası](https://unity.com/releases/unity-6/support): destek süresi ve sürüm çizgisi.
- [Unity 6.3 sistem gereksinimleri](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html): editör ve player platform gereksinimleri; Apple Silicon ışık bake sınırları dahil.
- [Unity Hub macOS/Windows kurulumu](https://docs.unity.com/hub/install-hub-win-mac): resmî Hub indirme ve kurulum akışı.
- [Unity fiyatlandırma güncellemesi](https://unity.com/products/pricing-updates): Personal gelir/fonlama sınırı ve plan değişiklikleri.
- [Unity Editor Software Terms](https://unity.com/legal/editor-terms-of-service/software): Editor lisansı ve `Total Finances` tanımı için resmî sözleşme.

### Kod, input, test ve içerik

- [Unity Input System](https://docs.unity3d.com/Manual/com.unity.inputsystem.html): action map, çoklu cihaz ve yeniden atama tabanı.
- [Unity Test Framework](https://docs.unity3d.com/Manual/testing-editortestsrunner.html): Edit/Play Mode testleri.
- [Unity Addressables](https://docs.unity3d.com/Packages/com.unity.addressables%401.20/index.html): adreslenebilir içerik yaklaşımı; kurulacak paket sürümü proje sürümüne göre ayrıca doğrulanacak.
- [Unity 6.3 IL2CPP açıklaması](https://docs.unity3d.com/6000.3/Documentation/Manual/il2cpp-introduction.html): native backend ve hedef platform kısıtları.
- [VS Code ile Unity geliştirme](https://code.visualstudio.com/docs/other/unity): resmî Microsoft Unity/C# uzantı akışı ve editör entegrasyonu.
- [C# Dev Kit lisans SSS](https://code.visualstudio.com/docs/csharp/cs-dev-kit-faq): bireysel ve küçük ekip ücretsiz kullanım koşulları.
- [Desktop shader/AssetBundle notu](https://support.unity.com/hc/en-us/articles/207482023-Shaders-in-AssetBundles-for-Desktop-platforms-Win-Mac-): Windows/macOS shader varlıklarının platforma özgü olabileceği.
- [Unity Localization](https://docs.unity3d.com/Manual/com.unity.localization.html): TR/EN veri tabanı ve smart string altyapısı.

Belge sürümü veya paket URL'si ileride eskirse kurulum gününde aynı Unity sürümünün Package Manager önerisi esas alınır.

## 10. Steam/Windows yayın kaynakları

### Hesap, ücret ve yayın

- [Steamworks onboarding](https://partner.steamgames.com/doc/gettingstarted/onboarding): ortaklık, banka ve vergi süreci.
- [Steam Direct Fee](https://partner.steamgames.com/doc/gettingstarted/appfee): güncel 100 USD başvuru ücreti ve geri kazanma eşiği.
- [Steamworks FAQ](https://partner.steamgames.com/doc/gettingstarted/faq?language=english): genel süreç ve zamanlama.
- [Store/build review](https://partner.steamgames.com/doc/store/review_process?language=english): mağaza ve build inceleme kriterleri.
- [Releasing](https://partner.steamgames.com/doc/store/releasing?language=english): çıkış kontrolü ve güncel bekleme kuralları.
- [Platform support](https://partner.steamgames.com/doc/store/application/platforms): Windows/macOS/Linux depot ve platform işleyişi.

### Özellikler

- [Steam Cloud](https://partner.steamgames.com/doc/features/cloud?language=english): cloud kayıt entegrasyonu.
- [Steam Input](https://partner.steamgames.com/doc/features/steam_controller): kontrolcü katmanı.
- [Steam Input geliştirici başlangıcı](https://partner.steamgames.com/doc/features/steam_controller/getting_started_for_devs?language=english): action yaklaşımı.
- [Steam Playtest](https://partner.steamgames.com/doc/features/playtest?language=english): davetli test akışı.
- [Steam marketing tools](https://partner.steamgames.com/doc/marketing/tools): görünürlük ve mağaza araçları.
- [Store asset rules](https://partner.steamgames.com/doc/store/assets/rules?language=english): kapsül ve görsel kuralları.
- [Localization languages](https://partner.steamgames.com/doc/store/localization): desteklenen dil ve mağaza yerelleştirmesi.

### İçerik, AI, yorum ve vergi

- [Steam Content Survey](https://partner.steamgames.com/doc/gettingstarted/contentsurvey?language=english): pre-generated/live-generated AI ve diğer içerik beyanları.
- [Steam Error Reporting](https://partner.steamgames.com/doc/features/error_reporting): hizmetin yaşam sonu/Windows 32-bit sınırı; modern native crash çözümü varsayılmamasının kaynağı.
- [Steam User Reviews](https://partner.steamgames.com/doc/store/reviews): yorum sistemi ve kullanıcı yorumlarını ticari tanıtımda kullanma sınırları.
- [Steam Tax FAQ](https://partner.steamgames.com/doc/finance/taxfaq?language=english): vergi onboarding'ine yönelik resmî başlangıç.

Steam belgeleri yayın tarihine kadar değişebilir. Her mağaza/build gönderiminde güncel Partner belgeleri yeniden kontrol edilir.

## 11. macOS ve Apple kaynakları

- [Apple Developer Program](https://developer.apple.com/programs/): güncel yıllık üyelik ücreti ve program kapsamı.
- [macOS notarization](https://developer.apple.com/documentation/security/notarizing_macos_software_before_distribution): imzalama/notarization akışı.
- [Unity 6.3 IL2CPP](https://docs.unity3d.com/6000.3/Documentation/Manual/il2cpp-introduction.html): platforma özgü native derleme sınırı.
- [Steam platform support](https://partner.steamgames.com/doc/store/application/platforms): macOS depot/platform yapılandırması.

Kullanım: macOS'un Windows 1.0'dan sonra ayrı bütçe, imza, shader, performans ve QA aşaması olması.

## 12. Erişilebilirlik kaynakları

- [Xbox Accessibility Guidelines ana sayfa](https://learn.microsoft.com/en-us/xbox/accessibility/guidelines): kapsam ve test yaklaşımı.
- [XAG 101 – Text display](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/101): metin sunumu.
- [XAG 103 – Additional channels](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/103): tek duyusal kanala bağlı olmama.
- [XAG 104 – Subtitles and captions](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/104): metin karşılığı.
- [XAG 107 – Input](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/107): input erişilebilirliği.
- [XAG 108 – Game difficulty options](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/108): birden çok zorluk ve temel mekaniklerin ayrı ayarlanması.
- [XAG 112 – UI navigation](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/112): arayüz gezinmesi.
- [XAG 116 – Time limits](https://learn.microsoft.com/en-us/xbox/accessibility/xbox-accessibility-guidelines/116): süre baskısı ve ayarlanabilirlik.
- [Unity Practical Game Accessibility](https://learn.unity.com/course/practical-game-accessibility): Unity uygulama örnekleri.

Kullanım: erişilebilirliği zorluktan ayırma, rebind, gamepad, kontrast, büyük hedef, hareket azaltma ve esnek tutorial.

## 13. Sürüm kontrolü ve üretim araçları

### Sürüm kontrolü

- [Unity DevOps fiyatlandırması](https://docs.unity.com/en-us/devops/pricing): UVCS ücretsiz kota ve ücret yapısı.
- [Unity 2026 fiyat/kota güncellemesi](https://unity.com/products/pricing-updates): UVCS 25 GB ücretsiz storage, 100 GB egress ve cloud seat ücreti değişiklikleri.
- [GitHub Pricing](https://github.com/pricing): özel depo ve planların güncel tabanı.
- [Git LFS faturalandırması](https://docs.github.com/en/billing/concepts/product-billing/git-lfs?apiVersion=2022-11-28): 10 GiB Free kota ve her binary sürümünün depolama etkisi.
- [Git LFS](https://git-lfs.com/): 3.7.1 güvenlik güncellemesi ve alternatif kurulum kaynağı.
- [Perforce Helix Core fiyatlandırması](https://www.perforce.com/resources/vcs/helix-core-pricing): küçük self-host kullanım ve ticari alternatif.

### 3D, görsel ve ses

- [Blender lisansı](https://docs.blender.org/manual/en/4.3/getting_started/about/license.html): GPL ve üretilen artwork ayrımı.
- [Poly Haven lisansı](https://polyhaven.com/license): CC0 varlıklar.
- [Kenney lisans/destek](https://kenney.nl/support): CC0 oyun varlıkları.
- [Unity Asset Store Terms](https://unity.com/legal/as-terms): mağaza varlıkları için koşullar.
- [Audacity](https://www.audacityteam.org/): ücretsiz/açık ses editörü.
- [REAPER lisansı](https://www.reaper.fm/purchase.php): güncel 60 USD discounted license koşulu ve evaluation.

Her araç/asset fiyatı ve lisansı satın alma/kurulum gününde tekrar doğrulanır. CC0 kaynak kullanmak marka, kişi, patent veya her üçüncü taraf hakkının otomatik temizlendiği anlamına gelmez.

## 14. Türkiye'de telif, marka ve oyun kayıt işlemleri

- [Kültür ve Turizm Bakanlığı – zorunlu kayıt-tescil](https://telifhaklari.ktb.gov.tr/TR-332371/zorunlu-kayit-tescil.html): bilgisayar oyunlarının güncel kapsamı ve başvuru bilgileri.
- [İsteğe bağlı kayıt-tescil](https://telifhaklari.ktb.gov.tr/TR-332450/istege-bagli-kayit-tescil.html): gönüllü kayıt seçenekleri.
- [İsteğe bağlı kayıt-tescil ek açıklama](https://telifhaklari.ktb.gov.tr/TR-332370/istege-bagli-kayit-tescil.html): güncel kurum bilgisi.
- [Eser sahibi kimdir?](https://telifhaklari.ktb.gov.tr/TR-332390/eser-sahibi-kimdir.html): eser sahipliği için resmî temel bilgi.
- [TÜRKPATENT araştırma](https://www.turkpatent.gov.tr/arastirma-yap?form=patent): final isim/marka araştırmasına başlangıç.

10 Ağustos 2026 tarihli resmî sayfa, yerli veya ithal bilgisayar oyunlarını zorunlu kayıt-tescil içinde saymaktadır ve 2026 ücretleri yayımlamaktadır. Ücret, işlem ve hukuki yorum release tarihinde uzmanla tekrar doğrulanmalıdır. Bu belge hukuki danışmanlık değildir.

## 15. Kaynakların bilinçli sınırları

- Rakip oyunların ücretli tam sürümleri bu araştırma için satın alınmadı veya kurulmadı.
- Hiçbir rakip executable'ı decompile edilmedi; kod/asset incelenmedi.
- Mekanikler resmî sayfa, trailer/oynanış gösterimi, yama notu, geliştirici açıklaması ve kullanıcı geri bildiriminden çıkarıldı.
- Steam yüzdeleri ve inceleme sayıları saatlik değişebilir.
- Yeni çıkan ReStory için uzun dönem tutunma, save ve güncelleme kalitesi henüz bilinmiyor.
- Henüz çıkmamış Computer Store Simulator vaatleri uygulanmış özellik sayılmadı.
- Topluluk hata raporu, yeniden üretilmedikçe kesin ürün kusuru değil; risk sinyalidir.
- Mevcut eski oyunun USB canonical hash doğrulaması 11 Ağustos 2026'da tamamlandı; bu sonuç o tarihteki snapshot içindir ve sonraki değişikliklerde manifest yeniden üretilmelidir.
- Motor, Steam, Apple, vergi, telif, gizlilik ve lisans koşulları release'e kadar yeniden kontrol edilmelidir.
- Gerçek PC mağazası süreçleri ülke, tedarikçi ve işletmeye göre değişir; oyunda eğlenceli ve kurgusal karşılık kullanılır.

## 16. Araştırmayı güncelleme takvimi

| Zaman | Yeniden bakılacak alan |
|---|---|
| Kurulumdan hemen önce | Unity LTS yaması, lisans, paket uyumu, disk ve UVCS kotası |
| Vertical slice başlangıcı | Rakip mağaza oyunlarının yeni yamaları ve kullanıcı sorun örüntüleri |
| Vertical slice bitişi | ReStory ve yeni PC mağaza oyunlarının uzun dönem yorumları |
| Steam sayfası öncesi | Pazar konumlandırması, final isim, marka benzerliği, store kuralları |
| Playtest öncesi | Steam Input/Cloud/Playtest ve privacy gereksinimleri |
| Release candidate | Steam review/release, AI survey, vergi, Türkiye kayıt-tescil ve lisans kanıtı |
| macOS port kararı | Apple program/notarization, Unity/Steam macOS destek ve pazar verisi |

Yeni araştırma sonucu yalnız bu listeye link eklemekle kalmaz; etkilediği karar `06_PROJE_HAFIZASI.md` içinde tarihli değişiklik kaydıyla güncellenir.
