# PC Shop Empire 3D – Yaşayan Proje Hafızası

**Sürüm:** 1.0 — üç Codex görevi tek ana görev ve merkezî konuşma arşivinde birleştirildi<br>
**Son güncelleme:** 15 Ağustos 2026<br>
**Kural:** Bu dosya karar ID'lerinin yaşayan özetidir. Üç görevin birleşik uygulanabilir bağlamı [`11_BIRLESIK_CODEX_PROJE_HAFIZASI.md`](11_BIRLESIK_CODEX_PROJE_HAFIZASI.md), tam kullanıcı/Codex konuşmaları ise [`Docs/CodexHistory`](../CodexHistory/README.md) altında korunur. Onaylanmış bilgi tekrar sorulmaz; değişiklik olursa eski karar silinmez, yerine tarihli bir değişiklik kaydı eklenir.

## Durum işaretleri

- **Onaylı:** Kullanıcı açıkça kabul etti veya tekrar tekrar aynı yönü doğruladı.
- **Proje lideri kararı:** Kullanıcının küçük/geri alınabilir kararları devretmesi üzerine alınmıştır.
- **Geçici:** Prototip veya test verisiyle doğrulanacaktır.
- **Açık:** Büyük etkili ve daha sonra kullanıcı kararı gerektirir.
- **Ertelendi:** Temel sürüme girmeyecek; ileride yeniden değerlendirilecek.
- **Kapsam dışı:** Mevcut ürün hedefinin parçası değildir.

## Kuzey yıldızı

**Vizyon:** Küçük bir garajdan başlayan, fiziksel perakende operasyonunu gerçekçi PC bilgisi ve uzun vadeli müşteri güveniyle birleştiren birinci şahıs teknoloji mağazası simülasyonu.

**Konumlandırma cümlesi:** Fiziksel teknoloji perakendesi + teknik PC ustalığı + müşteri güveni.

**Kalite ilkesi:** Çok sayıda yüzeysel özellik yerine, siparişten garantiye kadar aynı stok ve işlem gerçeğini kullanan az sayıda kusursuz zincir.

## Onaylanmış ana kararlar

### Kimlik, kapsam ve ticari model

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-001 | Yeni 3D oyun sıfırdan geliştirilecek. | Onaylı | Eski kod port edilmeyecek; yalnız davranış/veri referansı olacak. |
| D-002 | `PC Shop Empire` çalışma adıdır; final isim değişebilir. | Onaylı | Marka araştırması ve logo üretimi isim kilitlenene kadar ertelenir. |
| D-003 | PC Building Empire yalnız referanstır; isim, içerik veya varlık kopyalanmayacak. | Onaylı | Telif ve kimlik ayrımı korunur. |
| D-004 | Temel sürüm tek oyunculu olacak. | Onaylı | Ağ kodu ve co-op üretim riski v1'e eklenmez. |
| D-005 | Mimari gelecekte co-op'u tamamen engellemeyecek; fakat bunun için özellik yapılmayacak. | Proje lideri kararı | Durum kimlikleri ve işlem sınırları temiz tutulur. |
| D-006 | Premium tek seferlik satın alma modeli. | Onaylı | Reklam, abonelik, loot box ve mikro ödeme yok. |
| D-007 | Hata düzeltmeleri ve temel iyileştirmeler ücretsiz olacak. | Onaylı | Güven ve uzun ömür önceliği. |
| D-008 | Ücretli genişleme yalnız eksiksiz temel oyundan sonra, gerçekten büyük içerik için düşünülebilir. | Onaylı | Temel özellikler DLC'ye ayrılmaz. |
| D-009 | Varsayılan geliştirme bütçesi 0 avro. | Onaylı | Ücretsiz/açık araçlar ve lisanslı ücretsiz varlıklar önce gelir. |
| D-010 | Çok büyük kalite/iş gücü etkisi olan küçük ödeme kullanıcıya gerekçe, alternatif, fiyat, disk ve lisans etkisiyle sunulabilir. | Onaylı | Her satın alma ayrı onay kapısıdır. |

### Platform ve teknoloji

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-011 | Windows 10/11 64-bit ana ve zorunlu hedef. | Onaylı | Performans ve yayın kriterleri Windows'a göre belirlenir. |
| D-012 | Steam zorunlu mağaza/platform. | Onaylı | Steamworks, Cloud, Input, başarımlar ve yayın kapıları planlanır. |
| D-013 | macOS, Windows sürümü tamamlandıktan ve bütçe uygun olduktan sonra hedeflenecek. | Onaylı | İlk üretim kapsamı bölünmez; mimari Metal/macOS'a gereksiz engel koymaz. |
| D-014 | Linux yerel sürümü zorunlu değil. | Onaylı | Steam Deck/Proton daha sonra best-effort; yerel Linux vaadi yok. |
| D-015 | Başlangıç motor tabanı: Unity 6.3 LTS, URP, C#. | Onaylı | Mac geliştirme/Windows hedefleme dengesi; 6.3 bütün üretim ömrü için son sürüm sayılmaz. |
| D-016 | Unity veya başka araç henüz kurulmayacak; önce belge/onay kapısı. | Tarihsel kapı — D-145 ile 11 Ağustos 2026'da tamamlandı | Kurulum öncesinde araştırma ve ortak anlayışı korudu; kullanıcı Stage A kapsamını onayladıktan sonra araç kurulumu ayrı yetkiyle uygulandı. |
| D-017 | MacBook Air M4/32 GB ana geliştirme makinesi olabilir. | Onaylı; kapasite geçici | Resmî gereksinimleri karşılar ve prototipe uygundur; Apple Silicon'da CPU lightmapping yoktur, GPU lightmapper ile tam üretim import/build/bake kapasitesi milestone benchmark'ıyla doğrulanır. |
| D-018 | Windows x64 makine erken oynanabilir sürümden itibaren sağlanacak. | Onaylı; dış bağımlılık | Mac'ten erken Windows build Mono'dur; final IL2CPP/native eklenti ve DirectX/GPU/Steam QA gerçek Windows x64 cihazdadır. Windows ARM VM yalnız smoke testtir. |
| D-019 | Windows test kapıları: ilk oynanabilir, vertical slice, alpha ve release candidate. | Proje lideri kararı | Platform hataları birikmeden yakalanır. |
| D-139 | Unity 6.3 standart desteği Aralık 2027'de biteceğinden alpha öncesi desteklenen LTS'ye kontrollü yükseltme kapısı zorunlu. | Proje lideri kararı | Ayrı dalda paket/plugin, görsel, save, performans ve Windows IL2CPP regresyonu geçmeden yükseltme kabul edilmez. |

### Kamera, karakter ve sunum

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-020 | Birinci şahıs kamera. | Onaylı | Etkileşim ve ölçü algısı bu perspektife göre tasarlanır. |
| D-021 | Eller görünür olacak; özellikle montajda fiziksel bağ kuracak. | Onaylı | El rig'i, animasyon ve IK hero asset sayılır. |
| D-022 | Kurucu karakter sessiz ve özelleştirilebilir. | Onaylı | İsim, görünüm, ten/el görünümü; sabit sesli kahraman yok. |
| D-023 | Sanat tarzı grounded/semi-realistic. | Onaylı | Parçalar, malzeme ve ölçüler inandırıcı; insan yüzleri hafif stilize. |
| D-024 | Gerçek marka ve logolar yerine kurgusal ürün ekosistemi. | Onaylı | Lisans maliyeti ve katalog eskimesi azaltılır. |
| D-025 | Şehir kurgusal, uluslararası ve gerçekçi olacak. | Onaylı | Gerçek bir ülkenin hukukunu birebir simüle etmez. |
| D-026 | Dünya, ayrıntılı ayrı iş bölgeleri/lokasyonlar ve harita geçişlerinden oluşacak. | Onaylı | V1'de kesintisiz açık dünya sürüşü yok. |

### Oyuncu eylemi ve fizik

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-027 | Hibrit etkileşim fiziği. | Onaylı | Taşıma/yerleştirme serbest; hassas montaj snap ve yönlendirmeli. |
| D-028 | İri ürünler elde, arabada veya palette görünür. | Onaylı | Sihirli sınırsız envanter yok. |
| D-029 | Küçük araçlar sınırlı takım kemerinde taşınabilir. | Onaylı | Akış hızlanır, fiziksel kimlik korunur. |
| D-030 | Mağaza yerleşimi serbest + snap/grid yardımı. | Onaylı | Oyuncu yaratıcılığı ile rota güvenliği dengelenir. |
| D-031 | İşlevsel erişim ve NPC navigasyonu yerleştirme sırasında doğrulanır. | Proje lideri kararı | Takılan çalışan/müşteri riski azaltılır. |
| D-032 | Bina dış kabuğu sabit; iç bölmeler, kapılar, döşeme, ışık ve onaylı modüller değişebilir. | Onaylı | Tam inşaat simülasyonu olmadan anlamlı kişiselleştirme. |

### Temel mağaza ve stok

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-033 | Perakende ile PC montaj/servis eşit iki çekirdek sütun. | Onaylı | Biri diğerinin yan mini oyunu olmayacak. |
| D-034 | Sipariş Dashboard'dan verilir, ürün fiziksel teslim edilir. | Onaylı | Dijital karar ile fiziksel operasyon bağlanır. |
| D-035 | Teslimatlar planlı zaman pencerelerinde kurye/kutu/palet/kamyonla gelir. | Onaylı | Kapasite ve vardiya planlama doğar. |
| D-036 | Oyuncu veya yetkili çalışan teslimatı sayar, hasarı inceler ve kabul eder. | Onaylı | Eksik/hasarlı teslimat ve claim zinciri anlamlı olur. |
| D-037 | Stok katmanlı fiziksel temsil kullanır. | Onaylı | Aktif/kıymetli ürünler tekil; büyük hacim kapalı kutu/palet ve mantıksal defter. |
| D-038 | Değerli parçalarda seri, durum, garanti ve rezervasyon; düşük değerli sarfta batch takibi. | Proje lideri kararı | Gerçekçilik, performans ve kayıt boyutu dengelenir. |
| D-039 | Tek bir authoritative stok gerçeği olacak. | Proje lideri kararı | Raf, depo, el, çalışan, müşteri işi ve Dashboard aynı kimliği görür. |
| D-040 | Başlangıçta fiyat elle; ileride kategori kuralları ve öneriler. | Onaylı | Otomasyon kararı çalmaz, tekrarı azaltır. |
| D-041 | Fiyat/indirim/vergi/iade/online satış tek işlem anlık görüntüsünde hesaplanır. | Proje lideri kararı | Çifte satış ve fiyat değiştirme exploit'leri engellenir. |

### Müşteriler ve satış

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-042 | Müşteriler bütçe, ihtiyaç, sabır, teknik bilgi, tercih ve risk duyarlılığıyla farklılaşır. | Onaylı | Aynı ürünün farklı müşteride farklı değeri olur. |
| D-043 | Bilgi aşamalı ve kanıta dayalı açılır; zihin okuma yok. | Onaylı | Soru sorma ve danışmanlık oynanış olur. |
| D-044 | Diyalog bağlamsal, önceden yazılmış ve modüler. | Onaylı | Oyuncuya açık üretken AI sohbeti yok. |
| D-045 | Bazı müşteriler kalıcı ve tekrar eden karakter olur; sıradan kalabalık hafif simüle edilir. | Onaylı | Hikâye bağı ile NPC maliyeti dengelenir. |
| D-046 | İlk aşamada kasa elle; sonra kasiyer ve self-checkout otomasyonu. | Onaylı | Erken fiziksel öğrenme, geç ölçeklenme. |
| D-047 | Oyuncu otomasyondan sonra da istediği işe müdahale edebilir. | Onaylı | Büyüyen şirkette karakterin amacı kaybolmaz. |
| D-048 | Memnuniyetsizlik ve yorumlar nedenleriyle açıklanır. | Proje lideri kararı | Oyuncu neyi düzelteceğini bilir. |
| D-049 | İtibar fiyat, uzmanlık, hız, kalite, garanti ve güvenilirlik boyutlarından oluşur. | Onaylı | Tek sayı yerine stratejik kimlik. |
| D-050 | Reklam görünürlük getirir; itibarı doğrudan satın almaz. | Proje lideri kararı | Para ile sahte güven exploit'i yok. |
| D-051 | Meşru olumsuz yorum silinemez; yanıt ve telafi yapılabilir. | Proje lideri kararı | İtibar nedensel ve onarılabilir olur. |

### PC toplama ve teknik değer

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-052 | Dashboard'dan tek tuş PC toplama kaldırılacak. | Onaylı | Montaj fiziksel atölye işidir. |
| D-053 | Montaj oyuncu veya çalışan tarafından kasa seçiminden teslimata kadar yapılır. | Onaylı | Parça toplama, uyumluluk, OS, test, kablo, temizlik ve paketleme dahil. |
| D-054 | Uyumluluk katmanlı ve deterministik olacak. | Onaylı | Soket, chipset, RAM, form, güç, konektör, boyut, depolama ve termal temel çekirdektir. |
| D-055 | BIOS, lane, profil, header, airflow, gürültü ve sürücüler ilerleyen katmanlardır. | Onaylı | Yeni başlayan boğulmaz; uzman derinlik bulur. |
| D-056 | Hata mesajı yalnız “uyumsuz” demez; neden ve düzeltme yolunu gösterir. | Onaylı | Teknik öğrenme ve güven. |
| D-057 | İmkânsız fiziksel eşleşme bloke edilir; okunaklı risklerde ihmal hasar verebilir. | Onaylı | Rastgele cezadan kaçınılır. |
| D-058 | Hasar dereceli, okunabilir ve önlenebilir. | Onaylı | Kozmetik, performans, kararsızlık ve arıza farklı sonuçlar. |
| D-059 | Geri döndürülemez ciddi zarar yalnız açık risk ve daha yüksek zorluklarda nadir. | Onaylı | Adil zorluk. |
| D-060 | Tekrarlanan güvenli işler ustalık, araç ve şablonlarla hızlanır. | Onaylı | Vida/kablo/kurulum angaryası endgame'i boğmaz. |
| D-061 | Kurgusal işletim sistemi ve yazılım ekosistemi kullanılacak. | Onaylı | Gerçek OS arayüzü veya marka kopyalanmaz. |
| D-062 | Benchmark çok boyutlu olacak. | Onaylı | Oyun, üretkenlik, termal, gürültü, güç, stabilite ve depolama profilleri. |
| D-063 | Müşteri kabulü kullanım amacına göre ölçülür. | Onaylı | Tek sentetik puan bütün değeri belirlemez. |
| D-064 | Özel PC işi yazılı teklif ve kabul ölçütleri içerir. | Onaylı | Bütçe, tercih, teslim, garanti ve değişiklik onayı görünür. |
| D-065 | Depozito oranı ilişki ve riskle değişir; iptal şartları açık olur. | Onaylı | Nakit ve müşteri riski dengelenir. |

### Servis, ikinci el ve veri güvenliği

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-066 | Servis kanıta dayalı teşhis zinciri kullanır. | Onaylı | Belirti, intake, görsel inceleme, yeniden üretme, log, sıcaklık, güç ve known-good swap. |
| D-067 | Araçlar olasılığı daraltır; sihirli tek tık teşhis yok. | Onaylı | Teknik ustalık korunur. |
| D-068 | Servis teklifi güven düzeyi ve kanıt içerir. | Onaylı | Yanlış teşhis riski anlaşılır. |
| D-069 | Müşteri verisi okunabilir kişisel içerik olarak gösterilmez. | Onaylı | Mahremiyet ve yaş derecelendirme riski azaltılır. |
| D-070 | Yedekleme izni, erişim kaydı, güvenli silme ve veri teslim prosedürü olacak. | Onaylı | Servis güveni oyuna bağlanır. |
| D-071 | İkinci el/refurbish temel oyunda orta aşamada tam döngü olacak. | Onaylı | Trade-in, sahiplik, tanı, sanitize, temizlik, onarım, test, grade, garanti, fiyat. |

### Çalışanlar ve ölçeklenme

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-072 | Çalışan yönetimi politika tabanlı özerklik + doğrudan müdahale. | Onaylı | Rol, vardiya, bölge, öncelik ve görev kuyruğu. |
| D-073 | Roller satış, kasa, teknisyen, depo, temizlik, yönetim ve güvenliği kapsayabilir. | Onaylı | Rol ekleme veri güdümlü olur. |
| D-074 | Çalışanlar kalıcı, iş odaklı karakterler. | Onaylı | Tam yaşam simülasyonu yok. |
| D-075 | Hız, uzmanlık, hata eğilimi, maaş, eğitim, moral ve güvenilirlik farklıdır. | Onaylı | İşe alım anlamlı karar olur. |
| D-076 | Çalışan hatası skill, yorgunluk, araç, yük ve süre baskısından doğar. | Onaylı | Sabit rastgele hata zarından kaçınılır. |
| D-077 | Kritik engelde çalışan durur ve yardım ister. | Proje lideri kararı | Ürün kaybetme veya sessiz hata zinciri önlenir. |
| D-078 | Offscreen çalışan aynı durum kurallarıyla iş yapar; yalnız sunum ayrıntısı azalır. | Onaylı | Teleport/fabrikasyon yok. |
| D-079 | Eğitim iş üzerinde gözetim, mentorluk ve vardiya deneyiminden gelir. | Proje lideri kararı | Gerçek zaman bekleme grind'ı yok. |

### Ekonomi, tedarik ve rakipler

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-080 | Pazar dinamik, açıklanabilir ve sınırlı şoklu. | Onaylı | Nesil, sezon, etkinlik, arz, rakip ve oyuncu etkileri. |
| D-081 | Rastgele seed kayıtla saklanır; reload ile fiyat reroll edilemez. | Proje lideri kararı | Save-scumming azaltılır. |
| D-082 | Birden çok tedarikçi fiyat, hız, güvenilirlik, garanti, MOQ ve vadeyle ayrışır. | Onaylı | Tek en iyi tedarikçi yok. |
| D-083 | Tedarikçi ilişkileri indirim kadar kriz erişimi ve trade-off getirir. | Onaylı | İlişki stratejiktir. |
| D-084 | Muhasebe yönetsel ve anlaşılır olacak. | Onaylı | Cash, gelir, COGS, brüt kâr, maaş, kira, fatura, vergi karşılığı, borç, vade ve forecast. |
| D-085 | Vergiler kurgusal ve şeffaf; gizli ücret yok. | Onaylı | Eğitim değeri korunur, gerçek hukuk taklidi yapılmaz. |
| D-086 | Birkaç kalıcı ve sistemik rakip olacak. | Onaylı | Uzmanlıkları görülür; hile/rubber-band yapmazlar. |
| D-087 | Rakip etkisi fiyat, reklam, yorum, tedarikçi ve iş piyasasında görünür. | Onaylı | Soyut “rakip baskısı” açıklanır. |
| D-088 | Teknoloji zaman çizgisi kurgusal ve hızlandırılmış. | Onaylı | Nesil çıkışları önceden sinyallenir; eski ürün niche korur. |
| D-089 | Canlı gerçek dünya veri bağımlılığı yok. | Onaylı | Oyun offline ve dengelenebilir kalır. |

### Zaman, ilerleme, zorluk ve iflas

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-090 | Kariyer: garaj → mahalle dükkânı → teknoloji mağazası → amiral mağaza. | Onaylı | Her aşama yalnız metrekare değil operasyon değişimi getirir. |
| D-091 | Eski lokasyon satılabilir, kapatılabilir veya depo/servis/online/ikinci el birimine çevrilebilir. | Onaylı | Geçmiş yatırım anlamını korur. |
| D-092 | Uzak lokasyonlar yöneticilerle hafif simüle edilir. | Onaylı | CPU ve mikro yönetim kontrolü. |
| D-093 | Gün hedefi geçici olarak 25–30 gerçek dakika. | Geçici | Prep, açık saat ve kapanış sonrası dengelenecek. |
| D-094 | Oyuncu açılış/kapanışı kontrol eder; sınırsız ücretsiz gece çalışması yok. | Onaylı | Zaman stratejik kaynak olur. |
| D-095 | Dashboard varsayılan olarak zamanı durdurur; canlı sim opsiyon olabilir. | Onaylı | Yönetim ekranı adil ve erişilebilir. |
| D-096 | Fiziksel işler zaman ilerletir; PC işi birden çok güne yayılabilir. | Onaylı | Planlama değeri. |
| D-097 | Dört şirket zorluğu: rahat, standart, uzman, hardcore. | Onaylı | Geniş oyuncu profili. |
| D-098 | Şirket zorluğu sabit; gizli dinamik zorluk yok. | Onaylı | Ekonomi oyuncuya karşı hile yapmaz. |
| D-099 | Erişilebilirlik zorluktan ayrı. | Onaylı | Yardım seçenekleri cezalandırılmaz. |
| D-100 | İflas kademeli ve geri kazanılabilir olacak. | Onaylı | Uyarı, yapılandırma, küçülme, varlık satışı, kayyum/yeniden başlama basamakları. |
| D-101 | Kayıt otomatik silinmez. | Onaylı | Oyuncu zamanı korunur. |
| D-102 | Yeniden başlama aynı veya daha düşük zorlukta yapılabilir. | Onaylı | Başarısızlık öğrenmeye dönüşür. |
| D-103 | Yeni şirkette yalnız bilgi/QoL açılımları taşınabilir; para, stok, çalışan ve tedarikçi avantajı taşınmaz. | Onaylı | Mücadele korunur. |
| D-104 | İlk kariyer zaferi geçici 40–60 saatte sürdürülebilir amiral mağaza hedefi. | Onaylı/geçici denge | Zaferden sonra aynı kayıt endless devam eder. |
| D-105 | İsteğe bağlı New Company+ olabilir. | Onaylı | Tekrar oynanabilirlik, zorunlu reset değil. |

### Görev, hikâye ve tekrar oynanabilirlik

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-106 | Hikâye hafif ve sistemlerle iç içe. | Onaylı | Tekrarlanan müşteri, çalışan, rakip ve tedarikçi temsilcileri. |
| D-107 | Ham “5.000 ürün sat” grind'ı ana ilerleme olmayacak. | Proje lideri kararı | Başarımlar karar/ustalık/kriz çözümü odaklı. |
| D-108 | Etkinlikler ana şirket kaydını sıfırlamaz. | Onaylı | Sezon ayrı ve gönüllü olursa düşünülebilir. |
| D-109 | Farklı mağaza tipleri gerçekten farklı sistem baskıları yaratmalı. | Proje lideri kararı | Sadece dekor değişikliği yok. |
| D-110 | Ek gelir: online sipariş, kurumsal anlaşma, okul/ofis, e-spor, servis, ikinci el ve uygun kiralama seçenekleri. | Onaylı/evreli | Her kanal temel stok ve servis sistemini kullanır. |

### Kayıt, erişilebilirlik ve kalite

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-111 | Offline yerel kayıt her zaman çalışacak. | Onaylı | Steam Cloud bağımlılığı yok. |
| D-112 | Çoklu manuel kayıt, döner autosave ve kritik checkpoint'ler. | Onaylı | Gün başı, büyük alım, kredi, upgrade ve kriz öncesi kurtarma. |
| D-113 | Kayıt atomik yazma, doğrulama, schema, checksum, migration ve recovery kullanır. | Onaylı | Save güveni ürün özelliğidir; mutlak güç-kesintisi garantisi verilmez, flush/replace/fallback ve fault injection ile kanıtlanır. |
| D-114 | Steam Cloud hedeflenir; çatışma açıkça çözülür. | Onaylı | Cloud sessizce yerel ilerlemeyi ezmez. |
| D-115 | TR ve EN çıkışta insan kontrollü arayüz/metin. | Onaylı | Diğer diller demo verisi ve bütçeyle. |
| D-116 | Tam seslendirme yok; kısa evrensel sesler ve tüm bilginin metin/altyazı karşılığı. | Onaylı | Maliyet ve erişilebilirlik dengesi. |
| D-117 | Klavye/fare ve kontrolcü baştan. | Onaylı | Sonradan pahalı input dönüşümü önlenir. |
| D-118 | Tam tuş atama, glyph, hassasiyet, invert ve hold/toggle seçenekleri. | Onaylı | Erişilebilir birinci şahıs kontrol. |
| D-119 | Renk tek başına bilgi taşımaz; UI ölçeği, kontrast, büyük metin/hedef ve hareket azaltma. | Onaylı | Eski oyunun erişilebilirlik yaklaşımı genişletilir. |
| D-120 | Öğretici adım sırasına kilitli olmayacak; idempotent, tekrar açılabilir, resetlenebilir ve atlanabilir. | Proje lideri kararı | Rakiplerdeki softlock sınıfı önlenir. |

### PSE Guardian ve üretim AI politikası

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-121 | Yayınlanan oyunda görünmez, offline çalışan PSE Guardian Core bulunacak. | Onaylı | Oyun durumu ve hatalar izlenir. |
| D-122 | Guardian OpenAI, ChatGPT, Codex, dış API, anahtar veya üçüncü taraf model ağırlığı kullanmaz. | Onaylı | Bağımsızlık, maliyet ve mahremiyet. |
| D-123 | Guardian üretken AI değil; deterministik kural/invariant/işlem kontrolü ve sınırlı istatistiksel baseline öğrenmesi. | Proje lideri kararı | Açıklanabilir ve güvenli kalır. |
| D-124 | Guardian kaynak kodu veya oyun kodunu değiştiremez. | Onaylı | İnsan/Codex onayı olmadan patch yok. |
| D-125 | Shell, plugin, self-update, inbound network veya keyfi ekonomi düzenleme yetkisi yok. | Onaylı | Saldırı yüzeyi sınırlandırılır. |
| D-126 | Güvenli recovery yalnız önceden tanımlı işlemler olabilir. | Onaylı | Rezervasyon bırakma, görevi yeniden kuyruğa alma, son sağlam işlem/checkpoint. |
| D-127 | Online raporlama outbound-only ve açık rıza ile. | Onaylı | Offline sistem tam çalışır; oyuncu verisi zorunlu gönderilmez. |
| D-128 | Rapor neden zincirini ayrıntılı verir. | Onaylı | Beklenen/gerçek, olay zinciri, invariant, kanıt, güven, recovery, etki, repro ve fingerprint. |
| D-129 | Otomatik rapor SteamID, kullanıcı adı, dosya yolu, özel ad, ekran görüntüsü, serbest metin, ham save veya bellek dökümü içermez. | Onaylı | Veri minimizasyonu. |
| D-130 | Ham save/memory yalnız ayrı, açık ve olay bazlı izinle düşünülebilir. | Onaylı | Gizlilik varsayılanı korunur. |
| D-131 | Kodlama sürecinde Codex/AI kullanılabilir; kod insan incelemesinden geçer. | Onaylı | Geliştirme aracı verimliliği ürün içi “AI” diye pazarlanmaz; oyuncuya ulaşan shipped/live-generated çıktı varsa güncel Steam anketi ayrıca değerlendirilir. |
| D-132 | Oyuncunun tükettiği final art/ses/hikâye/çeviri varsayılan olarak insan yapımı veya doğrulanmış lisanslı. | Onaylı | Oyuncuya ulaşan GenAI istisnası ayrı hukuk/lisans ve o tarihteki Steam beyanı ister. |
| D-140 | Tanılama verisi “anonim” diye vaat edilmeyecek; IP/log ve teknik ID nedeniyle pseudonymous/kişisel veri ihtimali yönetilecek. | Proje lideri kararı | Oturum kapsamlı ID, DPA/processor, şifreleme, erişim, retention, silme ve rıza geri çekme politikası. |
| D-141 | Guardian aynı proses içindeki her native/hard crash'i yakalayamaz. | Proje lideri kararı | Breadcrumb + sonraki açılışta unclean shutdown; native crash SDK/handler ayrı lisans/gizlilik/onay kapısı. |

### İçerik ve varlık üretimi

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-133 | Kontrollü hibrit asset pipeline. | Onaylı | Prototipte CC0/generic; kimlik taşıyan hero assetler özel. |
| D-134 | Benzersiz üretilecekler: eller, PC parçaları, kutular/etiketler, kurgusal marka/UI, tezgâh, servis/kasa/Dashboard, ana NPC'ler. | Proje lideri kararı | “Asset flip” algısı azaltılır. |
| D-135 | Her asset için kaynak, lisans, indirme tarihi, değişiklik ve kullanım kaydı tutulur. | Onaylı | Yayın ve hak zinciri. |
| D-136 | Restricted/AI-training belirsiz/ham yeniden dağıtım riski taşıyan asset kullanılmaz. | Proje lideri kararı | Lisans riski azaltılır. |
| D-137 | Modüler ve data-driven mimari; v1'de resmî Workshop/editor yok. | Onaylı | Gelecek modlar için kapı, bugünkü kapsam korunur. |
| D-138 | Gelecekte mod olursa veri/asset sandbox; keyfi DLL yok. | Onaylı | Guardian modlu kayıtları işaretler. |

### Süreç kapıları

| ID | Karar | Durum | Etkisi |
|---|---|---|---|
| D-142 | Araştırma ve ön üretim tasarım paketi 0.1 ortak anlayış olarak kabul edildi. | Onaylı — 11 Ağustos 2026 | Canonical kaynak doğrulaması ve kesin araç/kurulum planı hazırlanabilir; gerçek kurulum, indirme ve proje oluşturma için ayrı kullanıcı onayı gerekir. |
| D-143 | USB'deki `02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU` canonical legacy snapshot'tır. | Doğrulandı — 11 Ağustos 2026 | USB ile yazılabilir yerel inceleme kopyası, hiçbir dosya değiştirilmeden salt-okunur yöntemle karşılaştırıldı ve 26/26 dosyada SHA-256 eşleşti; Mac paketleme çalışma kopyası ayrı türevdir. |
| D-144 | Legacy Electron güvenlik kanıtı kaynak/config düzeyiyle sınırlıdır. | Doğrulandı — 11 Ağustos 2026 | CSP tam self-only değildir (`'unsafe-inline'`/`data:` izinleri); fuse/ASAR ayarları config'de vardır, paketlenmiş binary fuse durumu ayrıca doğrulanmamıştır. |
| D-145 | Stage A kurulum kapsamı kullanıcı tarafından onaylandı ve uygulandı. | Onaylı — 11 Ağustos 2026 | Unity Hub 3.20.1, Unity 6000.3.21f1 ARM64, Windows Build Support (Mono) ve resmî VS Code uzantıları kuruldu; maliyet 0, ödeme yöntemi yok. |
| D-146 | Yeni authoritative Unity kaynak kökü `/Users/cixanla/Developer/PCShopEmpire3D/Game`; buildler `../Builds/Local` altında repo dışıdır. | Uygulandı — 11 Ağustos 2026 | Legacy kaynak ve canlı cache ayrımı fiziksel olarak korunur. |
| D-147 | Stage A baseline 4/4 Edit Mode testi, macOS Universal build/smoke ve Windows x64 Mono cross-build ile doğrulandı. | Doğrulandı — 11 Ağustos 2026 | Mac günlük geliştirmeye uygundur; Windows sonucu native runtime/IL2CPP/Steam/DirectX kanıtı değildir. |
| D-148 | Unity Cloud projesi ve ücretsiz/private UVCS repo oluşturuldu; ilk check-in bağlantı reseti nedeniyle tamamlanmadı. | Kısmen uygulandı — 11 Ağustos 2026 | Credential exchange başarılıdır; standalone ve Editor protokol yolları aynı `connection reset by peer` sonucunu verir; `.plastic` workspace oluşmadı ve tekrarlı deneme durduruldu. Gelecekteki ilk check-in yardımcısı klavye kısayolu taşımaz; repo/yol gösteren açık insan onayı, repo varlık/boşluk, workspace çakışma/mapping ve yarış-sonrası kontrolleri geçmeden değişiklik yapamaz. |
| D-149 | Yeni proje milestone yedeği USB'de legacy'den ayrı tarihli kaynak snapshot + SHA-256 manifest olarak tutulur; cache, build ve credential dahil edilmez. | Uygulandı — 11 Ağustos 2026 | İlk hedef `90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`; readback hash ve kaynak dry-run kapıları zorunludur. |
| D-150 | Kalan kullanım için bounded çalışma ve otomatik checkpoint protokolü uygulanır. | Kullanıcı talimatı — 11 Ağustos 2026 | Her tamamlanabilir iş commit/hash/USB checkpoint ile kapatılır; kullanıcı/panel ≤%5 bildirirse uzun işe başlanmaz, yaklaşık %2'de çalışma durdurulup durum kaydedilir ve kullanıcı uyarılır. Model hesap yüzdesini doğrudan göremediğinden panel veya kullanıcı bildirimi authoritative'dir. |
| D-151 | UVCS bağlantısı beklemeye alınır; Git `main` geçmişi tek authoritative VCS olur. | Proje lideri kararı — 11 Ağustos 2026; D-155 ile remote eklendi | UVCS'de workspace/changeset yoktur; Git root commit `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166` ve `stage-a-baseline-2026-08-11` etiketi korunur. Git LFS büyük binary asset öncesi ayrı kapıdır. |
| D-152 | Stage B, Unity/Editor bağımlılığı olmayan saf `PSE.Core` assembly sınırıyla küçük ve testli paketler halinde başlar. | Proje lideri kararı — uygulandı 11 Ağustos 2026 | Assembly anchor ve sınır testleri eklendi; 6/6 Edit Mode testi geçti; commit `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`. Henüz gameplay veya ürün domain modeli eklenmedi. |
| D-153 | Kalıcı domain kimlikleri tür kapsamlı, canonical `StableId<TScope>`; beklenen iş kuralı başarısızlıkları makine-okunur `Failure.Code` taşıyan `OperationResult` olur. | Proje lideri kararı — uygulandı 11 Ağustos 2026 | Girdi sessizce normalize edilmez; UI failure kodunu yerelleştirilmiş metne çevirir; boş hata, null başarı ve başlatılmamış sonuç güvenli invariant'larla ele alınır. Toplam 24/24 test geçti; commit `4cd2d928dbfda1886632bacce4a141c2a43161df`. |
| D-154 | Oyun zamanı yalnız açık adımlarla ilerleyen integer `SimulationClock` kaynağından akar; kalıcı alan olayları stable ID/type, one-based sequence, simulation timestamp ve schema taşıyan immutable zarf kullanır. | Proje lideri kararı — uygulandı 11 Ağustos 2026 | Pause veya hatalı/taşan adım zamanı değiştirmez; core `DateTime`, OS saati veya Unity frame zamanı okumaz. Event zarfı replay/duplicate/journal kanıtı sağlar; event bus ve persistence henüz eklenmedi. Toplam 42/42 test geçti; commit `8af2ad3d05906839c4b607e4958650e723060465`. |
| D-155 | Yeni oyunun authoritative remote'u private `cixanla/PC-Shop-Empire-3D`, dalı `main` olur; eski public `cixanla/PC-Shop-Empire` yalnız legacy release geçmişi olarak değişmeden kalır. | Kullanıcı onayı — uygulandı 11 Ağustos 2026 | Yerel history ve Stage A etiketi normal push ile taşındı; force-push/history rewrite yapılmadı. Repository private kalır; public dönüşüm ayrı secret/lisans/marka incelemesidir. |
| D-156 | Repository kendi kendini açıklayan yaşayan devir paketi taşır. | Kullanıcı talimatı — uygulandı 11 Ağustos 2026 | Root `PROJECT_BIBLE.md`, ayrıntılı `Docs/ProjectBible`, handoff/governance, ADR, changelog, issue/PR şablonları ve repo guard her material değişiklikte güncellenir. Canonical legacy 26 dosya byte-exact snapshot + SHA-256 manifest olarak private repodadır. |
| D-157 | Günlük yürütme GitHub Issues + private Development Roadmap Project ile izlenir; Codex'te çalışma mevcut ana proje içinde sürer. | Revize edildi — 13 Ağustos 2026 | 22 epic ve Status/Phase/Priority/Risk alanları korunur. Yanlışlıkla oluşturulan ayrı `Game` kaydı kaldırıldı; `/Users/cixanla/Developer/PCShopEmpire3D/Game`, `.git` ve GitHub remote'u değişmedi. Kalıcı karar gerçeği repository belgeleridir. |
| D-158 | Temel simülasyon rastgeleliği `pcg32-xsh-rr-64-32-v1` ile sürümlenir; raw state+odd increment snapshot'tır ve bounded integer modulo bias üretmez. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | Initial state bütün `ulong` aralığını kabul eder; benzersiz stream için selector `0..2^63-1` ile sınırlıdır ve high-bit alias reddedilir. Global/System/Unity RNG fallback yoktur; kriptografik kullanım yasaktır. Edit Mode toplamı 62/62 geçti; root-seed/context derivation sonraki pakettir. |
| D-159 | Kayıtlı root seed ve kalıcı domain/context kimliği `sha256-framed-be-pcg32-v1` ile çağrı sırasından bağımsız PCG32 initialization üretir. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | Root seed 16 lowercase hex; framed big-endian preimage ve iki golden vector sürümlüdür. Eksik/bozuk/bilinmeyen metadata fallback yapmaz. Aynı occurrence reload ile reroll olmaz; değişken-draw devamı ayrıca `Pcg32State` saklar. Edit Mode toplamı 85/85 geçti. |
| D-160 | Domain eventler zarf tarafından canonical payload'dan hesaplanan fingerprint/correlation/direct-causation taşır; in-memory dispatcher tek simulation thread'inde global FIFO, registration sırası ve breadth-first nested enqueue uygular. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | Mutation enqueue'da reddedilir veya dispatch'te karantinaya alınır; exact metadata+fingerprint duplicate idempotent, conflict açık failure; handler failure/exception izole, no-retry; receipt/drain kapasitesi zorunlu ve rapor raw payload/stack içermez. Kalıcı receipt ledger save paketine ertelendi. Edit Mode toplamı 105/105 geçti. |
| D-161 | İlk oynanabilir hareket tabanı CharacterController, izole runtime Input Action kopyası ve connected PlayerRig prefabı kullanan GarageGraybox olur. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | Klavye/fare + gamepad Move/Look/PrimaryAction/Interact/Sprint/Drop/Pause sözleşmesi, rebind override store, FOV/hassasiyet/invert/motion-reduce ve görünür prototip eller eklendi. Head-bob/sprint FOV/jump kapsam dışıdır. Edit Mode 114/114, gerçek device-state Play Mode 2/2, Universal macOS build ve runtime-ready smoke geçti; Windows native kapısı açıktır. |
| D-162 | İlk fiziksel ürün etkileşimi joint/spring fiziği değil, tek slotlu kinematic carry ve doğrulanmış safe-drop kullanır. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | `E/A` pickup, `G/B` drop; range+LOS, stable ID, collider/body snapshot, visible hands, dynamic prompt, blocked/no-support fail-closed ve disable/world-floor recovery eklendi. Küçük ürün root scale=1 invariant'ı zorunludur. Edit Mode 120/120, Play Mode 6/6 ve Mac player smoke geçti. |
| D-163 | Küçük kutu stock placement'ı yalnız işaretli yüzeyde, deterministik grid/yaw snap ve fail-closed ghost doğrulamasıyla yapılır. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | `PrimaryAction` (`Mouse Left/RT`) modu açar; `G/B` mod açıkken onaylar, kapalıyken safe-drop'u korur. İlk yüzey 0,25 m grid/90° yaw snap, beş noktalı taban desteği ve world/interactable/player overlap kontrolü kullanır. Geçerli placement gravity-off kinematic sabitlenir; stable ID/recovery korunur. Edit Mode 123/123, gerçek input Play Mode 8/8 ve Mac player smoke geçti. Serbest rotation, istifleme, büyük kutu ve Inventory authority kapsam dışıdır. |
| D-164 | Pickup/drop ve kontrollü küçük-kutu placement kaynakları ayrı tarihli USB milestone olarak korunur. | Uygulandı — 13 Ağustos 2026 | `2026-08-13_STAGE_B_SMALL_BOX_PLACEMENT`, source commit `7794e2a`; 336 tracked dosya / 5.928.850 bayt, manifest SHA-256 `b4df8efd...e0fb`. Çift manifest readback ve checksum dry-run geçti; cache/build/log/credential dışarıda bırakıldı. |
| D-165 | Büyük kutu ayrı, bounded bir carry profili kullanır; küçük-kutu placement sözleşmesini genişletmez. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | `LargeBox`: `0,65×` hareket, sprint kapalı, en çok `8°` içinde ilk `6°` FOV isteği ve geniş iki-el pozu. `motionReduced` açıkken lens cezası uygulanmaz; görünür kutu/eller geri bildirimi korur. Gerçek yarı boyutla safe-drop fail-closed, effective prompt, stable ID/tek slot/recovery geçerlidir. Placement/rotation/stacking ve taşıma arabası kapsam dışıdır. Edit Mode 126/126, gerçek Input System Play Mode 10/10 ve Mac `large-carry=ok` smoke geçti; commit `e944198`. |
| D-166 | Küçük kutu placement rotation'ı ayrı Input System eylemiyle deterministik clockwise 90° quarter-turn kullanır. | Proje lideri kararı — uygulandı 13 Ağustos 2026 | `R / Right Shoulder`; yalnız SmallBox placement modunda çalışır. Ghost ve confirm aynı yaw/solver girdisini, döndürülmüş footprint tam destek/obstruction kontrolünü kullanır; iptal/release/recovery'de sıfırlanır. Edit Mode 127/127, Play Mode 10/10 ve Mac `rotation=ok` smoke geçti; commit `661f2dc`. |
| D-167 | Oyunun görsel hedefi okunaklı yarı gerçekçiliktir; mevcut graybox final sanat değildir. | Kullanıcı onayı — 13 Ağustos 2026 | Gerçek oran, URP/PBR yüzey tepkisi, bevel/normal detayı, zemine oturan ışık ve doğal ağırlık; hafif stilize siluet/etkileşim okunabilirliğiyle birleşir. Fotogerçekçilik, lisansı belirsiz/ücretli asset ve full-scene dönüşümü otomatik kapsam değildir. İlk kapı Issue #34 tek benchmark köşesidir. |
| D-168 | Okunaklı yarı gerçekçilik önce tek ölçülebilir benchmark köşesinde uygulanır. | Uygulandı — 13 Ağustos 2026 | Prosedürel lisanslı yüzeyler, bevel, görev ışığı, ACES/bloom/reflection probe eklendi; gameplay collider/kimlik invariantları korundu. Edit Mode 128/128, Play Mode 10/10 ve Mac `lookdev=ok` geçti; commit `c7214af`. |
| D-169 | Küçük kutu istifi serbest rigidbody yığını değil, stable dünya desteğinde deterministik ve fail-closed placement ilişkisidir. | Uygulandı — 15 Ağustos 2026 | Yalnız kinematic/gravity-off `SmallBox`; merkez/90° snap, beş noktalı tam destek, overlap engeli, tek kat/tek üst, dolu taban pickup kilidi ve üst kutu alımında ilişki çözümü. Stable ID/recovery korunur; Inventory authority değildir. Edit Mode 131/131, Play Mode 12/12, Mac `stacking=ok`; commit `2e11e30`. |
| D-170 | Tamamlanan geliştirme paketleri USB'ye her dosya kaydında değil, doğrulanmış milestone snapshotı olarak yazılır. | Kullanıcı onayı — uygulandı 15 Ağustos 2026 | Güncel `2026-08-15_STAGE_B_IMMUTABLE_CHECKOUT_SNAPSHOT` snapshotı source `0936cc0`, 508 tracked source + 4 test/build/runtime evidence + source kaydı taşır; 513 manifest satırı `30c1e7fa…16efa` SHA-256, tam readback/source path+checksum kapılarından mismatch `0` ile geçti. `.git`, cache, büyük build uygulaması, credential ve AppleDouble dışarıda kalır. |
| D-171 | İlk taşıma arabası serbest rigidbody araç değil, tek `LargeBox` kapasiteli deterministik ve fail-closed dünya projeksiyonudur. | Uygulandı — 15 Ağustos 2026 | Hands→cart→hands aynı stable item ID/physics snapshot'ını korur; dört noktalı destek, hedef overlap ve swept bounds geçmeden pose uygulanmaz. Yüklü/boş hız `0,85×`/`0,90×`, sprint kapalıdır; cart/controller disable recovery'si yükü son güvenli dünya pozuna alır. Çoklu slot/palet ve authoritative Inventory yoktur. Edit Mode 136/136, Play Mode 14/14, Mac `transport-cart=ok`, `cart-flow=ok`; commit `82bf74f`. |
| D-172 | Mantıksal stok tek Unity-bağımsız Inventory authority tarafından yönetilir; dünya nesneleri açık adaptör kurulana kadar yalnız projeksiyondur. | Uygulandı — 15 Ağustos 2026 | `PSE.Catalog` immutable ürün/tracking tanımı; `PSE.Inventory` serialized item, bölünebilir batch position, unit-capacity container, atomik transfer ve claim reservation taşır. Release/consume, deterministic sorgu, revision ve invariant audit vardır; failure state değiştirmez. Fiyat/para, Orders, events, save ve world binding Issue #8+ kapsamındadır. Edit Mode 161/161, Play Mode 14/14; commit `71935f1`. |
| D-173 | Purchase order, onay veya arrival stok yaratmaz; yalnız exact manifestin fiziksel kabulü bütün satırları atomik receiving intake olarak ekler. | Uygulandı — 15 Ağustos 2026 | `PSE.Orders` stable order/supplier/delivery ve monotonik lifecycle taşır. Manifest order product/adet/tracking ile birebir eşleşir; Inventory bütün identity/capacity kontrolünü mutation öncesi yapar. Failure order+stok revision'ını değiştirmez, duplicate acceptance çoğaltmaz. Disk-crash atomikliği Save paketine aittir. Edit Mode 184/184, Play Mode 14/14; commit `e596e07`. |
| D-174 | Dünya nesnesi stok authority'si değil, Inventory container durumunun transactional görünür projection'ıdır. | Uygulandı — 15 Ağustos 2026 | Exact serialized item `Arrived → Receiving → ActorHands → Shelf/WorldFloor` zincirinde explicit Presentation adaptörüyle taşınır. Domain transferi önce, fiziksel mutation sonra gelir; domain failure no-world-mutation, physical failure domain rollback, recovery iki durumu birlikte son güvenli konuma alır. RAF A açık zone eşlemesi kullanır. Edit Mode 188/188, Play Mode 17/17, Mac `stock-flow=ok`; commit `9d75573`. |
| D-175 | Fiziksel delivery parcel opening Inventory mutation değil, accepted exact manifestin idempotent görünür reveal işlemidir. | Uygulandı — 15 Ağustos 2026 | Kapalı dış parcel yalnız order Accepted, item Receiving'de ve manifest/binding exact ise açılır. İlk transition ürün kutusu + dünyada kalan açık kabuğu gösterir; repeated open transition/duplicate üretmez, Inventory/Orders revision sabit kalır. Açılmamış item pickup edilemez; invalid state/identity/location no-reveal. Edit Mode 192/192, Play Mode 17/17, Mac `parcel-open=ok`; commit `3766f3f`. |
| D-176 | Raf fiyatı Inventory, world item veya Unity etiketi değil; stable product+shelf kimlikli Unity-bağımsız Retail offer authority state'idir. | Uygulandı — 15 Ağustos 2026 | `PSE.Retail` üç harf currency ve iki ondalıklı pozitif bounded integer minor-unit fiyat kullanır. Exact Set idempotent, update tek revision; unknown/non-shelf/invalid/duplicate/conflict failure no-mutation kalır. RAF A etiketi yalnız başarılı `E / Gamepad South` publish sonrası `549,99 EUR` gösterir; Inventory/Orders state değişmez. Checkout snapshot/ledger ayrıdır. Edit Mode 207/207, Play Mode 17/17, Mac `shelf-offer=ok`; commit `7a23cd9`. |
| D-177 | Müşteri sepeti, Unity etiketi veya fiziksel nesne durumu değil; exact shelf offer + serialized item + Inventory claim bağlayan Unity-bağımsız Retail authority state'idir. | Uygulandı — 15 Ağustos 2026 | Stable customer/basket/line kimlikleri kullanılır. Reserve yalnız exact offer Shelf konumunda başarılıdır; duplicate item/customer conflict, unknown/mismatch ve cross-authority drift failure no-mutation kalır. Başarı Retail+Inventory revision'ını birer kez artırır, total quantity değişmez; release availability'yi geri getirir. RAF A `G / Gamepad East` reserve/release ve reserved `E / South` pickup fail-closed kanıtı verir. Basket line fiyat snapshot'ı taşımaz; checkout ayrıdır. Edit Mode 220/220, Play Mode 17/17, Mac `basket-reservation=ok release=ok`; commit `45c2cdc`. |
| D-178 | Açık checkout fiyatı mutable shelf offer değildir; bütün aktif basket satırları preflight edildikten sonra stable kimlikler, integer price/currency/total ve source offer revision immutable Retail checkout snapshot'ına alınır. | Uygulandı — 15 Ağustos 2026 | Başarı yalnız Checkout revision'ını ilerletir; Basket/Inventory/ShelfOffer/Orders sabit kalır. Exact repeat idempotent, duplicate/identity/mixed-currency/stale/drift failure no-mutation'dır. Sonraki offer update'i snapshot'ı değiştirmez. RAF A Mouse Left/RT checkout başlatır; aktif checkout release/pickup fail-closed. Edit Mode 233/233, Play Mode 17/17, Mac `checkout-snapshot=ok price-frozen=ok`; commit `294999f`. |
| D-179 | Checkout fulfillment satır satır mutation değil, exact reservation setinin tam preflight sonrası tek Inventory commit'i ve stable immutable completion kaydıdır. | Uygulandı — 15 Ağustos 2026 | Bulk serialized/batch consume başarıda Inventory revision'ını bir kez ilerletir; Basket ve Checkout kendi revision'larını birer kez ilerletip item/reservation/line'ı birlikte kapatır, Offer/Orders sabit kalır. Exact repeat ve completed begin idempotent; identity/time/drift failure cross-authority no-mutation. RAF A'da ikinci Mouse Left/RT projection'ı kaldırır, `TAMAMLANDI`, stok 0 gösterir. Fulfillment ödeme değildir. Edit Mode 242/242, Play Mode 17/17, Mac `sale-completion=ok stock-consumed=ok`; commit `bb89b0c`. |

## Vertical slice kilidi

**Dahil:**

- Tek garaj ve teslimat dış alanı.
- Birinci şahıs, görünür eller, alma/taşıma/yerleştirme.
- Terminalden sipariş; kurye, kutu, kabul, raf ve fiyat.
- Temel müşteriler, danışmanlık, kuyruk ve kasa.
- Bir özel PC'nin teklif → parça → montaj → kurgusal OS → benchmark → paket → teslim zinciri.
- Temel Dashboard, ledger, ekonomi, save/recovery.
- Guardian olay kaydı ve neden raporu iskeleti.
- Teknoloji prototipinde daha küçük, slice sonuna doğru yaklaşık 50–80 anlamlı SKU.

**Dahil değil:**

- Çalışan sistemi.
- Şubeler ve uzaktan yönetim.
- Tam servis/refurbish zinciri.
- Gelişmiş rakip simülasyonu.
- Online satış ve kurumsal anlaşma.
- Resmî mod desteği.
- Co-op.
- macOS ticari yayın.

## Geçici varsayımlar

| ID | Varsayım | Nasıl doğrulanacak? |
|---|---|---|
| A-001 | 25–30 dakikalık oyun günü yeterli nefes ve baskı sağlar. | 3 farklı oyuncu profilinde zaman-görev telemetrisi. |
| A-002 | İlk zafer 40–60 saatte tatmin edici olur. | Ekonomi simülasyonu ve uzun dönem playtest. |
| A-003 | 50–80 slice SKU'su yeterli karar çeşitliliği verir. | Sipariş çeşitliliği ve tekrar oranı ölçümü. |
| A-004 | Unity URP, hedef görünüm ve orta sınıf Windows performansını dengeler. | Gri kutu ve sanat vertical slice benchmark'ı. |
| A-005 | Tek garaj slice, iki çekirdek sütunun pazar değerini gösterebilir. | Demo oturum tamamlama, tekrar oynama ve nitel görüşme. |
| A-006 | Deterministik Guardian, görünmez tanılama ve bütünlük ihtiyacını açıklanabilir biçimde karşılar. | Hata enjeksiyonu, yanlış pozitif oranı ve rapor anlaşılırlığı. |
| A-007 | MacBook Air'in tam üretim import/build/bake yükü kabul edilebilir. | Her milestone'da termal süre, import, build, bellek ve GPU lightmapping benchmark'ı. |

## Açık büyük kararlar

Kullanıcının isteği doğrultusunda yalnız gerçekten büyük kararlar sorulacak ve her seferinde tek soru yöneltilecek.

1. **Final oyun adı ve marka kimliği:** Vertical slice'ın görsel kimliği oturmadan önce.
2. **Şirket/hak sahibi ve yayın ülkesi düzeni:** Steam onboarding, sözleşme, vergi ve Türkiye'deki kayıt yükümlülüğü öncesi.
3. **Erken Erişim mi, doğrudan 1.0 mı:** Stabil demo/playtest verisi ve maddi durum görüldükten sonra.
4. **İlk ücretli harcama:** Yalnız ölçülmüş üretim darboğazı varsa.
5. **macOS yayın kararı:** Windows 1.0 ve bütçe sonrası.
6. **İleri seviye resmî mod desteği:** 1.0 stabilitesinden sonra.

## Ertelenen fikirler

- Co-op/multiplayer.
- Kesintisiz açık dünya ve araç sürüşü.
- Yerel Linux sürümü.
- Steam Workshop ve tam mod editörü.
- Çok şubeyi birebir aynı ayrıntıda canlı simüle etmek.
- Gerçek marka lisansları.
- Tam seslendirme.
- Bulut zorunlu hesap.
- Oyuncuya açık üretken AI müşteri konuşması.
- Canlı gerçek dünya donanım fiyatları.

## Kapsam dışı

- Eski Electron kodunu Unity içine sarmak veya otomatik port etmek.
- Eski menüleri, görselleri, CSS'i veya gerçek marka listesini aynen kullanmak.
- Reklam, gacha, loot rarity ekonomisi veya pay-to-win.
- Ana şirket kaydını sezon sonunda silmek.
- Hile yapan rakip veya gizli rubber-band ekonomi.
- Guardian'ın kendi kodunu değiştirmesi ya da internetten model/komut indirmesi.
- Oyuncunun görebildiği veya sohbet edebildiği Guardian arayüzü.
- Tam işçi yaşam/romantizm simülasyonu.

## Bilinen ana riskler

| ID | Risk | İlk karşılık |
|---|---|---|
| R-001 | Kapsam patlaması | Vertical slice kilidi; her yeni fikir backlog'a, çekirdeğe değil. |
| R-002 | PC kombinasyon test patlaması | Az aile, veri doğrulama, çözüm kanıtlayıcı, pairwise + kritik tam matris. |
| R-003 | NPC/çalışan pathfinding ve görev kaybı | Yerleşim doğrulama, rezervasyon, watchdog, idempotent görevler. |
| R-004 | Save bozulması | Atomik snapshot+journal, döner yedek, migration, fault injection. |
| R-005 | Fizik jank'i | Hassas işlerde snap/animasyon; serbest rigidbody yalnız uygun nesnelerde. |
| R-006 | Tekrar ve mikro yönetim | Ustalık hızlandırmaları, politikalar, çalışanlar, batch işlemler. |
| R-007 | Ekonomi deadlock/iflas adaletsizliği | Nakit forecast, uyarı, işletme sermayesi tamponu, kademeli kurtarma. |
| R-008 | Asset-flip algısı | Benzersiz hero asset ve tutarlı art direction. |
| R-009 | Lisans/telif/marka | Provenans defteri, kurgusal markalar, hukuk kapısı. |
| R-010 | Mac'te Windows hatalarının geç görülmesi | Erken ve düzenli fiziksel Windows testleri. |
| R-011 | Fansız Mac'te uzun build/import | Batch küçültme, cache, düşük önizleme, zamanlanmış ağır işler. |
| R-012 | Guardian yanlış pozitif veya gizlilik sorunu | Yalnız açık invariant, güven puanı, opt-in, redaksiyon testleri. |
| R-013 | Oyuncunun iki çekirdek arasında bölünmesi | Açılış/kapanış, randevu, servis kotası ve çalışan kapsaması. |
| R-014 | Tutorial softlock | Durum sorgulayan hedefler, alternatif sıra, skip/reset, otomatik çözüm denetimi. |
| R-015 | “Her şeyi yapıyor ama hiçbir şeyi iyi yapmıyor” algısı | Önce tek garaj zincirini cilalamak. |
| R-016 | Stok, ledger ve save durumunun ayrışması | Tek authoritative durum, atomik transaction, seed, uzlaşma testi ve Guardian invariant'ı. |
| R-017 | Özgün sanat üretiminin sistem geliştirmesini kilitlemesi | Modüler kit, hero asset önceliği ve yalnız ölçülmüş darboğazda dış destek. |
| R-018 | Araç, cloud kota ve depolama maliyetinin gizlice büyümesi | Aylık kota/disk/lisans denetimi; ödeme yöntemi eklemeden önce ayrı onay. |
| R-019 | Tek geliştirici çalışma yükü ve tükenme | Aynı anda tek ana milestone, küçük haftalık teslim ve kapsam/saat yeniden tabanlama. |
| R-020 | Rakip taklidi veya hazır asset şablonu algısı | Kurgusal kimlik, özgün hero varlıklar ve güven/teknik değer üzerinden mekanik ayrım. |
| R-021 | Unity 6.3 desteğinin üretim tamamlanmadan bitmesi | Alpha öncesi zorunlu, ayrı dalda desteklenen LTS yükseltme kapısı. |
| R-022 | Gerçek Windows x64 test cihazının zamanında sağlanamaması | İlk oynanabilir dış bağımlılığı; cihaz/bütçe alternatifi milestone öncesi çözülür. |
| R-023 | Native hard crash ve raporlama gizliliğinin Guardian kapsamı sanılması | Unclean shutdown ayrımı; crash SDK için ayrı lisans, DPA, redaksiyon ve retention kararı. |
| R-024 | UVCS cloud dosya protokolünün mevcut ağ/rota/hizmet katmanında uzak uç tarafından sıfırlanması | UVCS tekrarları durduruldu; private GitHub `main` authoritative uzak geçmiş, USB hash snapshot ayrı off-device katmandır. Git LFS kararı büyük binary asset öncesi verilir. |

## Değişiklik protokolü

1. Büyük karar değişikliği tek soru ve kısa etki analiziyle kullanıcıya sunulur.
2. Onaylanırsa yeni karar ID'si veya tarihli revizyon eklenir.
3. Etkilenen GDB, yol haritası, risk ve kapsam bölümleri aynı turda güncellenir.
4. Kod başladıktan sonra kararın save schema, içerik, test ve migration maliyeti ayrıca yazılır.
5. Küçük, geri alınabilir uygulama kararlarını proje lideri verir ve sonraki raporda topluca bildirir.

## Sonraki kayıt girişi

Paket ve Stage A kapsamı 11 Ağustos 2026'da onaylandı ve uygulandı. Stage B Core, oynanabilir GarageGraybox fiziksel akışı, görsel benchmark, Catalog + Inventory authority, atomik purchase-order receiving, transactional dünya/stok projection'ı, idempotent parcel reveal, authoritative shelf offer, customer basket reservation, immutable checkout price snapshot ve atomik checkout fulfillment ayrı testli paketlerde kapatıldı. Güncel baseline Edit Mode 242/242 ve Play Mode 17/17'dir; sıradaki bounded alan Issue #9 altındaki müşteri intent/state ve timeout/fallback sözleşmesidir.

## Oturum checkpoint'i — 10 Ağustos 2026

Kullanıcı bu noktada konuşmanın korunmasını ve daha sonra aynı yerden devam edilmesini istedi.

**Tamamlanan durum:**

- Eski Electron proje, 14 Dashboard bölümü ve dönüşüm sınıfları salt-okunur yöntemle incelendi.
- Rakipler, resmî oynanış/yama kaynakları ve oyuncu risk sinyalleri araştırıldı; olgu–çıkarım ayrımı çapraz kontrolden geçirildi.
- Game Design Bible, teknik mimari, Guardian sınırları, yol haritası, kaynak defteri ve bu Proje Hafızası oluşturuldu.
- Unity 6.3 yalnız başlangıç tabanı olarak kaydedildi; alpha öncesi kontrollü LTS yükseltme kapısı eklendi.
- Mac'ten erken Windows build'in Mono, final Windows build'in gerçek Windows x64 üzerinde IL2CPP olacağı netleştirildi.
- Guardian'ın oyuncuya kapalı, kod değiştiremeyen ve hard crash'te sınırlı olduğu; raporlamanın opt-in ve pseudonymous veri riski taşıdığı kaydedildi.

**Hiç yapılmayanlar:** Kod yazma, Unity/Blender kurma, ücretli satın alma, büyük indirme, yeni motor projesi açma veya eski proje/USB/kayıt değiştirme.

**Tarihsel devam notu — tamamlandı:** Bu checkpoint'te bekleyen belge onayı, USB hash doğrulaması ve kesin araç planı 11 Ağustos 2026'da tamamlandı. Güncel durum aşağıdaki checkpoint'tir.

## Oturum checkpoint'i — 11 Ağustos 2026

- Kullanıcı 0.1 araştırma ve tasarım paketini ortak anlayış olarak onayladı.
- USB yeniden bağlandı; canonical `KAYNAK_KODU` ile yerel inceleme kopyası 26/26 dosyada göreli yol, boyut ve SHA-256 düzeyinde eşleşti.
- USB, eski proje, canlı kayıtlar ve Mac paketleme kopyası değiştirilmedi.
- Kesin Stage A planı hazırlandı: Unity 6000.3.21f1 ARM64 + URP, Windows Build Support (Mono), mevcut VS Code için resmî Microsoft Unity/C# araçları ve ücretsiz/private UVCS önerildi.
- İlk kurulum maliyeti 0; tahmini indirme 12–22 GB, kurulu uygulama/ilk cache 25–40 GB, güvenli rezerv en az 80 GB olarak planlandı.
- Blender, ücretli araçlar, Steam/Apple ödemeleri, asset edinimi ve gameplay geliştirmesi bu kapının dışında bırakıldı.
- Stage A kapsamı daha sonra kullanıcı tarafından onaylandı ve uygulandı: Hub 3.20.1, Unity 6000.3.21f1 ARM64, Windows Mono modülü ve Microsoft VS Code araçları kuruldu.
- Yeni URP proje legacy'den ayrı kaynak kökünde oluşturuldu; resmî paketler kilitlendi; 4/4 test, macOS Universal build/smoke ve Windows x64 Mono cross-build geçti.
- Unity Cloud projesi ile ücretsiz/private UVCS repo oluşturuldu; ödeme yöntemi eklenmedi. Credential exchange başarılı olsa da uzak bağlantı reseti ilk check-in'i engelledi ve yerel workspace oluşmadı.
- Legacy canonical manifest kurulum sonunda yeniden 26/26 eşleşti; yeni proje için ayrı, cache/build/token içermeyen hash doğrulamalı USB Stage A snapshot'ı oluşturuldu.
- UVCS bağlantı preflight'ı beklemeye alındı; yerel Git tek authoritative geçmiş olarak başladı. Gameplay Stage B, Blender, Steam/Apple ödemeleri, asset edinimi, remote/LFS ve gerçek Windows IL2CPP ayrı kapılardır.

### Güncel devam eki — yerel Git ve kullanım protokolü

- Kullanıcı kalan kullanımın yaklaşık %30 olduğunu bildirdi ve %2 civarında işin güvenli checkpoint ile durdurulmasını istedi.
- Hesap kullanım yüzdesi model tarafından doğrudan okunamadığı için kullanıcı/panel bildirimi authoritative kabul edilir; her bounded iş ayrıca tamamlanmış commit ve USB manifestle kapatılır.
- Unity proje kökünde yerel Git `main` deposu oluşturuldu; 81 kaynak dosyası, 273.462 bayt, bilinen secret kalıbı 0 ve generated/cache yolu 0 olarak ilk commite alındı.
- Root commit `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166` ve `stage-a-baseline-2026-08-11` etiketi korunur. Daha sonra private `cixanla/PC-Shop-Empire-3D` origin eklendi; `main` ve etiket normal push ile gönderildi.
- UVCS bu sırada ikinci authoritative sistem değildir; `.plastic` workspace ve uzak changeset yoktur.
- Stage B'nin ilk bounded teknik paketi tamamlandı: Unity/Editor referansı taşımayan `PSE.Core` assembly iskeleti ve iki sınır testi eklendi; toplam Edit Mode sonucu 6/6 geçti.
- İlk Stage B paketinin HEAD'i `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`, tree'si `d6ff7d59ae8ff311e4e4f05b243fd4f2d7989d46`; `git fsck --full` geçti.
- İkinci Stage B paketi tür kapsamlı stable ID, failure code ve generic/non-generic sonuç sözleşmelerini ekledi; toplam 24/24 Edit Mode testi geçti.
- İkinci Stage B paketinin HEAD'i `4cd2d928dbfda1886632bacce4a141c2a43161df`, tree'si `1814b4a59f6de378130913733192261bc19802ba`; önceki iki commit ve Stage A etiketi korunur.
- Üçüncü Stage B paketi açık-adımlı monotonik oyun zamanı ile stable metadata taşıyan immutable alan olayı zarfını ekledi; toplam 42/42 Edit Mode testi geçti.
- Güncel HEAD `8af2ad3d05906839c4b607e4958650e723060465`, tree `566c1884e681feb8fbf0f68e0fb0a7594b560012`; çalışma ağacı temizdir. Önceki commitler ve Stage A etiketi korunur.
- İş birliği/devir paketi `2ee421193833111f76c85dabb33910240c36db03` commit'iyle private remote'a gönderildi: yaşayan Bible, governance/handoff, workflow, issue/PR şablonları, 26/26 legacy snapshot ve manifest.
- GitHub'da 22 epic ile private Project #2 oluşturuldu. Sonradan gereksiz olduğu belirlenen ayrı Codex `Game` kaydı 13 Ağustos'ta kaldırıldı; kaynak ve GitHub bağlantısı korundu.
- Dördüncü Stage B paketi sürümlü PCG32 akışını, raw snapshot/restore'u ve bias'sız bounded integerı ekledi; toplam 62/62 Edit Mode testi geçti.
- Beşinci Stage B paketi canonical root seed ve sürümlü SHA-256 framed domain/context stream derivation'ı ekledi; toplam 85/85 Edit Mode testi geçti.
- Altıncı Stage B paketi canonical payload fingerprint/mutation karantinası/correlation/causation ve bounded deterministik event dispatcher'ı ekledi; toplam 105/105 Edit Mode testi geçti.
- Yedinci Stage B paketi `c7a3a26075998252d9ae8b88824d8285e5067069` commit'iyle oynanabilir GarageGraybox, connected PlayerRig, klavye/fare + gamepad hareket/kamera, rebind temeli ve görünür prototip elleri ekledi; Edit Mode 114/114, Play Mode 2/2, Universal macOS build ve runtime-ready smoke geçti.
- Sekizinci Stage B paketi `44b816289f942e57fc176b26b203711090d0e61c` commit'iyle stable fiziksel ürün, hedefleme, görünür el durumları, güvenli pickup/drop ve recovery ekledi; Edit Mode 120/120, Play Mode 6/6 ve Universal macOS runtime smoke geçti.
- Dokuzuncu Stage B paketi `720e6d4ac2b2afad9ee86f907c533cbabb1bf5ed` commit'iyle işaretli stock surface, deterministik snap, geçerli/geçersiz ghost, stabil placement ve gerçek keyboard/mouse + gamepad testlerini ekledi; Edit Mode 123/123, Play Mode 8/8 ve Universal macOS runtime smoke geçti.
- Pickup/drop + placement kaynağı `7794e2ab82c3b26c1149af526ed582f1cc406acb` commit'inden ayrı USB milestone'a alındı; 336 dosyalık manifest çift readback/checksum ile doğrulandı.
- Onuncu Stage B paketi `e94419862b04f6f03f97ef2e43c9da393c5d30a9` commit'iyle ayrı büyük-kutu graybox/carry profili, iki-el durumu, bounded hız/FOV maliyeti, sprint kilidi, fail-closed drop ve gerçek keyboard/gamepad testlerini ekledi; Edit Mode 126/126, Play Mode 10/10 ve Universal macOS `large-carry=ok` runtime smoke geçti.
- On birinci Stage B paketi `661f2dcc64246a8282fd63fbf303454ec856ea40` commit'iyle küçük-kutu placement moduna `R / Right Shoulder` deterministik 90° rotation, etkin prompt/açı, döndürülmüş footprint doğrulaması ve görünür yön işareti ekledi; Edit Mode 127/127, Play Mode 10/10 ve Universal macOS `rotation=ok` runtime smoke geçti.
- On ikinci Stage B paketi `c7214afab81a360a3ca10a88cbdd29f67e741994` commit'iyle tek-köşe okunaklı yarı gerçekçi benchmarkı ekledi; Edit Mode 128/128, Play Mode 10/10 ve Mac `lookdev=ok` geçti. Sıradaki paket tam destekli küçük-kutu istiflemedir; taşıma arabası ve Inventory authority ayrı kalır.
- On üçüncü Stage B paketi `2e11e30a1a4b3435046ae18001004cacc170079e` commit'iyle stable küçük kutu üstünde merkez/90° snap, beş noktalı tam destek, overlap engeli, tek kat/tek üst ilişki, dolu taban pickup kilidi ve gerçek keyboard/gamepad testlerini ekledi; Edit Mode 131/131, Play Mode 12/12 ve Universal macOS `stacking=ok` runtime smoke geçti. Sıradaki paket taşıma arabasıdır; Inventory authority Issue #7/#8'e bağlı kalır.
- On dördüncü Stage B paketi `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1` commit'iyle tek `LargeBox` kapasiteli stable platform arabasına hands→cart→hands transferi, dört noktalı destek/swept obstruction, yüklü/boş hız, sprint kilidi, dinamik prompt ve fail-closed recovery ekledi; Edit Mode 136/136, Play Mode 14/14 ve Universal macOS `transport-cart=ok`, `cart-flow=ok loaded=ok stable=ok` runtime smoke geçti. Sıradaki paket Issue #7 Catalog + Inventory çekirdeğidir; fiziksel projection ekonomik authority değildir.
- On beşinci Stage B paketi `71935f11b80d02d03f9dcc1a3f08cafca7e301ff` commit'iyle Unity bağımsız Catalog + Inventory authority, serialized/batch/container/transfer/reservation invariantları ve failure no-mutation revision sözleşmesini ekledi; Edit Mode 161/161 ve regresyon Play Mode 14/14 geçti. Sıradaki paket Issue #8 sipariş/teslimat/raf bağlantısıdır; fiziksel projection kendiliğinden stok değiştirmez.
- On altıncı Stage B paketi `e596e079d90b6d5b9d94714d7821502574eba3c9` commit'iyle Unity bağımsız PSE.Orders lifecycle/exact manifest ve Inventory bulk-intake ekledi; kabul öncesi stok `0`, başarıda tek revision, bütün failure yollarında order+stok no-mutation kanıtlandı. Edit Mode 184/184 ve Play Mode 14/14 geçti.
- On yedinci Stage B paketi `9d75573a86e395d2fa74f3808d43310e4d65f760` commit'iyle görünür teslimat kabulünü aynı serialized item için Receiving→ActorHands→RAF A Shelf/WorldFloor authoritative container zincirine bağladı. Domain-first mutation, rollback ve recovery kanıtlandı; Edit Mode 188/188, Play Mode 17/17, Universal macOS ve Apple M4/Metal `stock-flow=ok` geçti. Sırada fiziksel koli açma/manifest birim projection'ı vardır.
- On sekizinci Stage B paketi `3766f3f06df624093f4774ef8fa4e7f1286d1c01` commit'iyle dış teslimat kolisini exact accepted manifestten ayrılan idempotent reveal adımına dönüştürdü. Kapalı parcel→ürün+açık kabuk görünür; opening domain revision/quantity değiştirmez, invalid state/binding/location no-reveal kalır. Edit Mode 192/192, Play Mode 17/17, Universal macOS ve Apple M4/Metal `parcel-open=ok` geçti. Sırada authoritative shelf offer/fiyat etiketi vardır.
- On dokuzuncu Stage B paketi `7a23cd92be6ff1169ff49530319b0759965cadf5` commit'iyle Unity bağımsız PSE.Retail shelf-offer authority ve RAF A fiyat etiketi projection'ını ekledi. Stable offer/product/shelf, integer minor-unit fiyat, idempotent set/update ve failure no-mutation kanıtlandı; gerçek keyboard/gamepad publish Inventory/Orders state'ini değiştirmedi. Edit Mode 207/207, Play Mode 17/17, Universal macOS ve Apple M4/Metal `shelf-offer=ok price-minor=54999 currency=EUR` geçti. Sırada customer basket/serialized reservation sınırı vardır.
- Yirminci Stage B paketi `45c2cdc4f4f437824567c7e7cb5b6fcea1ecb4ce` commit'iyle stable customer/basket/line kimliklerini exact shelf offer, serialized item ve Inventory reservation'a bağladı. Exact reserve idempotent, duplicate/mismatch/drift failure cross-authority no-mutation; release availability'yi geri getirir. Gerçek keyboard/gamepad `G / East` reserve/release ve reserved `E / South` pickup fail-closed çalışır. Edit Mode 220/220, Play Mode 17/17, Universal macOS ve Apple M4/Metal `basket-reservation=ok release=ok` geçti. Sırada immutable checkout price snapshot sınırı vardır.
- Yirmi birinci Stage B paketi `294999f6ad48d4831f56031cc542cf43cac09d3e` commit'iyle stable checkout identity ve deterministic immutable line snapshot'larını ekledi. Begin bütün basket satırlarında exact offer/item/reservation preflight yapar; integer minor-unit currency/total'i dondurur, yalnız Checkout revision'ını ilerletir ve sonraki offer price update'inden etkilenmez. Gerçek keyboard/mouse + gamepad checkout başlangıcı ve aktif release/pickup kilidi çalışır. Edit Mode 233/233, Play Mode 17/17, Universal macOS ve Apple M4/Metal `checkout-snapshot=ok price-frozen=ok` geçti. Sırada reservation consume + sold/fulfilled commit sınırı vardır.
- Yirmi ikinci Stage B paketi `bb89b0c297400f6eed22407df76dc1c85912cd74` commit'iyle atomik multi-reservation consume, stable checkout completion ve tamamlanmış historical snapshot invariantını ekledi. Inventory/Basket/Checkout başarıda birer revision; ShelfOffer/Orders sıfır mutation üretir. Exact repeat idempotent, conflict/time/drift failure no-mutation'dır. İkinci Mouse Left/Gamepad RT ürünü raftan kaldırır; stok/sepet/reservation 0, HUD `TAMAMLANDI` olur. Edit Mode 242/242, Play Mode 17/17, Universal macOS ve Apple M4/Metal `sale-completion=ok stock-consumed=ok completed-quantity=0` geçti. Sırada Issue #9 müşteri intent/state alt işi vardır.
