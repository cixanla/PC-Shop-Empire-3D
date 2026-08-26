# PC Shop Empire 3D — İnsan Oyuncu Kabul Kapısı

Bu belge, her oynanabilir geliştirme paketinin yalnız sınıf veya metot düzeyinde değil,
gerçek bir oyuncunun yaşayabileceği akışlar üzerinden doğrulanmasını zorunlu kılar. Amaç
sonsuz ihtimal iddiasında bulunmak değil; yüksek olasılıklı, yüksek etkili ve sınır durumlu
senaryoları sistematik biçimde kapsayarak sahte güveni önlemektir.

**Sürüm:** v2 — 27 Ağustos 2026

## İki ayrı kabul sınıfı

Bu belge artık issue/dilim kapanışı ile nihai Steam 1.0 yayın sertifikasyonunu açıkça ayırır:

1. **Teknik oynanabilir dilim kapanışı:** Exact source üzerinde domain, scene/input ve tam
   regresyon; Mac ve Windows native runtime; gerçek foreground pencereye işletim sistemi
   seviyesinde sürülen keyboard/mouse girdisi ve sıfır süreç kalıntısı birlikte geçtiğinde,
   kullanıcı oturumu bulunmadan bounded issue kapanabilir. Bu yol yalnız otomasyonun gerçek
   oyuncu rotasını yeterince temsil ettiği, gözlenen ürün sonucu hash-bound kanıtlandığı ve
   kapsam sınırları açıkça yazıldığı durumda kullanılabilir.
2. **Nihai Steam 1.0 yayın sertifikasyonu:** Aşağıdaki tam insan risk matrisi, desteklenen
   fiziksel cihazlar ve dayanıklılık turu gerçek insan tarafından ayrıca tamamlanır. Teknik
   issue kapanışları bu yayın kapısını düşürmez veya silmez.

Agent-operated OS-input kanıtı hiçbir zaman `human=true`, `physical keyboard tested` veya
`physical gamepad tested` diye adlandırılamaz. Kayıtta en az exact commit/tree, oturum ve
foreground window kimliği, gönderilen scan code/mouse sayıları, ayrı ve eşzamanlı gözlenen
sonuçlar, artifact hashleri, task/exit sonucu ve player/Unity/PowerShell/task residue bulunur.
Fiziksel gamepad yoksa Input System gamepad otomasyonu yazılabilir; bu yalnız otomasyon PASS'idir
ve fiziksel gamepad sertifikasyonu Steam 1.0 yayın kapısında açık kalır.

## Değişmez kaynak ve sürüm kimliği

- Tek yazma kaynağı Mac üzerindeki güncel Git çalışma ağacıdır.
- USB yalnız doğrulanmış, değişmez kilometre taşı yedeğidir; eski USB içeriği canlı kaynağın
  üzerine geri taşınmaz.
- Her native build açılışta güncel `GARAGE_GRAYBOX_RUNTIME_READY` sürümünü yazmalıdır.
- Mac ve Windows oynanabilir kopyaları, yalnız test edilen build'in dosya kimliği ve runtime
  işareti doğrulandıktan sonra güncellenir.
- Kaynak sürümü ile masaüstündeki uygulama sürümü farklıysa bu bir P1 teslimat hatasıdır ve
  yeni geliştirme kapatılmaz.

## Her paket için zorunlu beş katman

1. Domain ve veri testleri: başarı, geçersiz girdi, çakışma, stale revision, tekrar oynatma,
   taşma ve kısmi başarısızlıkta sıfır yan etki.
2. Sahne ve giriş testleri: gerçek Input System, gerçek `CharacterController`, odak, mesafe,
   görüş hattı, collider ve sunum metinleri.
3. Tam regresyon: bütün EditMode ve PlayMode paketleri; eski mekanikler de yeşil kalmalıdır.
4. Native runtime: Mac Metal ve Windows IL2CPP/DirectX build'i, exact başarı işareti ve hata
   taraması.
5. Kabul oturumu: bounded teknik issue için yukarıdaki sıkı agent-operated OS-input yolu veya
   aşağıdaki gerçek insan risk matrisi; nihai Steam 1.0 için daima gerçek insan matrisi.

## İnsan oturumu risk matrisi

### Hareket, kamera ve pencere durumu

- W, S, A ve D ayrı ayrı basma, basılı tutma ve bırakma.
- W+D ve W+A çapraz hareket; hızın diyagonal yönde artmaması.
- W+S ve A+D zıt girdileri; yatay sürüklenme olmadan nötr sonuç.
- Yürüme, koşma, yük taşırken koşu engeli ve taşıma arabası hız profili.
- Fare ve gamepad bakışı, ters Y seçeneği, hassasiyet, yukarı/aşağı açı sınırı.
- Pause sırasında tüm fiziksel işlemlerin donması; devam edince eski tuş basışının yeniden
  tetiklenmemesi.
- Alt-tab/odak kaybı, geri dönüş, pencere ve tam ekran çözünürlüğü.

### Fiziksel etkileşim

- Doğru nesneye doğru mesafe ve görüş hattından yaklaşma.
- Menzil dışı, arkası dönük, görüşü kapalı ve yanlış nesne odaklı denemeler.
- Alma, taşıma, döndürme, bırakma, kontrollü yerleştirme ve güvenli kurtarma.
- Aynı işlemi iki kez yapma, yanlış sırada yapma, işlem ortasında pause ve sahne yeniden
  yükleme.
- Her durumda tek fiziksel nesne, tek kararlı kimlik ve doğru authoritative container.
- Collider, `Ignore Raycast`, kablo görselleri ve etkileşim hedeflerinin birbirini yanlışlıkla
  engellememesi.

### Ticaret, müşteri ve envanter

- Başarılı uçtan uca müşteri ziyareti: giriş, gezinme, görüşme, karar, kasa veya çıkış.
- Eksik stok, yanlış parça, bütçe aşımı, mevcut rezervasyon, stale komut ve duplicate kimlik.
- Çok parçalı işlemlerde ya tümü ya hiçbiri; kısmi rezervasyon veya kayıp nesne olmaması.
- Aynı komutun exact replay'i: başarı sonucu aynı kalır, revision ve kayıt sayısı artmaz.
- Başarısız işlemde sipariş, teklif, sepet, kasa, ekonomi, müşteri ve assembly otoritelerinin
  yanlışlıkla değişmemesi.
- Ekrandaki fiyat, parça sayısı, istem ve durum metninin authoritative kayıtla aynı olması.

### Oturum dayanıklılığı

- Temiz açılış ve eski kayıt olmadan başlangıç.
- Desteklenen save/load geldiğinde işlem öncesi, ortası ve sonrası kayıt yükleme.
- Uygulamayı normal kapatma/açma; çökme kurtarma tasarımı geldiğinde yarım işlem kurtarma.
- 15 dakikalık kesintisiz yürüyüş ve etkileşim turunda kilitlenme, takılma, bellek artışı,
  tekrar eden müşteri veya kaybolan nesne olmaması.
- Mac ve Windows'ta aynı komut sırası için aynı authoritative sonuç.

## Kanıt ve durdurma kuralı

Her paket; commit/tree kimliği, test XML sayıları, native build kimliği, runtime işareti,
platform/GPU bilgisi ve gözlenen negatif senaryolarla kaydedilir. Aşağıdakilerden biri varsa
paket tamamlanmış sayılmaz:

- oyuncunun temel hareketi veya ana etkileşimi çalışmıyor;
- ekrandaki uygulama doğrulanan kaynak sürümünden eski;
- tek nesne/tek kimlik, atomiklik veya replay değişmezi bozuluyor;
- yalnız bir platform doğrulanmışken iki platform desteği iddia ediliyor;
- test başarısı var ama native uygulama açılıp aynı işareti üretmiyor.
