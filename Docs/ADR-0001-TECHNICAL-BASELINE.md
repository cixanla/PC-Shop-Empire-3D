# ADR-0001 — Stage A teknik temel

**Durum:** Kabul edildi ve uygulandı  
**Tarih:** 11 Ağustos 2026

**Uygulama istisnası:** Yerel teknik temel, testler ve iki development build tamamlandı. Unity Cloud/private UVCS repo oluşturuldu ve credential exchange geçti; ilk check-in ise hem standalone istemci hem Editor entegrasyonunda uzak `connection reset by peer` hatası nedeniyle bekliyor. Yerel `.plastic` workspace oluşmadı. Bu istisna çözülene kadar hash doğrulamalı USB kaynak snapshot'ı ikinci güvenlik katmanıdır.

## Karar

- Oyun eski Electron kodundan port edilmez; yeni Unity projesi sıfırdan kurulur.
- Unity Editor `6000.3.21f1` ve Universal Render Pipeline `17.3.0` sabitlenir.
- İlk ana hedef Windows/Steam'dir. Mac geliştirme ve erken Mac build yapılır; gerçek Windows cihaz testi mümkün olan en erken uygun milestone'da başlar.
- Asset serialization `Force Text`, meta dosyaları `Visible Meta Files` olur.
- İlk aşamada yalnız resmî Unity paketleri kullanılır; üçüncü taraf asset ve runtime AI eklenmez.
- Unity Version Control için Editor içi resmî entegrasyon kullanılır. Ayrı macOS istemcisinin ayrıcalıklı yerel sunucu bileşeni ayrıca onaylanmadan kurulmaz.
- Unity projesinin kökü aynı zamanda UVCS workspace köküdür; `Docs`, `SourceAssets` ve `Tools` bu kökün içindedir.
- USB yalnız tarihli kaynak snapshot'ları içindir; canlı Unity workspace veya cache USB'de çalıştırılmaz.

## Gerekçe

Bu temel, MacBook üzerinde düşük maliyetli geliştirmeyi mümkün kılarken Windows hedefini erken buildlerle doğrular; ikili varlıklar için kilitleme ve Unity sahneleri için metin tabanlı diff imkânı bırakır. Eski oyunun davranış/veri bilgisi korunur fakat eski teknik borç yeni mimariye taşınmaz.

## Yeniden değerlendirme kapıları

- Nihai oyun adı ve marka taraması.
- İlk oynanabilir prototipten sonra performans bütçesi.
- Gerçek Windows cihazında ilk işlev testi.
- UVCS 15/20/23 GB kota eşikleri.
- Blender ve diğer içerik araçları için ayrı ihtiyaç/onay.
