# ADR-0014 — Kontrollü Küçük Kutu İstifleme

**Durum:** Kabul edildi ve uygulandı
**Tarih:** 15 Ağustos 2026
**Bağlı işler:** Epic [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6), Issue [#35](https://github.com/cixanla/PC-Shop-Empire-3D/issues/35)

## Bağlam

Serbest rigidbody kutu yığını; titreme, iç içe geçme, kayıp ürün ve tekrar yüklemede farklı sonuç riski taşır. İlk oynanabilir stok akışı için amaç fizik simülasyonu gösterisi değil, oyuncunun niyetini okunaklı ve güvenli biçimde gerçekleştiren bir placement sözleşmesidir.

## Karar

- Yalnız dünya durumunda, gravity kapalı ve kinematic olan `SmallBox` başka bir küçük kutuya destek olabilir.
- Üst kutu desteğin merkezine ve desteğe göre en yakın `90°` yaw açısına snap olur.
- Merkez ile dört köşeden aşağı yapılan beş ışın aynı destek kutusuna temas etmeden placement geçerli sayılmaz.
- Döndürülmüş dikdörtgen footprint destekten taşıyorsa veya oyuncu/duvar/nesne overlap'i varsa işlem fail-closed kalır; kutu elde tutulur.
- Bir taban yalnız bir üst kutu kabul eder ve iki kutudan daha yüksek zincir kurulmaz. Üstünde kutu bulunan taban alınamaz.
- Üst kutu alındığında iki yönlü runtime ilişki çözülür. Geçerli istif sonucu gravity kapalı kinematic poz, stable item ID ve son güvenli recovery pozu korunur.
- Klavye/fare ve gamepad mevcut placement eylemlerini kullanır; geçerli hedef HUD'da `İSTİF GEÇERLİ` olarak görünür.

## Sonuçlar

İlk stok alanında deterministik, titremesiz ve kayıpsız iki-kutu istifi vardır. Bu ilişki henüz ekonomik stok gerçeği değildir; yalnız dünya projeksiyonudur. Çok katlı/palet istifi, büyük kutu istifi, taşıma arabası ve raf `Inventory` authority ayrı paketlerdir.

## Doğrulama

- EditMode: stabil/dinamik destek, tam footprint, rotation, ilişki ve pickup kilidi.
- PlayMode: gerçek Input System keyboard/mouse ve gamepad ile pickup → preview → rotation/fail-closed → confirm zinciri.
- GarageGraybox: iki ayrı stable kimlikli küçük kutu ve `stacking=ok` runtime tanısı.
