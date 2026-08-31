# PC Shop Empire 3D – Aşamalı Geliştirme Yol Haritası

**Belge durumu:** Ön üretim yol haritası 0.1  
**Tarih:** 10 Ağustos 2026  
**Son bütünlük QA güncellemesi:** 31 Ağustos 2026<br>
**Planlama ufku:** Araştırma → teknik prototip → vertical slice → üretim → test → Steam/Windows 1.0 → sonradan macOS.

## Yönetici özeti

Bu proje büyük ölçeklidir. En güvenli üretim sırası, bütün özellikleri paralel başlatmak değil, tek bir küçük garajda şu kanıt zincirini tamamlamaktır:

> Dashboard siparişi → fiziksel teslimat → stok → raf/satış veya özel PC işi → montaj → test → paketleme → teslim → muhasebe → kayıt ve hata raporu.

Bu zincir eğlenceli, anlaşılır, performanslı ve kayıt güvenli olmadan çalışan, şube, yüzlerce ürün veya servis merkezi eklenmeyecek.

### Güncel üretim checkpoint'i

Issue #115 Retail Shelf Authority Consolidation r57 technical head `96d72d5202cdb72b1c017ce5e063948c892ce88d` üzerinde Mac-ready durumdadır. Legacy `StarterShelf` hiyerarşisi kaldırıldı; exact tek `AuthoritativeRetailShelfA`, `5` collider, tek placement surface ve tek inventory shelf zone kaldı. Authored renderer `499→483`; retail runtime `486/462`, Assembly regression `477/468`; accepted r56/r55 readability ve customer approach→browse→checkout→fulfilled-exit authority zinciri korunur.

Mac scene `11/11`, targeted PlayMode `1/1 + 1/1`, keyboard/mouse + virtual-gamepad flows `2/2`, full `754/754 EditMode + 158/158 PlayMode`, universal Mac build ve iki Apple M1/Metal native smoke geçti. Draft PR #118 açıktır; Issue/Roadmap `In Progress`, parent Epic #18 ve Steam 1.0 Goal açıktır. Fiziksel Windows olmadığından exact-head clean x64 IL2CPP/D3D11/Iris Xe gate; USB olmadığından immutable checkpoint/readback ertelenmiştir. UTM fiziksel release kanıtının yerine geçmez. Bu kapılar ve CI/PR entegrasyonu tamamlanmadan Issue #115 kapatılmaz; Mac tek authoritative write lane'de sonraki bounded ürün işini hazırlamayı sürdürebilir.

Geçici gerçekçi takvim:

- İlk graybox oynanabilir: **2–4 ay**.
- Teknik prototip: **4–7 ay**.
- Halka açık olmayan kaliteli vertical slice: başlangıçtan **6–12 ay**.
- İçerikçe anlamlı alpha: **18–30 ay**.
- Windows/Steam 1.0: solo çekirdek üretim ve kontrollü dış destekle yaklaşık **30–54+ ay**.
- macOS: Windows 1.0 ve bütçe sonrasında ayrı **3–8 aylık** port/QA penceresi.

Bu süreler söz değil, risk aralığıdır. Haftalık çalışma saati, özgün 3D sanat yükü, Windows test erişimi, kullanıcı testleri ve dış destek sonucu ciddi değişebilir. Codex planlama, kodlama, test otomasyonu, teknik yazım ve hata analizini hızlandırabilir; fakat insan oynanış testi, sanat yönü onayı, hukuki/yayın kararları ve gerçek Windows donanımı ortadan kalkmaz.

## 1. Planlama varsayımları

Takvim aşağıdaki varsayımlarla hazırlanmıştır:

- Bir ana geliştirici/yapımcı ve Codex'in sürekli teknik/proje desteği.
- Düzenli fakat tam zamanlı olması garanti edilmeyen üretim.
- İlk bütçe €0; yalnız kanıtlanmış yüksek etkide cüzi harcama.
- Kurgusal marka ve özgün/uygun lisanslı varlıklar.
- Windows/Steam birincil, macOS sonradan.
- V1 tek oyunculu; co-op yok.
- Başlangıç tabanı Unity 6.3 LTS + URP + C#; Aralık 2027 destek sonu nedeniyle alpha öncesi kontrollü LTS yükseltme zorunlu.
- Gerçek Windows x64 PC bugün doğrulanmış ekipman değil; ilk oynanabilirden önce karşılanacak dış bağımlılık.
- Vertical slice tek lokasyon ve küçük katalog.
- Kalite kapıları takvim baskısıyla atlanmayacak.

Varsayım değişirse süre tahmini yeniden tabanlanır; eski tarih “söz verilmiş teslim” gibi korunmaz.

## 2. Zorluk ölçeği

| Kod | Anlam | Tipik risk |
|---|---|---|
| S | Küçük, yerel ve iyi anlaşılmış | Günler–birkaç hafta; az bağımlılık |
| M | Orta, birden çok sistem | Birkaç hafta; entegrasyon testi gerekir |
| L | Büyük, içerik ve sistem birlikte | Aylar; geri dönüş ve performans riski |
| XL | Çok büyük/oyunun kimliği | Birden çok milestone; geniş test matrisi |

Zorluk “kaç satır kod” değildir. 3D sanat, animasyon, UX, içerik, test kombinasyonu ve hata etkisini birlikte ifade eder.

## 3. Faz özeti

| Faz | Sonuç | Öncelik | Bağımlılık | Zorluk | Geçici süre |
|---|---|---|---|---|---:|
| 0 | Araştırma, proje hafızası ve güvenlik kapısı | P0 | Yok | M | Tamamlanmaya yakın |
| 1 | Proje temeli ve graybox etkileşim | P0 | Faz 0 onayı | L | 6–10 hafta |
| 2 | Temel mağaza teknik prototipi | P0 | Faz 1 | XL | 8–14 hafta |
| 3 | PC toplama teknik prototipi | P0 | Faz 1–2 stok/işlem | XL | 10–18 hafta; kısmen paralel |
| 4 | Vertical slice kalite geçişi | P0 | Faz 2–3 | XL | 12–24 hafta |
| 5 | Temel perakende üretimi | P0 | Slice kabulü | XL | 4–7 ay |
| 6 | PC montaj, servis ve ikinci el üretimi | P0 | Faz 4 go kararı + katalog pipeline | XL | 5–9 ay |
| 7 | Çalışanlar, müşteri ölçeği ve otomasyon | P1 | Faz 5–6'da oyuncunun tamamlayabildiği stabil işler ve stok rezervasyonu | XL | 4–8 ay |
| 8 | Ekonomi, ilerleme, lokasyon ve kanallar | P1 | Faz 6 servis/ikinci el çekirdeği + Faz 7 çalışan kapasitesi | XL | 5–9 ay |
| 9 | İçerik, sanat, ses, hikâye, yerelleştirme | P1 | Pipeline'lar | XL | 8–14 ay; üretim boyunca |
| 10 | Alpha: denge, optimizasyon, erişilebilirlik | P0 | Feature complete çekirdek | XL | 4–7 ay |
| 11 | Demo/Steam Playtest ve geri bildirim | P0 | Stabil alpha adası | L | 2–4 ay hazırlık + test |
| 12 | Beta, yayın hazırlığı ve Windows 1.0 | P0 | Alpha/Playtest | XL | 4–8 ay |
| 13 | macOS portu ve ayrı QA | P2 | Windows 1.0 + bütçe | L–XL | 3–8 ay |

Fazlar tamamen ardışık değildir. Sanat, ses, test ve dokümantasyon üretim boyunca akar. Ancak bağımlılığı tamamlanmayan büyük özellik “paralellik” gerekçesiyle başlatılmaz.

## 4. Faz 0 — Keşif, ortak anlayış ve kaynak güvenliği

**Durum:** Tamamlandı; ortak anlayış onaylandı, canonical legacy kaynak ve ayrı Unity milestone snapshot'ı hash düzeyinde doğrulandı.  
**Öncelik:** P0  
**Zorluk:** M

### Amaç

Eski oyunu bozmadan neyin var olduğunu, yeni oyunun ne olacağını ve hangi kararların henüz verilmediğini yazılı hâle getirmek.

### Çıktılar

- Eski Electron projesinin salt-okunur teknik ve Dashboard haritası.
- Korunacak/dönüştürülecek/yeniden tasarlanacak/eklenecek/çıkarılacak matrisi.
- Rakip ve gerçek işleyiş araştırması.
- Game Design Bible ve yaşayan proje hafızası.
- Teknik mimari/Guardian sınırları.
- Araç, lisans ve platform ön kararı.
- İlk risk kayıt defteri.

### Kapanış kayıtları

- USB kaynak klasörü hash karşılaştırması tamamlandı: 26/26 eşleşme, 11 Ağustos 2026.
- Canonical legacy snapshot belirlendi: USB `KAYNAK_KODU`; yerel inceleme kopyası doğrulanmış aynası.
- Üç kopyalı yedek planının hedeflerini netleştirme.
- Bu 0.1 belge paketi 11 Ağustos 2026'da ortak anlayış olarak onaylandı.

### Riskler

- USB ile yerel kopyanın ayrışması bu snapshot için kapandı; gelecekteki her değişiklikte manifest yeniden doğrulanmalı.
- Final oyun adının mevcut markalarla çakışması.
- Eski gerçek marka/verilerin doğrudan kullanılabilir sanılması.

### Geçiş ölçütü

- Kullanıcı tasarım yönünü onaylar.
- Eski kaynak “referans, port değil” olarak kilitlenir.
- Kurulum için sürüm/disk/lisans/yedek listesi ayrıca onaylanır.

## 5. Faz 1 — Proje temeli ve graybox etkileşim

**Durum:** Temel mağaza/müşteri/kasa zinciri tamamlandı; parent Epic #9 kapalı/Done. Epic #10'un motherboard seating ve deterministic fastener secure/unsecure dilimleri Issue #53–#54 ile kaynak/test/build/runtime doğrulamasından geçti; Issue #54 kapalı/Done, yalnız USB checkpointi ertelendi.<br>
**Öncelik:** P0  
**Bağımlılık:** Faz 0 onayı ve kurulum kapısı  
**Zorluk:** L  
**Geçici süre:** 6–10 hafta

### Amaç

Boş Unity projesinden, Windows'ta da açılan; yürüyüş, eller, etkileşim ve tek test odası bulunan teknik omurgaya ulaşmak.

### İş paketleri

1. Unity sürümünü kilitleme, URP ve assembly sınırları.
2. Sürüm kontrolü, ignore/lock kuralları ve otomatik yedek.
3. C# stil, klasör, kimlik, olay ve test standartları.
4. Birinci şahıs hareket: yürüme, çömelme gereksinimi kararı, FOV, hassasiyet.
5. Klavye/fare ve gamepad action map'i; rebind temeli.
6. Görünür el prototipi, alma/bırakma/inceleme.
7. Hibrit taşıma: küçük kutu, büyük kutu ve taşıma arabası graybox'ı.
8. Etkileşim hedefi, outline/ikon/metin ve erişilebilir hedef büyütme.
9. Tek odalı performans referans sahnesi.
10. macOS editöründeki Windows Build Support (Mono) ile yalnız erken taşınabilirlik/smoke build'i; ilk oynanabilirden önce gerçek Windows x64 makinede Windows Unity Editor + gerekli C++ Build Tools/Windows SDK ile IL2CPP baseline build'i ve temiz makinede açılış. Mac'ten alınan Mono çıktısı DirectX, Windows IL2CPP, Steam veya native eklenti kanıtı sayılmaz.

**Güncel teknik kanıt:** `PSE.Core`, `PSE.Catalog`, `PSE.Inventory`, `PSE.Orders`, `PSE.Retail`, `PSE.Actors`, `PSE.Economy` ve `PSE.Assembly` Unity bağımsız domain sınırlarını korur. Order→stock→offer→customer→checkout→exact-cash zincirine ek olarak tek serialized `MicroAtx` anakart existing Inventory'den managed Workbench'e atomik attach/detach edilir ve Assembly-owned captive fastener ile `SeatedUnsecured ↔ SeatedSecured` geçer. Garage'da açık kasa/keyed seat, deterministic seat + fastener solverları, stable physical identity, secured detach gate'i, historical replay ve recovery çalışır. Issue #54 feature `b681239`; EditMode 411/411, PlayMode 29/29, Universal macOS 328.057.977 bayt ve Apple M4/Metal 1280×720 `garage-motherboard-fastener-r23-v1 assembly-flow=ok ... detach-authority-blocked=ok ... recovery=ok` geçti. Sıradaki bounded gameplay yalnız CPU socket seating + retention lever dilimidir. RAM/GPU/cooler, tam build/benchmark, Save/Guardian, çoklu müşteri/ürün ve final art ayrı kalır.

### Kapsam dışı

- Güzel final mağaza sanatı.
- Tam müşteri AI.
- Tam ekonomi; mevcut kapsam yalnız bounded exact-cash satış settlement'ı ve ilk ledger deltalarıdır.
- Tam PC montajı; mevcut kapsam motherboard seating + tek captive-fastener secure/unsecure dilimidir.
- Steam entegrasyonu.

### Ana riskler

- Eller ile fizik nesnesinin titremesi/çakışması.
- Kontrolcünün sonradan eklenmiş hissettirmesi.
- Asset/package seçiminin temel mimariyi kilitlemesi.
- Mac'te çalışan input/build'in Windows'ta ayrışması.
- Windows x64 test cihazı erişiminin takvime yetişmemesi.

### Doğrulama ölçütleri

- Yeni kullanıcı 3 dakika içinde yardım almadan kutuyu alıp işaretli yüzeye bırakır.
- Fare ve gamepad ile tüm temel eylemler yapılır.
- Kamera rahatsızlığı seçenekleri çalışır.
- Fizik nesnesi dünya dışına düşse bile kimliği kaybolmaz.
- Mac'ten üretilen Windows Mono smoke build'i gerçek Windows x64 makinede açılır ve aynı graybox akışı tamamlanır.
- Aynı commit'in Windows-hosted IL2CPP baseline build'i alınır; temiz Windows kurulumunda açılış, input, save ve temel etkileşim doğrulanır.
- Temel testler temiz makinede tek komut/işlemle çalıştırılabilir.

### Durdur/iyileştir kapısı

Taşıma ve etkileşim keyifsizse mağaza sistemi eklenmez. Önce el hissi, hedefleme ve fizik stabilitesi çözülür.

## 6. Faz 2 — Temel mağaza teknik prototipi

**Öncelik:** P0  
**Bağımlılık:** Faz 1 etkileşim ve alan çekirdeği  
**Zorluk:** XL  
**Geçici süre:** 8–14 hafta

### Amaç

Sanat kalitesi aramadan tek ürünün siparişten kasaya kadar aynı kimlikle yaşayabildiğini kanıtlamak.

### İş paketleri

- Küçük garaj graybox'ı: teslim önü, depo, satış alanı, kasa, ofis.
- Ürün tanımı, item instance, batch ve konteyner modeli.
- Dashboard satın alma siparişi.
- Tedarik süresi ve fiziksel teslimat spawn/manifest'i.
- Sayım, hasar kontrolü, kabul/ret/claim.
- Kutu açma ve depo/raf transferi.
- Raf bölmesi, kapasite, fiyat etiketi ve facing.
- Serbest + snap mağaza yerleşimi ve nav yaklaşım kontrolü.
- Basit müşteri: giriş, ürün bulma, sepete alma, sıra ve ödeme.
- Kasa transaction'ı, fiş, stok ve ledger.
- Gün açılışı/kapanışı ve temel gider.
- Dashboard stok/fiyat/finans view model'i.
- Oyuncuya açık faaliyet/işlem/kurtarma günlüğü; ham Guardian tanısı ayrı kalır.
- Atomik save/load, birden çok manuel yuva, döner autosave, kritik işlem checkpoint'i ve yedek ilk sürümü (**D-112 ilk uygulama sahibi**).
- Guardian stok/satış/save invariant'ları.

### Riskler

- Dünya objesi ile stok defterinin ayrışması.
- Fiziksel raf doldurmanın birkaç üründen sonra angaryaya dönüşmesi.
- Serbest yerleşimin rota oluşturmayı bozması.
- Kasa sırasında fiyat değiştirme veya çift satış exploit'i.
- Oyun kapanırken hareketli nesnelerin kayıt kaybı.

### Doğrulama ölçütleri

- Aynı seri kimliği sipariş manifestinden raf ve fişe kadar izlenir.
- Teslimat yarıda kesilip kayıt yüklenince ürün çoğalmaz/kaybolmaz.
- Manuel yuva, autosave ve büyük alım öncesi checkpoint birbirini sessizce ezmez; doğru metadata ile ayrı yüklenir.
- 100 otomatik satışta nakit ve stok defteri tam uzlaşır.
- Müşteri yerleşmiş raf ve kasaya ulaşır; ulaşamıyorsa placement bunu önceden açıklar.
- İlk kez oynayan tester 15 dakika içinde ürün sipariş edip satar.
- Tekrar doldurma üçüncü kez yapıldığında ustalık/kısayol ihtiyacı ölçülür.

### Geçiş kapısı

P0/P1 stok, save ve transaction hataları sıfır; en az iki Windows donanımında akış tamamlanır.

## 7. Faz 3 — PC toplama teknik prototipi

**Öncelik:** P0  
**Bağımlılık:** Faz 1 etkileşim; Faz 2 kimlik, stok, sipariş ve save  
**Zorluk:** XL  
**Geçici süre:** 10–18 hafta; Faz 2'nin son bölümüyle kontrollü paralel

### Amaç

Bir özel PC işinin tekliften teslimata fiziksel, açıklanabilir ve teknik olarak doğru çalıştığını kanıtlamak.

### Slice katalog sınırı

- 2 CPU soket ailesi.
- 2 anakart form sınıfı.
- 2 kasa boyutu.
- Sınırlı RAM, GPU, PSU, depolama ve soğutucu ailesi.
- 50–80 toplam mağaza/PC SKU'su içinde anlamlı alt küme.

### İş paketleri

1. Müşteri ihtiyacı ve yazılı kabul snapshot'ı.
2. Build graph ve uyumluluk kural motoru.
3. Solvability proof ile iş üretimi.
4. İş emrine item rezervasyonu ve parça kiti.
5. Kasa açma, anakart/CPU/RAM/depolama/PSU/GPU/soğutucu montajı.
6. Vida, kablo ve termal macun için yönlendirilmiş fizik.
7. Hata/risk/kalite dereceleri.
8. Kurgusal firmware/OS kurulumu.
9. Boot, stabilite, termal, güç, gürültü ve kullanım benchmark'ı.
10. Kablo yönetimi/temizlik, paketleme ve teslim.
11. Garanti kaydı ve müşteri kabulü.
12. Guardian build graph ve iş zinciri invariant'ları.

### Ana riskler

- Kasa–parça geometrisinin kombinasyon patlaması.
- Tutorial'ın tek adım sırasına kilitlenip softlock olması.
- Vida/kablo hareketlerinin çabuk yorucu olması.
- “Uyumsuz” sonucunun nedenini açıklayamamak.
- Otomatik iş üreticisinin çözülemez iş oluşturması.
- Testin bekleme çubuğuna dönüşmesi.

### Doğrulama ölçütleri

- Üretilen her iş için en az bir geçerli katalog çözümü kaydedilir.
- Geçerli ve geçersiz kombinasyonların beklenen gerekçesi testte sabittir.
- Oyuncu adımları güvenli farklı sırayla yapabilir; öğretici bunu kabul eder.
- Her bloklayıcı uyumsuzluk neden + etkilenen parça + düzeltme gösterir.
- İlk PC 20–35 dakikada öğreticiyle tamamlanır; ustalaşmış tekrar belirgin kısalır.
- Save/load montajın her güvenli ara aşamasında çalışır.
- Benchmark sonucu müşteri kullanım amacıyla ilişkilidir.
- Parça kaybı, çoğalma veya iş rezervasyonu sızıntısı yoktur.

### Geçiş kapısı

İlk kez oynayan en az beş testerın çoğu harici yardım almadan bir PC işini bitirir ve başarısızlığının nedenini doğru anlatabilir.

## 8. Faz 4 — Vertical slice

**Öncelik:** P0  
**Bağımlılık:** Faz 2 ve 3 kabulü  
**Zorluk:** XL  
**Geçici süre:** 12–24 hafta  
**Toplam başlangıçtan hedef:** 6–12 ay

### Vertical slice'ın amacı

“Oyunun bütün özellikleri var” demek değil; final kalite hedefini küçük bir dilimde kanıtlamak.

### Dahil olanlar

- Tek, cilalı garaj ve teslimat önü.
- Birinci şahıs karakter, görünür eller ve erişilebilir input.
- Küçük kurgusal ürün kataloğu.
- Dashboard sipariş/stok/fiyat/finans ve tek özel PC talebi.
- Teslimat, kabul, kutu, depo, raf, fiyat ve kasa.
- Birkaç müşteri segmenti ve bir tekrar eden karakter.
- Baştan sona tek özel PC işi.
- Temel gün döngüsü, gider, itibar ve ilerleme sinyali.
- Sağlam save/recovery; oyuncuya kapalı geliştirici Guardian neden raporu ve yalnız gerekirse genel, Guardian adı taşımayan kurtarma bildirimi.
- TR ve EN vertical slice metni.
- Windows performans ve temiz kurulum.

### Açıkça dahil olmayanlar

- İşe alınabilir çalışan sistemi.
- Birden çok mağaza/şube.
- Tam servis/refurbish döngüsü.
- Online satış ve kurumsal anlaşmalar.
- Yüzlerce ürün.
- Gelişmiş rakip simülasyonu.
- Açık dünya araç kullanma.
- Co-op.

### Kalite çalışmaları

- Hero el ve montaj animasyonu ilk kalite geçişi.
- Garaj aydınlatması, okunabilir materyal ve ses kimliği.
- UI/Dashboard ortak görsel dili.
- Öğretici, bağlamsal yardım ve hata açıklaması.
- İki dilde metin taşması.
- Gamepad-only tamamlanabilirlik.
- Windows frame-time, bellek ve save spike profili.

### Riskler ve karşı önlemler

- **Slice'ın tam oyunu temsil edememesi:** Tek garajın iki çekirdek sütununu ve güven ekonomisini birlikte kanıtlama ölçütü kullanılır; şube/çalışan varmış gibi pazarlanmaz.
- **Cila uğruna kapsam büyümesi:** Dahil/dahil değil listesi kilitlenir; yeni fikirler sonraki faz backlog'una gider.
- **Az tester ile yanlış go kararı:** Oturum davranışı, görüşme ve hata verisi birlikte okunur; tek olumlu/olumsuz yorumla yön değiştirilmez.
- **Hero art'ın sistemi gölgelemesi:** Önce akış, save ve performans kabulü; final kalite art yalnız stabil etkileşime uygulanır.

### Başarı ölçütleri

- 30–60 dakikalık slice'ta katılımcıların ≥%80'i ana zinciri bloklanmadan tamamlar.
- Katılımcıların ≥%70'i “bu oyunun farkı nedir?” sorusuna teknoloji perakendesi + gerçek PC işi/güven ekseninde cevap verir.
- İlk satış ve PC tesliminin neden-sonucu anlaşılır.
- Tekrar edilen fiziksel işte “angarya” puanı kabul edilen eşik altında kalır; veri ile kısayol planı çıkar.
- P0/P1 açık hata yok.
- Zorla kapatma ve bozuk dosya testinde kayıt kaybı yok.
- Hedef Windows sınıfında ölçeklenebilir performans hipotezi doğrulanır veya kapsam/kalite ayarı revize edilir.
- 2 saatlik oturumda kayıp item, çift işlem ve takılı kalıcı NPC işi yok.
- Guardian'ın ham invariant, neden zinciri ve geliştirici raporu oyuncu UI/Dashboard'unda görünmez; maddi bir recovery yalnız genel açıklama ve olay kimliğiyle bildirilir.

### Go / pivot / stop kararı

- **Go:** Ana zincir eğlenceli, farklı ve teknik olarak güvenilir.
- **Pivot:** İnsanlar montajı seviyor fakat perakendeyi ya da tersini sevmiyorsa ağırlık ve akış düzeltilir.
- **Stop/scope reset:** Temel zincir birkaç iterasyonda anlaşılır/eğlenceli hâle gelmiyor veya hedef donanımda sürdürülemiyorsa büyük üretime geçilmez.

## 9. Faz 5 — Temel perakende üretimi

**Öncelik:** P0  
**Bağımlılık:** Vertical slice go kararı  
**Zorluk:** XL  
**Geçici süre:** 4–7 ay

### Sistem genişletmeleri

- Ürün kategorileri: parçalar, monitör, giriş aygıtı, ses, webcam, kontrolcü, kablo, sarf.
- Kategoriye özgü raf/vitrin/güvenlik davranışı.
- Tedarikçi fiyat, MOQ, vade, güvenilirlik ve garanti farkları.
- Kutu, koli ve palet teslimat katmanları.
- Depo, rezerv ve raf replenishment politikaları.
- Kasa; nakit/kart soyutlaması, iade ve fiş.
- Fiyat önerisi ve kategori kuralı; oyuncu onayı.
- Kuyruk, yoğunluk, hafta sonu ve çalışma saati.
- Hırsızlık/güvenlik çok seyrek, okunabilir ve karşı önlemli.
- İlk mağaza iç yerleşim/dekorasyon seti.
- Raf, taşıma arabası, kasa, güvenlik ve depo ekipmanı gibi dünyada görülen fiziksel upgrade/renovasyon akışı.

### Riskler

- SKU sayısının karar derinliğinden hızlı büyümesi.
- Raf doldurma ve kutu açmanın içerik yerine grind olması.
- NPC yoğunluğu ve pathfinding maliyeti.
- Fiyat sisteminin tek optimal yüzdeye indirgenmesi.

### Doğrulama ölçütleri

- Her yeni kategori farklı bir seçim veya operasyon baskısı yaratır.
- 1000 seed'li satışta para/stok uzlaşır.
- 20+ müşteri senaryosunda rota/queue tamamlanır.
- Oyuncu büyüyünce toplu etiketleme, parça kiti ve çalışan desteğine açık geçiş görür.
- Fiyat kararında hız, güven, stok ve rakip nedeniyle en az birkaç uygulanabilir strateji vardır.

## 10. Faz 6 — PC montaj, servis ve ikinci el üretimi

**Öncelik:** P0  
**Bağımlılık:** Faz 4 go kararı, slice montaj çekirdeği ve katalog pipeline  
**Zorluk:** XL  
**Geçici süre:** 5–9 ay

### Montaj genişlemesi

- Katalog 300–500 tam oyun geçici hedefe doğru kontrollü büyür.
- Daha fazla soket/form/termal/gürültü/estetik kararı.
- BIOS/firmware, fan profili ve ileri testler.
- İş kullanım profilleri: öğrenci, ofis, oyun, yayın, tasarım, geliştirme, kurumsal.
- Değişiklik talebi, depozito, deadline ve teslim kabulü.
- Ustalık kısayolları, kaliteli araçlar ve şablonlar.

### Servis zinciri

- Intake ve cihaz/kondisyon kaydı.
- Veri erişim/yedekleme/silme izni.
- Belirti → hipotez → test → known-good swap → kanıt.
- Teklif, müşteri onayı, parça siparişi ve revizyon.
- Onarım, temizlik, burn-in ve teslim.
- Garanti tekrar gelişi, RMA ve tedarikçi claim.

### İkinci el/refurbish

- Trade-in ve sahiplik kanıtı.
- Kondisyon/grade ve gizli risk aralığı.
- Güvenli veri silme.
- Onarım/temizlik/test.
- Yeni seri kayıt, sınırlı garanti ve doğru açıklama.

### Riskler

- Gerçekçilik katmanlarının yeni oyuncuyu boğması.
- Teşhisin rastgele “doğru parçayı bul” oyununa dönüşmesi.
- Katalog kombinasyon test maliyetinin aşırı büyümesi.
- İkinci el riskinin oyuncuya haksız görünmesi.

### Doğrulama ölçütleri

- İleri kural açılmadan temel iş anlaşılır ve tamamlanabilir.
- Teşhis sonucu kanıt zinciriyle açıklanır; magic one-click yoktur.
- Her servis işinde en az bir çözülebilir yol ve maliyet sınırı vardır.
- İkinci el ilanı görünür bilgi ile gerçek risk arasında adil aralık taşır.
- Tekrarlanan temiz işlerde süre ustalık/araçlarla azalır.
- Tüm yeni katalog aileleri otomatik ve geometrik fixture testini geçer.

## 11. Faz 7 — Çalışanlar, müşteri ölçeği ve otomasyon

**Öncelik:** P1  
**Bağımlılık:** Faz 5 perakende ile Faz 6 montaj/servis/ikinci el işlerinin oyuncu tarafından stabil tamamlanabilmesi; stok rezervasyonu ve iş istasyonları  
**Zorluk:** XL  
**Geçici süre:** 4–8 ay

### Neden daha sonra?

Çalışan yalnız oyuncunun yapabildiği, iyi tanımlanmış bir işi devralabilir. Temel iş henüz stabil değilken çalışan AI eklemek hataları çoğaltır ve nedenini gizler.

### İş paketleri

- İşe alım adayı: hız, uzmanlık, dikkat, güven, maaş ve tercih.
- Roller: satış, kasa, teknisyen, depo, temizlik; yönetim/güvenlik sonra.
- Vardiya, mola, bölge ve öncelik politikası.
- Görev kuyruğu, kaynak/istasyon rezervasyonu ve kritik engelde yardım.
- Mağaza ölçeği gerektirdiğinde self-checkout; yaş/kategori/istisna ve hata için çalışan gözetimi, oyuncu müdahalesi ve açık işlem izi (**D-046 sahibi**).
- Gözetimli öğrenme, mentorluk ve iş deneyimi.
- Moral/yorgunluk; şeffaf ve sömürücü olmayan sonuçlar.
- Yakın NPC tam sunum, uzakta simulation LOD.
- Tekrar eden müşteri hafızası ve sadakat.
- Randevu, servis kapasitesi ve müşteri kabul sınırı.
- Mahalle ekolojisi: yalnız müşteri olmayan sakin, yaya, kurye, sürücü, bakım görevlisi ve ziyaretçiler; her rolün zaman çizelgesi, hedefi, erişebildiği bilgi, hafızası ve ayrılmış fiziksel kaynağı vardır.
- Seed'li ve yeniden oynatılabilir dünya olayları: teslimat gecikmesi, yol/park yoğunluğu, hava etkisi, mahalle etkinliği ve tedarik hareketi; hiçbir olay stok veya para authority'sini atlamaz.
- Yaya, müşteri, çalışan, kurye ve araç rotaları aynı kapı/koridor/park/yükleme alanını rezervasyon ve timeout ile paylaşır; sıkışma ürünü yok etmez ve işi sessizce tamamlamaz.

### Riskler

- Çalışanların ürünü kaybetmesi, takılması veya ekonomiyi bozması.
- Otomasyonun oyuncunun tüm amacını alması.
- Mikro yönetimin daha da büyümesi.
- Aynı anda perakende ve atölye yükünün haksız itibar kaybı üretmesi.

### Doğrulama ölçütleri

- 8 saat hızlandırılmış mağaza soak testinde kayıp/çift item yok.
- Engelli görev timeout olur, ürün korunur ve neden raporlanır.
- Oyuncu politika belirleyip her işe tek tek tıklamadan mağazayı çalıştırabilir.
- Oyuncu istediği fiziksel işe geri müdahale edebilir.
- Self-checkout rutini azaltır; ödeme/istisna hatasını gizlemez ve kasa transaction'ıyla aynı ledger kurallarını kullanır.
- Çalışan eklemek yalnız gelir değil, yeni kapasite/strateji getirir.
- Atölye randevusu sırasında mağaza kapsaması oyuncunun kontrolündedir.
- Aynı seed ve başlangıç durumunda yakın tam simülasyon ile uzak simulation LOD ekonomik sonucu değiştirmez; yalnız sunum ayrıntısı ve hesaplama sıklığı değişir.
- Mahalle NPC soak testinde rota kilidi, kalıcı kuyruk, aynı kaynağa çift sahiplik, kayıp ürün ve açıklamasız müşteri davranışı yoktur.

## 12. Faz 8 — Ekonomi, ilerleme, lokasyon ve gelir kanalları

**Öncelik:** P1  
**Bağımlılık:** Faz 6 servis/ikinci el çekirdeği ve Faz 7 çalışan kapasitesi; stabil satış, stok ve işlem sistemi  
**Zorluk:** XL  
**Geçici süre:** 5–9 ay

### Ekonomi

- Nakit, gelir, COGS, brüt kâr, gider, borç ve vade.
- Dinamik ama sınırlı/açıklanabilir pazar dalgası.
- Nesil çıkışı, eski ürün değer kaybı ve niche talep.
- Tedarikçi ilişkisi, kampanya, gecikme, hasar ve kıtlık.
- Kurgusal vergi/kira/fatura ve forecast.
- Kademeli iflas ve yeniden yapılandırma.
- Marka/pazar terminali: kampanya, yorum, rakip etkisi ve tedarikçi ilişkisi.
- Business Intelligence: nedensel KPI, risk, nakit tahmini ve güven aralığı.

### İlerleme

- Garaj → mahalle mağazası → teknoloji mağazası → amiral mağaza.
- Her lokasyonda farklı müşteri, kira, operasyon ve hizmet beklentisi.
- Eski yeri depo, servis, online veya ikinci el birimine çevirme.
- Hafif simüle uzak lokasyon ve yönetici.
- Uzmanlık itibarı: fiyat, teknik kalite, hız, garanti ve güven.
- Büyük fiziksel upgrade, taşınma ve renovasyonda satın alma sonrası işletme sermayesi/ödeme riski önizlemesi.
- Bölüm hedefi ve kariyer kilometre taşları; nitelikli başarımların sistem olaylarına bağlanması.

### İşlevsel mahalle, kişisel ev ve araç/logistik katmanı

- Garaj ve mağazaların çevresinde yürüyerek gezilebilen kompakt bir mahalle bulunur. Binalar, dükkânlar, yollar, kaldırımlar, park/yükleme alanları ve dış dünya NPC'leri yalnız dekor değildir; mağaza trafiği, teslimat, işe gidiş, kira ve hizmet kapasitesine bağlanır.
- Her binanın içi açılmaz. Oyuncu kararına veya çekirdek döngüye hizmet eden ev, aktif mağaza/servis, tedarik/teslim noktası ve seçilmiş komşu işletmeler işlevsel girilebilir alanlardır; geri kalan kabuklar tutarlı dünya ve performans sınırıdır.
- Kişisel ev/garaj; dinlenme ve gün geçişi, manuel save erişimi, kişisel terminal, sınırlı depolama, eve teslim, araç parkı ve ilerlemeyle açılan işlevsel yükseltmeler sunar. Ev masrafları ve işe ulaşım ekonomiyle bağlantılıdır; ücretsiz ikinci depo veya teleport menüsü değildir.
- Oyuncu araçları; sürüş, park, anahtar/sahiplik, bagaj/kargo kapasitesi, yakıt/enerji, bakım/hasar, teslim alma-bırakma ve save/load kimliği taşır. Kurye/tedarik araçları aynı yükleme ve teslimat authority sözleşmesine uyar.
- Trafik ve yaya sistemi hız/çarpışma/erişim kurallarıyla okunabilir kalır. Araç veya NPC sıkışması ürün, sipariş veya para kaybettirmez; güvenli timeout/recovery ve açık neden kaydı üretir.
- Dış dünya kapsamı çekirdek mağaza ve atölye döngüsünü destekleyecek kadar genişler; boş kilometre, yalnız dosya boyutu büyüten dekor veya ana oyundan kopuk ayrı sürüş oyunu hedeflenmez.

### Mahalle ve araç doğrulaması

- Garaj → ev → mağaza → tedarik/teslim noktası rotaları editör teleportu olmadan yaya ve uygun araçla tamamlanır.
- Araç bagajı, teslimat alanı, oyuncu eli ve mağaza stok konteyneri arasında stable item kimliği korunur; kayıp/çift ürün yoktur.
- Park, yükleme ve bina girişleri yaya/araç erişilebilirlik fixture'larını geçer; save/reload sonrası araç, kargo, ev, mağaza ve görev durumu aynıdır.
- Düşük/orta/yüksek dünya yoğunluğu ayarları aynı economy/authority sonucunu verir; yalnız sunum yoğunluğu ve performans maliyeti değişir.

### Gelir kanalları

Sırayla, mevcut sistemleri yeniden kullanma ve ekonomik olarak ölçekleme oranına göre:

1. Online sipariş ve mağazadan teslim.
2. Faz 6'da kurulmuş tamir/servisin randevu, sözleşme ve kapasite kanalı olarak ölçeklenmesi.
3. Faz 6'da kurulmuş ikinci el/refurbish akışının trade-in, tedarik ve ayrı satış bölümü olarak ölçeklenmesi.
4. Küçük kurumsal/okul/ofis sözleşmesi.
5. E-spor/etkinlik sistemleri.
6. Uygunsa kiralama ve ayrı servis merkezi.

Yeni kanal ayrı bir mini oyun olmadan stok, fiyat, iş emri, teslim ve garanti gerçeğine bağlanır.

### Riskler

- Ekonominin grind veya kolay para exploit'ine dönüşmesi.
- Rakiplerin hile/rubber-band gibi algılanması.
- Yeni lokasyonların yalnız daha büyük oda olması.
- İflasın tek hatada kayıt kaybı yaratması.

### Doğrulama ölçütleri

- Seed'li binlerce kariyer simülasyonunda standart zorlukta birden çok yaşanabilir strateji vardır.
- Erken başarısızlık nedeni açıklanabilir ve kurtarma basamağı bulunur.
- İflas tek olayda değil, uyarılı süreçte oluşur; save silinmez.
- Her lokasyon en az iki yeni operasyon baskısı ve bir yeni fırsat getirir.
- Rekabet etkisi fiyat/tedarik/müşteri kanıtıyla görünürdür.
- Reload pazar sonucunu reroll etmez.

## 13. Faz 9 — İçerik, sanat, ses, hikâye ve yerelleştirme

**Öncelik:** P1; üretim boyunca  
**Bağımlılık:** Onaylı pipeline ve sistem sınırları  
**Zorluk:** XL  
**Geçici süre:** 8–14 ay eşzamanlı üretim

### İçerik ilkesi

İçerik sayısı değil, karar çeşitliliği hedeflenir. Yeni müşteri işi şu sorulardan en az birini değiştirmelidir:

- Hangi bilgi eksik?
- Hangi stok/tedarik riski var?
- Hangi teknik trade-off önemli?
- Hangi zaman/kapasite baskısı var?
- Güven ile kısa vadeli kâr nasıl çatışıyor?

### Sanat iş paketleri

- Kurgusal marka bible ve ürün aileleri.
- Modüler garaj/mağaza/depo/ofis/servis kitleri.
- Hero el/araç/montaj animasyonları.
- Sessiz kurucu karakterin isim, görünüm ve özellikle birinci şahısta görülen el/ten seçenekleri; tüm varyantlarda aynı rig, collider ve ekipman uyumu (**D-022 içerik sahibi**).
- PC parça geometrileri ve doğrulanmış snap noktaları.
- Raf, kutu, palet, araç ve dekor varyantları.
- Stilize ama inandırıcı müşteri/çalışan seti.
- LOD, collider, materyal ve performans geçişi.
- Mahalle ambiyansı, iç/dış mekân geçişi, trafik, yaya, mağaza, atölye ve araç seslerini kapsayan katmanlı spatial audio; ses olayları kaynak/mesafe/engel ve zaman durumuna göre okunabilir kalır.
- Gün/gece, hava ve mahalle yoğunluğu görsel/işitsel durum üretir; kritik etkileşim, rota, ürün rengi ve erişilebilirlik bilgisini gizlemez.
- Nihai kurulumun yaklaşık `15–20 GB` veya ölçülmüş ihtiyaçla daha büyük olması kabul edilebilir; bu bir hedef kota değil kalite tavanıdır. Her büyük asset ailede LOD, sıkıştırma, streaming/addressable sınırı, platform bütçesi ve lisans/provenans kaydı zorunludur; yapay dolgu yasaktır.

### Hikâye ve görev

- Tekrar eden müşteriler ve uzun vadeli sonuçlar.
- Çalışan ve tedarikçi karakter kırıntıları.
- Ahlaki ders vermeyen fakat güven seçimini gösteren durumlar.
- Hacim grind'ı yerine ustalık, kriz çözme ve farklı strateji başarımları.
- Ana şirket kaydını sıfırlamayan etkinlikler.

### Yerelleştirme

- TR ve EN ana dil şeması baştan.
- Metin anahtarı, değişken, çoğul ve ölçü birimi kontrolü.
- UI taşma ve font fallback testi.
- Makine taslağı kullanılsa bile çıkış metni insan kontrollü.
- Yeni diller ancak demo verisi ve bakım bütçesiyle.

### Riskler

- Asset kaynağı/lisans zincirinin kaybolması.
- Katalog sayısının regresyon testini geçmesi.
- Diyalog/hikâyenin üretim kapasitesini aşması.
- TR/EN'de UI ölçülerinin sonradan bozulması.

### Doğrulama ölçütleri

- Her final asset'in kaynak/lisans kaydı ve performans bütçesi var.
- Kurgusal marka gerçek markadan ayırt edilebilir ve kendi tutarlılığına sahip.
- Yeni içerik otomatik solvability/katalog testinden geçiyor.
- Her önemli müşteri sonucu neden zincirini gösteriyor.
- Her iki dilde bütün ana akış clipping olmadan tamamlanıyor.

## 14. Faz 10 — Alpha: denge, optimizasyon ve erişilebilirlik

**Öncelik:** P0  
**Bağımlılık:** Çekirdek feature complete ve içerik freeze pencereleri  
**Zorluk:** XL  
**Geçici süre:** 4–7 ay

### Alpha tanımı

Ana kariyer baştan sona oynanabilir; placeholder olabilir ama yeni temel sistem eklenmez. Çalışma hata düzeltme, performans, anlaşılabilirlik, denge ve erişilebilirliğe döner.

### İş paketleri

- 40–60 saatlik kariyer akışının ölçümü.
- Rahat/standart/uzman/hardcore ekonomi eğrileri.
- İflas ve yeniden başlama testleri.
- SKU/tedarik/pazar simülasyonu.
- NPC, fizik ve UI performans optimizasyonu.
- Save migration/fault injection matrisi.
- Alpha başlamadan desteklenen hedef Unity LTS'ye ayrı dalda yükseltme; paket/plugin/API, golden scene, save, performans ve gerçek Windows IL2CPP regresyon kapısı.
- Gamepad-only, rebind ve erişilebilirlik audit'i.
- Kurucu görünüm varyantlarının el animasyonu, kıyafet/araç clipping'i ve save uyumu.
- Tutorial idempotency/skip/reset testleri.
- Birden çok manuel yuva, döner autosave, kritik checkpoint ve eski şema recovery matrisi.
- TR/EN dil QA.
- Windows donanım matrisi ve uzun soak.
- Guardian false positive/negative ayarı ve privacy audit.

### Riskler

- “Bir özellik daha” ile alpha'nın hiç bitmemesi.
- Dengenin yalnız geliştirici oyun tarzına göre yapılması.
- Optimizasyonun içerik değişikliğiyle tekrar bozulması.
- Save schema'nın geç aşamada sık değişmesi.

### Doğrulama ölçütleri

- Ana kariyer farklı stratejilerle tamamlanabilir.
- Standart zorlukta başarısızlıkların çoğu geri bildirimle anlaşılır.
- Hedef donanım profilleri ve grafik preset'leri ölçülüdür.
- 20+ saatlik save ve migration döngüsünde ilerleme kaybı yok.
- Tüm eylemler yeniden atanabilir; temel oyun gamepad-only tamamlanabilir.
- P0 sıfır, P1 kontrollü kapanış eğrisinde.

## 15. Faz 11 — Demo, Steam Playtest ve topluluk doğrulaması

**Öncelik:** P0  
**Bağımlılık:** Stabil ve temsilî alpha adası  
**Zorluk:** L  
**Geçici süre:** 2–4 ay hazırlık; test pencereleri ayrıca

### Strateji

İlk dış test doğrudan açık Early Access çıkışı olmamalı. Sıra:

1. Küçük kapalı test.
2. Davetli Steam Playtest.
3. Düzeltme ve ikinci test dalgası.
4. Gerekirse cilalı demo.
5. Tam sürüm veya kanıt varsa dikkatli Early Access kararı.

Early Access varsayılan plan değildir. Kullanıcıların para verdiği build, “bir gün düzelir” diye temel save/performans/öğretici sorunlarıyla çıkmamalıdır.

### Ölçülecekler

- İlk satışa ve ilk PC teslimine süre.
- Tutorial terk/softlock oranı.
- Hangi fiziksel iş kaç tekrar sonra sıkıcılaşıyor?
- Hata nedenini anlama oranı.
- Oturum süresi, geri dönüş ve hedef kaybı.
- Save/cloud hata oranı.
- Donanım performansı ve crash-free sessions.
- En çok kullanılan otomasyon/erişilebilirlik ayarları.
- Açık uçlu “oyunun farkı” yanıtı.

### Veri ilkesi

- Telemetry opt-in, minimal ve amaçla sınırlı.
- Oyuncu yorumuyla telemetry aynı şey değildir.
- Küçük örneklemden ekonomi genellemesi yapılmaz.
- Yorumdaki en yüksek ses, tek tasarım otoritesi sayılmaz; davranış ve örüntüyle çaprazlanır.

### Riskler ve karşı önlemler

- **Örneklem yanlılığı:** Farklı PC bilgisi, sim tecrübesi, input cihazı ve donanım profilleri için dengeli davet grupları oluşturulur.
- **Telemetry'nin yanlış yorumlanması:** Sayısal olay, oturum gözlemi ve açık uçlu görüşme birlikte değerlendirilir; korelasyon neden sayılmaz.
- **Gizlilik/rıza hatası:** Telemetry varsayılan kapalıdır; veri sözlüğü, retention ve silme yolu testten önce yayıma hazır hâle gelir.
- **Kararsız build'in ilk izlenimi bozması:** Playtest dalı content-freeze ve P0/P1 kapısından geçmeden davet genişletilmez.

### Geçiş ölçütü

- Ana akış softlock oranı kabul eşiğinde.
- Save kaybı P0 olarak sıfır toleransla çözülmüş.
- Performans preset'leri gerçek donanım verisine dayanıyor.
- Farklılaşma oyuncu dilinde tekrar ediliyor.
- En önemli sıkıcılık kaynakları için ustalık/delegasyon karşılığı var.

## 16. Faz 12 — Beta ve Windows/Steam 1.0

**Öncelik:** P0  
**Bağımlılık:** Alpha ve dış test kabulü  
**Zorluk:** XL  
**Geçici süre:** 4–8 ay

### Beta tanımı

İçerik ve özellik kilitlidir. Yalnız hata, denge, performans, uyumluluk, çeviri ve yayın hazırlığı yapılır.

### İş paketleri

- Final Windows IL2CPP player ve Steam depot pipeline'ı gerçek Windows x64 build hostunda; Windows Unity Editor, desteklenen Visual Studio C++ Build Tools ve Windows SDK sürümleri kilitlenerek çalışır. Mac'ten üretilen Mono build yalnız smoke kontroldür ve release artifact'i değildir.
- Temiz kurulum, kaldırma/güncelleme ve kullanıcı dizini testleri.
- Steam Input, Cloud çatışması, başarımlar ve offline davranış.
- Store sayfası, kapsül, ekran görüntüsü, fragman ve doğru özellik metni.
- İçerik/AI anketi ve lisans kanıtları.
- Marka adı, telif, kayıt-tescil, vergi ve gizlilik kontrolleri.
- Son EULA/privacy/support metinleri.
- Crash/Guardian rapor alımı için güvenli sunucu veya rapor kanalı; opt-in.
- Native hard-crash handler/SDK gerekiyorsa ayrı lisans, binary boyutu, sembol, redaksiyon, DPA/retention ve güvenlik onay kapısı; Guardian'ın hard crash'i tek başına yakalayacağı varsayılmaz.
- Release branch, rollback build ve day-one destek planı.
- Basın/creator anahtar politikası ve dolandırıcılık önlemi.
- Steam review tamponu.

### Riskler ve karşı önlemler

- **Yanlış depot veya bozuk güncelleme:** Internal/beta/release depot ayrımı, imzalı build manifest'i ve denenmiş rollback paketi.
- **Steam Cloud'un yerel kaydı ezmesi:** Çakışma simülasyonu, oyuncu seçimi ve çevrimdışı yerel kayıt kanıtı.
- **Store vaadi–build farkı:** Her mağaza iddiası release checklist'te oynanabilir kanıt ve ekran görüntüsü sürümüyle eşleştirilir.
- **Lisans/hukuk/vergi eksiği:** Asset register, marka, Türkiye kayıt-tescil, sözleşme, gizlilik ve vergi için yayımdan önce uzman kapısı.
- **İlk gün hotfix'inin yeni kayıt bozması:** Yama migration, golden save ve rollback testini geçmeden canlıya çıkmaz.

### Yayın engelleyici ölçütler

- Açık P0 yok.
- Ana akışı etkileyen P1 yok veya kabul edilmiş çok dar istisna.
- Save migration ve rollback testi geçiyor.
- Temiz Windows sisteminde internet olmadan temel oyun çalışıyor.
- Steam Cloud kapalıyken yerel kayıt eksiksiz.
- Store vaatlerinin tamamı build'de gerçekten var.
- Tüm asset/müzik/font/yazılım lisans kayıtları tamam.
- Gizlilik/telemetry varsayılanı ve rıza akışı doğrulandı.
- TR/EN release QA tamam.
- Destek ve acil hotfix üretme prosedürü denendi.

### Yayın sonrası ilk dönem

- İlk 72 saat için crash/save/performance triage.
- P0/P1 hotfix; yeni özellik yok.
- Rapor nedenlerini toplu örüntüyle sınıflandırma.
- Yama öncesi migration ve rollback testi.
- Değişiklik notlarında açık, dürüst ve oyuncu verisini koruyan iletişim.

## 17. Faz 13 — macOS sürümü

**Öncelik:** P2; Windows 1.0'dan sonra  
**Bağımlılık:** Stabil Windows kod tabanı, bütçe ve Apple/Steam hazırlığı  
**Zorluk:** L–XL  
**Geçici süre:** 3–8 ay

### İş paketleri

- Desteklenecek macOS sürümü ile Apple Silicon/Intel/Universal mimari kararı; Windows'taki scripting backend seçiminin otomatik kopyası sayılmadan macOS Mono/IL2CPP boyut, performans ve plugin matrisiyle ayrıca ölçülmesi.
- Metal shader/material testleri.
- Platforma özgü native eklenti denetimi.
- Dosya yolu, input, pencere, DPI ve izinler.
- Performans preset'leri.
- Steam macOS depot, Cloud çapraz cihaz testi.
- Apple Developer Program üyeliği ve sertifika sahipliği için ayrı bütçe/onay kapısı; final build'in macOS build hostunda Developer ID Application ile imzalanması, gerekli hardened runtime/entitlement ayarları, Apple notarization ve uygun olduğunda ticket stapling.
- İmza/notarization ile Steam depot doğrulamasının ayrı kapılar olduğunun kabulü; biri diğerinin yerine geçmez.
- Ayrı beta ve QA.

### Riskler

- Windows'a özgü paket veya shader bağımlılığı.
- IL2CPP/native plugin farkları.
- Küçük pazar için destek maliyetinin bütçeyi aşması.
- Cloud save'in iki platformda farklı katalog/build sürümüyle çatışması.
- Developer ID sertifikası, entitlement veya notarization hatasının Steam dışı/ilk çalıştırma Gatekeeper akışını engellemesi.

### Geçiş ölçütü

- Windows kayıtları desteklenen şemada macOS'a güvenle taşınır.
- Metal'de görsel ve performans kabul edilir.
- Temiz, desteklenen bir Mac'te imza zinciri ve Gatekeeper ilk çalıştırma doğrulanır; Apple notarization tamamdır ve Steam macOS depot'u ayrıca uçtan uca test edilmiştir.
- Ayrı macOS destek kapsamı ve minimum sistem gereksinimi yayımlanabilir.

## 18. Sistem bağımlılık haritası

```text
Kaynak güvenliği + proje hafızası
        ↓
Motor/repo/test temeli
        ↓
Kimlik + katalog + stok + transaction + save
        ↓
Fiziksel teslimat/raf/kasa ─────┐
        ↓                       │
Müşteri ihtiyaç/satış           │
                                ├─→ Vertical slice → kalite kararı
Özel iş + uyumluluk + montaj ───┤
        ↓                       │
Test/paket/teslim/garanti ──────┘
        ↓
Servis/ikinci el çekirdeği
        ↓
Çalışanların perakende/montaj/servis işlerini devralması
        ↓
Tedarik/ekonomi + lokasyon/şube/kanal ölçeği
        ↓
Alpha → Playtest → Beta → Windows 1.0 → macOS
```

Guardian, kayıt, erişilebilirlik, Windows testleri ve lisans kayıtları en sonda eklenen kutular değildir; bütün satırlara yatay kalite katmanı olarak eşlik eder.

### 14 legacy Dashboard karşılığının faz sahipliği

| Eski ekran | Yeni modül/karşılık | Sahibi faz | Doğrulama kapısı |
|---|---|---|---|
| Dashboard | Fiziksel terminalde operasyon özeti | Faz 2; cila Faz 4/10 | Dünya state'iyle aynı değer; tek tık fiziksel iş yok |
| Component Market | Tedarikçi, PO, ETA, teslim ve kabul | Faz 2; derinlik Faz 5/8 | Sipariş kabul olmadan stoğa girmez; eksik/hasar claim'i izlenir |
| Inventory | Konum, seri/batch, kondisyon, rezervasyon | Faz 2; ölçek Faz 5/6 | Raf–depo–el–iş emri tek kimlikte uzlaşır |
| Assembly Workshop | Fiziksel build graph, montaj, OS ve test | Faz 3; genişlik Faz 6 | Solvability proof, nedenli uyumluluk, ara save |
| Customers | Fiziksel NPC, ihtiyaç, danışma, kasa, sadakat | Faz 2/4; derinlik Faz 7/9 | İhtiyaç ve memnuniyet nedenleri açıklanır |
| Staff | Vardiya, görev, beceri, politika ve yardım | Faz 7 | Soak testinde ürün kaybı yok; blocker nedeni görünür |
| Service Center | Intake, teşhis, izin, onarım, burn-in, RMA | Faz 6; ölçek Faz 8 | Her işte kanıt ve en az bir çözülebilir yol |
| Store & Rent | Gerçek lokasyon, kira, taşınma ve yeniden amaçlandırma | Faz 8 | Yeni lokasyon yalnız alan değil yeni baskı/fırsat getirir |
| Finance | Ledger, cash flow, COGS, vade, borç ve forecast | Faz 2; tam denge Faz 8/10 | Para hareketi transaction'a bağlı ve tahmin açıklanabilir |
| Brand & Market | İtibar boyutları, kampanya, rakip ve tedarik ilişkisi | Faz 8; denge Faz 10 | Reklam güven satın almaz; etki fiziksel trafik/talepte izlenir |
| Business Intelligence | Nedensel KPI, risk ve nakit tahmini | Faz 8; doğruluk Faz 10 | Öneri kaynak veriye ve güven aralığına geri izlenir |
| Upgrades | Fiziksel ekipman, renovasyon ve politika | Faz 5; ekonomi/lokasyon Faz 8 | Satın alınan etki dünyada var; soyut yüzde buff değildir |
| Activity Log | Oyuncuya açık operasyon/işlem/kurtarma geçmişi | Faz 2; UX Faz 10 | Kritik değişim neden koduyla bulunur; ham Guardian raporu görünmez |
| Career | Bölüm hedefi, hikâye kırıntısı ve nitelikli başarım | Faz 8/9; denge Faz 10 | Ham adet grind'ı değil karar/ustalık/kriz sonucu ölçer |

### Onaylı kararların açık faz sahipliği

| Karar ID | Uygulama sahibi | Doğrulama sahibi | Tamamlanma kanıtı |
|---|---|---|---|
| D-022 | Faz 9 — kurucu adı ve görünüm/el-ten varyantları | Faz 10 | Tüm varyantlar aynı rig, animasyon, araç, collider ve save akışında clipping/veri kaybı olmadan çalışır |
| D-046 | Faz 7 — kasiyer ve gözetimli self-checkout otomasyonu | Faz 7/10 | Oyuncu müdahalesi korunur; istisna ve ödeme işlemleri aynı authoritative ledger kurallarını kullanır |
| D-112 | Faz 2 — çoklu manuel yuva, döner autosave, kritik checkpoint ve ilk recovery | Faz 10; Cloud çatışması Faz 12 | Yuvalar birbirini sessizce ezmez; fault injection/migration sonrası son sağlam kayıt seçilebilir |

## 19. Kritik yol

En uzun ve birbirine bağımlı iş zinciri:

1. Etkileşim ve görünür eller.
2. Authoritative kimlik/stok/transaction.
3. Save ve recovery.
4. Fiziksel teslimat–raf–satış.
5. PC build graph ve geometrik montaj.
6. Müşteri işi/test/teslim.
7. Servis ve ikinci el çekirdeğinin aynı stok/iş kurallarıyla çalışması.
8. Çalışanların aynı görevleri güvenle devralması.
9. Katalog, ekonomi, ilerleme ve içerik genişletme.
10. İşlevsel mahalle, kişisel ev ve araç/logistik katmanının aynı stok/save/ekonomi authority'sine bağlanması.
11. Windows/Steam release QA.

Şube, dekor, rakip veya yüzlerce ürün bu kritik yolu hızlandırmaz. Çekirdek tamamlanmadan başlatılırsa bitişi geciktirir.

## 20. En büyük üretim riskleri

| ID | Risk | Erken sinyal | Önlem | Acil karar |
|---|---|---|---|---|
| R-001 | Kapsam şişmesi | Her hafta yeni ana sistem | Slice dışı backlog, faz kapısı | Yeni sistemi dondur |
| R-002 | Montaj kombinasyon patlaması | Her SKU yeni collider bug'ı | Az kurgusal aile, standart, matris test | Kataloğu küçült |
| R-003 | NPC pathfinding/görev kaybı | Takılan sıra/raf çalışanı | Placement doğrulama, rezervasyon, LOD | NPC sayısını sınırlayıp rota düzelt |
| R-004 | Save bozulması | Yarım yazma, checksum/migration hatası veya geri yüklemede ilerleme kaybı | Atomik snapshot+journal, döner yedek, migration ve fault injection | Yazmayı durdur; son sağlam checkpoint/recovery yoluna dön |
| R-005 | Fizik jank'i veya fiziksel nesne kaybı | Titreşim, clipping, dünya dışına düşme | Hassas işte snap/animasyon, state–projection ayrımı, son güvenli konum | Serbest fiziği o nesne sınıfında kapat; karantinaya al |
| R-006 | Tekrar ve mikro yönetim | Üçüncü tekrarda sıkılma veya aşırı tıklama | Ustalık kısayolu, politika, batch ve delegasyon | Animasyonu değil zorunlu eylem sayısını azalt |
| R-007 | Ekonomi deadlock/iflas adaletsizliği | Tek olayla çöküş veya tek zorunlu strateji | Nakit forecast, uyarı, sermaye tamponu, kademeli kurtarma | İlerleme eğrisini yeniden kur; haksız gideri dondur |
| R-008 | Asset-flip algısı | Hero yüzeylerde tutarsız hazır asset görünümü | Benzersiz hero asset ve tutarlı art direction | Kimlik taşıyan asset'i yeniden üret/değiştir |
| R-009 | Lisans/telif/marka | Eksik provenans veya gerçek markaya aşırı benzerlik | Asset register, kurgusal marka ve hukuk kapısı | Asset/isim kullanımını durdur ve değiştir |
| R-010 | Mac–Windows ayrışması | Geç Windows build | Erken donanım kapıları | Windows test erişimini öne çek |
| R-011 | Fansız Mac'te uzun build/import | Isıl yavaşlama, cache/import kuyruğu | Batch küçültme, düşük önizleme, cache ve ölçülü ağır iş | Ağır işi zamanla/böl; milestone'u bloklayan işi başka uygun hosta taşı |
| R-012 | Guardian yanlış pozitif veya gizlilik sorunu | Gereksiz recovery ya da redaksiyon dışı alan | Açık invariant, güven puanı, opt-in ve redaksiyon testleri | Otomatik recovery/gönderimi kapat; olayı karantinaya al |
| R-013 | Oyuncunun iki çekirdek arasında bölünmesi | Atölyedeyken haksız mağaza cezası veya bir sütunun ihmal edilmesi | Randevu, kapasite, açılış/kapanış ve çalışan kapsaması | Eşzamanlı talebi düşür; akış ağırlığını yeniden dengele |
| R-014 | Tutorial softlock | Geçerli alternatif sırada hedef ilerlemiyor | State tabanlı/idempotent hedef, skip/reset ve solvability | Bloklayan adımı otomatik tamamla/yeniden tahsis et |
| R-015 | “Her şeyi yapıyor ama hiçbir şeyi iyi yapmıyor” algısı | Çok sistem, zayıf ana döngü geri bildirimi | Önce tek garaj zincirini cilalama ve go/pivot/stop kapısı | Yeni sistemleri dondur; çekirdek kapsamı daralt |
| R-016 | Stok, ledger ve save durumunun ayrışması | Negatif stok, açıklamasız para, çift işlem veya aynı save'in farklı sonuç vermesi | Authoritative state, atomik transaction, kayıtlı seed, uzlaşma testi ve Guardian invariant | İçerik freeze; bütünlük sprint'i ve ledger/save uzlaştırması |
| R-017 | Özgün sanat üretiminin sistem geliştirmesini kilitlemesi | Graybox sistemden çok birikiyor veya hero asset kritik yolu durduruyor | Modüler kit, hero asset önceliği ve yalnız ölçülmüş darboğazda dış destek | Dış destek ROI kapısı veya geçici görsel kapsam daraltma |
| R-018 | Araç, cloud kota ve depolama maliyetinin gizlice büyümesi | Kota/cache/art tekrar işi veya beklenmeyen ödeme gereksinimi | Aylık kota/disk/lisans denetimi; ödeme yöntemi eklemeden önce ayrı onay | Araç/depolama değişim kapısı |
| R-019 | Tek geliştirici çalışma yükü ve tükenme | Çok eşzamanlı WIP ve haftalık hedeflerin sürekli taşması | Aynı anda tek ana milestone, küçük haftalık teslim ve kapsam/saat yeniden tabanlama | Üretim hızını düşür; kapsamı yeniden tabanla |
| R-020 | Rakip taklidi veya hazır asset şablonu algısı | Ekran/isim/döngü çok benzer veya kimlik taşıyan hazır varlıklar baskın | Kurgusal kimlik, özgün hero varlıklar ve güven/teknik değer üzerinden mekanik ayrım | Görsel/mekanik ayrımı güçlendir; sorunlu varlığı değiştir |
| R-021 | Unity 6.3 desteğinin üretim tamamlanmadan bitmesi | Alpha yaklaşırken kullanılan sürüm artık desteklenmiyor | Alpha öncesi ayrı dalda zorunlu desteklenen LTS yükseltme kapısı | Feature freeze ve migration sprint'i |
| R-022 | Gerçek Windows x64 test cihazının zamanında sağlanamaması | İlk Mono smoke build gerçek PC'de denenemiyor veya Windows-hosted IL2CPP baseline alınamıyor | Cihaz/ödünç/uygun test erişimini ilk oynanabilir milestone bağımlılığı yap | İlerlemeyi platformdan bağımsız çekirdekle sınırla; release vaadi verme |
| R-023 | Native hard crash ve raporlama gizliliğinin Guardian kapsamı sanılması | Guardian raporu üretilemeden proses kapanıyor | Breadcrumb + unclean shutdown ayrımı; native SDK için ayrı lisans, DPA, redaksiyon ve retention kapısı | Crash altyapısını gizlilik/lisansla yeniden seç |

## 21. İş yönetimi yöntemi

### Tek kaynaklar

- `Game Design Bible`: tasarımın bütünü.
- `Proje Hafızası`: onaylı karar ve açık sorular.
- Teknik karar kayıtları: “neden bu mimari?”
- Backlog: yapılacak iş ve kabul ölçütü.
- Hata defteri: yeniden üretim, etki, sürüm, düzeltme kanıtı.
- Asset register: kaynak/lisans/üretim bilgisi.
- Build/release notları: hangi commit, katalog ve save şeması.

### Bir işin “hazır” olması

- Oyuncu değeri açıklanmış.
- Bağımlılıkları tamam.
- Kapsam ve kapsam dışı yazılı.
- Kabul testi belirli.
- Save, Guardian, performans ve erişilebilirlik etkisi düşünülmüş.
- Gerekli asset/lisans kaynağı belli.

### Bir işin “tamam” olması

- Kod/asset yalnız çalışmıyor; kabul ölçütünü geçiyor.
- Dilime özel otomatik test, full EditMode + PlayMode regression, scene contract ve ilgili native smoke geçiyor.
- Gerçek oyuncu senaryosu WASD'nin dört yönü, mouse-look, keyboard/mouse ve gamepad kenar/pause/repress akışlarını editör hilesi olmadan kapsıyor; otomatik insan-şekilli rota gerçek insan oturumu diye yeniden adlandırılmıyor.
- Windows etkisi exact-head temiz klonda x64 IL2CPP/Direct3D11 build/runtime ile sınandı veya açık risk olarak kaydedildi.
- Mantık/gerçekçilik denetimi authority, fiziksel sahiplik, erişilebilir rota, zaman/ekonomi nedeni, başarısızlık/recovery ve save/load sonucunu gerçek oynanış ihtimalleriyle tek tek zorluyor.
- Save/migration etkisi test edildi.
- Yeni hata veya tasarım borcu kayda geçti.
- Belgeler ve kaynak/lisans kayıtları güncellendi.

## 22. Haftalık çalışma ritmi

Uygulamaya geçildiğinde önerilen ritim:

1. Haftanın tek doğrulama hedefi.
2. En fazla bir ana sistem işi aynı anda.
3. Önce başarısız otomatik/oynanış testi, sonra uygulama.
4. Küçük, geri alınabilir commit'ler.
5. Haftada en az bir Windows build uygun aşamadan sonra.
6. Haftalık 15–30 dakikalık gerçek oynama; editör hilesi olmadan.
7. Haftanın sonunda karar/hata/risk/belge güncellemesi.
8. Bir sonraki hafta için yalnız en büyük engel seçimi.

Codex kod üretebilir ve testleri çalıştırabilir; kullanıcı milestone hissini oynayarak onaylar. Kritik tasarım değişikliği yalnız “kodlandı” diye kabul edilmez.

## 23. Maliyet kapıları

### Kapı A — €0 prototip

- Unity Personal, Blender, ücretsiz ses/2D araçları.
- Graybox ve özgün basit varlıklar.
- Ücretsiz sürüm kontrol kotası.
- Mac authoritative yazma/Git/GitHub hattıdır. Lenovo ThinkPad T14s Gen 3 ayrı temiz worker olarak exact-head Windows x64 IL2CPP/Direct3D11 build, Intel Iris Xe runtime/input/performance, paketleme ve kanıt üretiminde kullanılır; aynı checkout'a iki yazma hattı açılmaz.

### Kapı B — Vertical slice yüksek etki harcaması

Yalnız kanıtlanırsa:

- Hero el/animasyon veya güçlü bir modüler mağaza kit'i.
- Kaliteli ses paketi.
- Verim artıran IDE/aracı.
- Windows test donanımı erişimi.

Her teklif için: fiyat, alternatif, lisans, disk, bakım, geri alma ve kaç haftalık iş kazandırdığı sunulur.

### Kapı C — Üretim dış desteği

- Karakter/animasyon.
- Fragman/marketing art.
- Profesyonel ses/müzik.
- TR/EN edit ve ek yerelleştirme.
- Hukuk, marka ve vergi.
- Geniş donanım QA.

### Kapı D — Yayın

- Steam Direct.
- Gerekli ticari/hukuki kayıtlar.
- Raporlama/website/support altyapısı.
- macOS için daha sonra Apple Developer Program/Developer ID, imza–notarization ve ayrı port QA bütçesi; Steam depot doğrulaması bunlardan ayrı tutulur.

## 24. Başarı göstergeleri

### Teknik

- Kayıp/çift stok olayı: sıfır toleranslı kritik metrik.
- Save success/recovery oranı.
- Crash-free session ve P0/P1 sayısı.
- Hedef donanım frame-time/bellek.
- NPC görev timeout ve recovery oranı.
- Guardian false positive oranı.

### Oynanış

- İlk satış ve ilk PC teslim tamamlama oranı.
- Hata nedenini doğru anlayan oyuncu oranı.
- Fiziksel eylem tekrarında sıkılma noktası.
- Perakende–atölye arasında geçirilen dengeli süre.
- Oyuncunun geliştirdiği farklı kârlı strateji sayısı.
- Güven/itibar kararlarının hatırlanması.

### Ürün

- Oyuncuların farklılaştırıcı cümleyi kendi sözleriyle söylemesi.
- Demo/Playtest geri dönüş ve oturum göstergeleri.
- Destek taleplerinde save/tutorial/performance oranı.
- Store vaadi ile gerçek deneyim uyumu.

Bu metrikler manipülatif günlük giriş veya zorunlu grind hedefi değildir; kalitenin kanıtıdır.

## 25. İlk uygulama sprinti için önerilen kapsam

Kurulum ayrıca onaylandığında ilk sprint yalnız şunları yapmalıdır:

- Boş, sürüm kontrollü başlangıç Unity 6.3 LTS URP projesi ve kayıtlı yükseltme kapısı.
- Assembly/module iskeleti.
- Test çalıştırma ve macOS'tan Windows Mono boş smoke build; Faz 1 kapanmadan aynı commit için gerçek Windows x64 hostta IL2CPP toolchain baseline kanıtı.
- Birinci şahıs graybox oda.
- Tek kutuyu alma, taşıma ve doğrulanmış yüzeye bırakma.
- Stable item kimliği ve konsolda olay zinciri.
- Basit save/reload deneyi.

İlk sprintte müşteri, ekonomi, PC parçası kataloğu, güzel sanat veya Steam yoktur. Hedef oyunun tamamını göstermesi değil, üretim temelinin güvenli olduğunu kanıtlamasıdır.

## 26. Bir sonraki büyük karar kapıları

Kullanıcının küçük uygulama ayrıntılarını proje liderine devretmesi korunur. Yalnız oyunun yönünü veya önemli maliyeti değiştiren şu konular ayrıca sorulur:

1. Bu 0.1 tasarım paketinin ortak anlayış olarak onayı.
2. Araç kurulum ve sürüm kontrolü için uygulama izni.
3. Vertical slice sonucu “go/pivot/stop”.
4. İlk ücretli asset/araç/dış destek.
5. Final isim ve marka kimliği.
6. Steam sayfası ve dış test açılışı.
7. Early Access mı tam sürüm mü—yalnız kanıt oluşunca.
8. Windows 1.0 release candidate onayı.
9. macOS port bütçesi ve hedef kapsamı.

Diğer küçük, geri alınabilir ve düşük maliyetli kararlar belgelenerek proje lideri tarafından alınabilir; sonuç sonraki durum raporunda kullanıcıya bildirilir.
