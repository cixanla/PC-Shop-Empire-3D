# PC Shop Empire – 3D Dönüşüm Ana Dosyası

**Belge durumu:** Onaylı araştırma/tasarım paketi 0.1 + uygulanmış Stage A, Stage B Core temeli ve private collaboration checkpoint'i 0.8<br>
**Tarih:** 11 Ağustos 2026  
**Çalışma biçimi:** Yaşayan belgeler; karar değiştikçe sürüm notuyla güncellenecek.

## Sonuç

Yeni oyunu eski Electron projesini 3D'ye çevirmeye çalışarak değil, **başlangıç tabanı olarak Unity 6.3 LTS + URP + C# ile sıfırdan** kurmak doğru yoldur. 6.3 desteği Aralık 2027'de biteceği için uzun üretim içinde alpha öncesi kontrollü LTS yükseltme kapısı zorunludur. Eski oyun; tema, ilerleme omurgası, Dashboard bölümleri, veri anlamları ve tasarım niyetleri için bir şartname kaynağıdır. Mevcut JavaScript, HTML, CSS, arayüz, marka/model listesi ve görseller yeni oyuna doğrudan taşınmamalıdır.

Oyunun belirgin konumu:

> **Fiziksel teknoloji perakendesi + teknik PC ustalığı + uzun vadeli müşteri güveni.**

Bu, yalnızca kutu açıp raf dolduran başka bir mağaza simülatörü ya da yalnızca tezgâhta PC toplayan bir montaj oyunu olmayacak. Mağaza, atölye, servis, çalışanlar, tedarik, finans ve satış sonrası işlemler aynı stok ve işlem gerçeğini paylaşacak.

## Bu pakette ne var?

1. [Game Design Bible](01_GAME_DESIGN_BIBLE.md): oyun vizyonu, döngüler, bütün ana sistemler ve vertical slice kapsamı.
2. [Mevcut Proje Denetimi ve Dönüşüm Matrisi](02_MEVCUT_PROJE_VE_DONUSUM_MATRISI.md): eski oyunun dosya/mimari/Dashboard incelemesi ile korunacak, dönüştürülecek, yeniden tasarlanacak, eklenecek ve çıkarılacak alanlar.
3. [Rakip Araştırması ve Farklılaşma](03_RAKIP_ARASTIRMASI_VE_FARKLILASMA.md): PC Building Empire, PC Building Simulator serisi, doğrudan PC mağazası rakipleri ve başarılı mağaza/tycoon simülatörlerinden çıkarılan dersler.
4. [Teknik Mimari, Araçlar ve PSE Guardian](04_TEKNIK_MIMARI_ARACLAR_VE_GUARDIAN.md): motor, platform, modüler mimari, kayıt güvenliği, NPC yaklaşımı, araç/maliyet/lisans planı ve gömülü tanılama sistemi.
5. [Geliştirme Yol Haritası](05_GELISTIRME_YOL_HARITASI.md): aşamalar, bağımlılıklar, zorluklar, riskler ve kabul ölçütleri.
6. [Proje Hafızası](06_PROJE_HAFIZASI.md): onaylanmış kararlar, varsayımlar, açık büyük kararlar, kapsam dışı ve ertelenen konular.
7. [Kaynak Kayıt Defteri](07_KAYNAKLAR.md): yerel ve çevrimiçi araştırma kaynakları, güven düzeyleri ve tarih notları.
8. [Canonical Kaynak ve Kesin Kurulum Planı](08_CANONICAL_KAYNAK_VE_KURULUM_PLANI.md): USB hash manifesti, bilgisayar envanteri, kesin Unity/araç sürümleri, disk-maliyet etkisi, yedekleme ve geri alma planı ile tek kurulum onayı kapsamı.
9. [Stage A Teknik Kurulum Raporu](09_STAGE_A_KURULUM_RAPORU.md): gerçek kurulum sürümleri, proje/paket ayarları, 4/4 test, macOS ve Windows buildleri, USB snapshot doğrulaması ile UVCS bağlantı engelinin neden zinciri.
10. [Devam ve Kullanım Güvenliği Checkpoint'i](10_DEVAM_CHECKPOINT.md): son sağlam commit/snapshot, kullanım tasarrufu protokolü ve kesintisiz devam sırası.

Repository'yi ilk kez devralan kişi önce root [`PROJECT_BIBLE.md`](../../PROJECT_BIBLE.md), ardından [`Docs/DEVELOPER-HANDOFF.md`](../DEVELOPER-HANDOFF.md) ve [`Docs/REPOSITORY-GOVERNANCE.md`](../REPOSITORY-GOVERNANCE.md) belgelerini okumalıdır. Günlük yürütme private [`cixanla/PC-Shop-Empire-3D`](https://github.com/cixanla/PC-Shop-Empire-3D) Issues ve [Development Roadmap Project](https://github.com/users/cixanla/projects/2) üzerinden izlenir.

## MacBook ile Windows oyunu geliştirebilir miyiz?

**Evet.** Bu MacBook Air'in Apple M4 işlemcisi ve 32 GB belleği Unity'nin resmî editör gereksinimlerini karşılar; prototip ve günlük geliştirme için uygundur. Tam üretimde büyük import, bake ve build kapasitesi henüz ölçülmediği için milestone benchmark'larıyla doğrulanacaktır. Apple Silicon editörde CPU lightmapping desteklenmediğinden gerekiyorsa GPU lightmapper kullanılacak ve bake süresi/belleği ayrıca ölçülecektir. Cihaz fansız olduğundan uzun görevlerde ısıya bağlı hız düşmesi olabilir. Altına hava akışını engellemeyen bir stand veya soğutucu koymak yardımcı olabilir; yine de iş yükünü küçük ve ölçülebilir paketlere ayırmak daha önemlidir.

Mac'ten **Windows Build Support (Mono)** ile erken Windows x64 build'leri üretilebilir. Fakat final Windows IL2CPP derlemesi native Windows editörü/toolchain'i ister; DirectX, Steam, sürücüler, çözünürlükler ve performans da yalnız gerçek Windows x64 PC'de güvenilir biçimde doğrulanır. Apple Silicon üzerindeki Windows ARM sanal makinesi olsa olsa smoke testtir; bunun yerine geçmez. Windows PC bugün doğrulanmış mevcut kaynak değil, ilk oynanabilirden önce karşılanması gereken dış bağımlılıktır. Bu nedenle plan:

- MacBook: günlük ana geliştirme ve prototipleme.
- İlk oynanabilir çekirdek oluşunca: Mac'ten Mono build; ardından erişimi sağlanmış gerçek Windows x64 PC'de native editör/build ve test.
- Vertical slice, alpha ve release candidate: düzenli Windows regresyon/performance testleri.
- Windows sürümü bitmeden macOS yayın taahhüdü yok.
- Windows sürümü tamamlanıp bütçe uygun olduğunda macOS imzalama, notarization ve ayrı kalite turu.

Unity'nin sürüme özel notları: [Unity 6.3 sistem gereksinimleri](https://docs.unity3d.com/6000.3/Documentation/Manual/system-requirements.html), [Unity 6.3 IL2CPP](https://docs.unity3d.com/6000.3/Documentation/Manual/il2cpp-introduction.html).

## Şu anda ne yapıldı, ne yapılmadı?

Yapıldı:

- Eski proje, dosya izinleri değiştirilmeden salt-okunur yöntemle incelendi.
- Kullanılan teknoloji ve 14 Dashboard bölümü haritalandı.
- Mevcut veri, test, kayıt ve teknik borç alanları belirlendi.
- PC ve mağaza simülatörleri; resmî sayfalar, yama notları ve oyuncu geri bildirimleri üzerinden karşılaştırıldı.
- Gerçek mağaza/servis işleyişlerinden seri takibi, teslimat kabulü, garanti, iade ve veri silme ilkeleri araştırıldı.
- Temel oyun kararları yaşayan proje hafızasına işlendi.
- USB canonical kaynak ile yerel inceleme aynası 26/26 dosyada yol, boyut ve SHA-256 düzeyinde doğrulandı.
- Canonical legacy manifesti kurulum sonunda tekrar doğrulandı; 26/26 dosya hâlâ birebir eşleşiyor.
- Ücretsiz Stage A kapsamı kullanıcı onayıyla uygulandı: Unity Hub 3.20.1, Unity 6000.3.21f1 ARM64 ve Windows Build Support (Mono) kuruldu.
- Legacy'den ayrı, sıfırdan Unity 6.3 LTS URP projesi oluşturuldu; resmî paketler kilitlendi.
- Edit Mode testleri 4/4 geçti; macOS Universal ve Windows x64 Mono development buildleri başarıyla üretildi.
- macOS player headless smoke testinde motor açılıp temiz biçimde kapandı.
- Unity Cloud projesi ve ücretsiz/private UVCS repo oluşturuldu; ödeme yöntemi eklenmedi.
- UVCS credential exchange başarılı oldu; ilk check-in ise iki resmî istemci yolunda aynı uzak bağlantı reseti nedeniyle güvenli biçimde durduruldu ve yarım workspace oluşmadı.
- Yeni Unity kaynağı ve yaşayan belgeler için ayrı, hash doğrulamalı USB Stage A snapshot'ı hazırlandı.
- Apple Git ile yerel `main` geçmişi başlatıldı; doğrulanmış Stage A kaynak temeli `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166` commit'i ve `stage-a-baseline-2026-08-11` etiketiyle sabitlendi.
- Stage B'nin ilk bounded paketi tamamlandı: Unity bağımlılığı taşımayan `PSE.Core` assembly sınırı, test iskeleti ve mimari karar kaydı eklendi; Edit Mode toplamı 6/6 geçti ve paket `8ecb05df48257d22dc7f4549c8dbfe7b261772a9` commit'iyle kapatıldı.
- İkinci bounded paket tamamlandı: tür kapsamlı canonical `StableId<TScope>`, oyuncuya gösterilmeyen makine-okunur failure kodu ve güvenli `OperationResult` sözleşmeleri eklendi; toplam Edit Mode sonucu 24/24 geçti ve paket `4cd2d928dbfda1886632bacce4a141c2a43161df` commit'iyle kapatıldı.
- Üçüncü bounded paket tamamlandı: işletim sistemi/Unity saatini okumayan açık-adımlı `SimulationClock` ile sıralama, replay ve duplicate tespiti için immutable alan olayı zarfı eklendi; toplam Edit Mode sonucu 42/42 geçti ve paket `8af2ad3d05906839c4b607e4958650e723060465` commit'iyle kapatıldı.
- Yeni oyun için private `cixanla/PC-Shop-Empire-3D` repository oluşturuldu; `main`, Stage A etiketi, Unity kaynakları, yaşayan belgeler, repo guard ve byte-exact legacy snapshot güvenli biçimde push edildi.
- 22 üst seviye epic oluşturuldu ve private `PC Shop Empire 3D — Development Roadmap` Project'ine bağlandı; Phase/Priority/Risk/Status alanları yürütme görünümü olarak tanımlandı.
- Gerçek Unity Git kökü Codex içinde `Game` adlı, Git repository olarak algılanan ayrı Project şeklinde kaydedildi.

Yapılmadı:

- Oynanabilir gameplay, Guardian runtime, 3D model veya ticari içerik üretimi başlatılmadı; yalnız saf çekirdek assembly iskeleti oluşturuldu.
- Blender, Steamworks SDK, Xcode, ücretli araç veya üçüncü taraf oyun asset'i kurulmadı.
- Eski proje, kayıtlar ve USB'deki legacy klasörler değiştirilmedi; yalnız onaylı yeni `90_BACKUPS/PCShopEmpire3D` hedefi yazıldı.
- Ücretli araç veya lisans satın alınmadı.
- UVCS cloud'a ilk kaynak check-in'i tamamlanmadı; bağlantı uzak uç tarafından sıfırlandı.
- Git LFS henüz eklenmedi; büyük binary asset kabulünden önce kota, lisans ve collaborator etkisiyle ayrı kapıda seçilecek.
- Gerçek Windows x64 bilgisayarda runtime, DirectX/GPU, Steam veya IL2CPP testi henüz yapılmadı.
- macOS çıktısı imzalanmadı/notarize edilmedi ve yayın sürümü değildir.

## Kaynak güvenliği notu

USB 11 Ağustos 2026'da `/Volumes/cixanla` altında yeniden bağlandı. USB'deki kaynak ile önceki aktarımda oluşturulmuş **yazılabilir yerel inceleme kopyası üzerinde hiçbir değişiklik yapmadan, salt-okunur yöntemle** karşılaştırıldı. USB'deki canonical legacy kaynak:

`/Volumes/cixanla/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

Doğrulanmış birebir yerel aynası:

`/Users/cixanla/Documents/Codex/2026-08-02/new-chat/work/transfer_review/CIXANLA/02_PC_SHOP_EMPIRE_MASAUSTU_1.1.6/KAYNAK_KODU`

İki kaynakta da 26 dosya vardır ve 26/26 göreli yol, boyut ve SHA-256 karşılaştırması eşleşmiştir; yalnız okuma yapıldı. Mac için hazırlanmış sonraki çalışma kopyası canonical değildir: ana oyun mantığı eşleşir, ancak `THIRD_PARTY_NOTICES.txt`, `forge.config.js`, `package-lock.json`, `package.json` ve `styles.css` macOS paketleme, lisans envanteri ve font çalışmaları nedeniyle ayrışır. Önceki placeholder Git deposunda gerçek kaynak yoktur; yeni Unity kaynağı artık kendi ayrı yerel Git geçmişine sahiptir.

Legacy Electron güvenlik bulgularının kanıt sınırı da nettir: kaynak/config düzeyinde `contextIsolation: true`, `nodeIntegration: false`, `sandbox: true` ve dış pencere/navigasyon engeli doğrulanmıştır. CSP tamamen “self-only” değildir; `style-src` içinde `'unsafe-inline'`, `img-src` içinde `data:` izni vardır. ASAR bütünlüğü ve fuse sertleştirmesi `forge.config.js` içinde yapılandırılmıştır; paketlenmiş binary'nin etkin fuse durumu ayrıca doğrulanmadıkça runtime gerçeği olarak sunulmaz. Bunlar yeni Unity oyunu için doğrudan güvenlik garantisi değil, legacy referans bulgularıdır.

## En önemli kapsam kararı

Tam oyun büyük olacak; fakat üretime şu küçük ve kaliteli çekirdekle başlanacak:

- Tek garaj ve teslimat önü.
- Birinci şahıs hareket, görünür eller, taşıma ve yönlendirilmiş fizik.
- Sipariş, fiziksel teslimat, kutu açma, depolama, raf, fiyat ve kasa.
- Temel müşteri gezinmesi ve ihtiyaç görüşmesi.
- Baştan sona tek özel PC işi: teklif, parça, fiziksel montaj, kurgusal OS, test, paketleme ve teslim.
- Temel Dashboard, ekonomi ve kayıt; oyuncuya gösterilmeyen Guardian tanı olay zinciri.
- Küçük ama doğrulanmış ürün kataloğu.

Çalışanlar, şubeler, geniş servis, online satış, gelişmiş rekabet ve yüzlerce ürün vertical slice'a yığılmayacak. Önce zincir kusursuz çalışacak, sonra genişleyecek.

## Sonraki kapı

Stage A ve private GitHub güvenlik/devir temeli tamamlandı. UVCS ilk check-in'i dış bağlantı reseti nedeniyle beklemeye alınmış, private GitHub `main` tek authoritative uzak geçmiş olarak seçilmiştir. Stage B'nin saf `PSE.Core` sınırı; kararlı kimlik/sonuç ile deterministik zaman/alan olayı sözleşmeleri 42/42 testle tamamlandı. Bir sonraki bounded iş, [Issue #2](https://github.com/cixanla/PC-Shop-Empire-3D/issues/2) kapsamındaki kayıtlı seed ve olay bağlamından yeniden üretilebilir sonuç veren deterministik RNG sözleşmesidir.

Gameplay prototipine geçiş ayrı Stage B kapsamıdır. Bu geçiş; Blender, ücretli araç, Steam/Apple ödemesi, üçüncü taraf asset, gerçek Windows IL2CPP release build veya legacy kaynak değişikliği için otomatik yetki vermez.
