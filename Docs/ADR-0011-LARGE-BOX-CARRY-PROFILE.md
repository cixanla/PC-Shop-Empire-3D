# ADR-0011 — Büyük Kutu Taşıma Profili

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 13 Ağustos 2026<br>
**İlgili işler:** GitHub #6, #32

## Bağlam

Küçük kutu pickup/drop ve kontrollü placement aynı fiziksel nesneyi güvenli biçimde taşıyabiliyordu. Büyük kutunun aynı davranışı yalnız daha büyük mesh ile kullanması; ağırlık hissini, görüş maliyetini ve bırakma güvenliğini kanıtlamaz. Buna karşılık joint/spring tabanlı serbest fizik, ilk graybox diliminde titreme ve kayıp eşya riskini gereksiz biçimde büyütür.

## Karar

- Fiziksel ürün projeksiyonu açık `SmallBox` ve `LargeBox` taşıma profillerinden birini taşır. Profil kuralları tek yerde, sabit ve test edilebilir değerlerle çözülür.
- Küçük kutu davranışı değişmez: `1,0×` hareket, sprint açık, FOV cezası yok ve kontrollü placement destekli.
- Büyük kutu `0,65×` hareket hızı kullanır ve sprinti kapatır. Hareket çarpanı sözleşmesi `0,5–1,0` aralığında, istenen FOV cezası en fazla `8°` ile sınırlıdır; ilk değer `6°`dir.
- Varsayılan `motionReduced` açıkken lens FOV'u değiştirilmez. Büyük kutunun kamera önündeki fiziksel görünümü ve geniş iki-el pozu görüş/ağırlık geri bildirimi sağlar. `motionReduced` kapatılırsa `6°` FOV geçişi ani sıçrama yerine sınırlı hızla uygulanır ve bırakınca geri alınır.
- Büyük kutu da tek slotlu kinematic carry anchor kullanır. Güvenli bırakma, kutunun gerçek yarı boyutlarıyla zemin desteği ve world/interactable/player obstruction kontrolünden geçmeden gerçekleşmez; hata durumunda kutu elde kalır.
- Büyük kutu bu dilimde küçük-kutu placement moduna giremez. `PrimaryAction` yok sayılır; prompt yalnız etkin `Drop` binding'ini, ağır yük/sprint durumunu ve engelli bırakmayı gösterir.
- Stable item ID, rigidbody/collider snapshot'ı, son güvenli poz ve disable/world-floor recovery sözleşmeleri profil eklenmesiyle değişmez.

## Sonuçlar

- Büyük ve küçük kutu aynı güvenli taşıma altyapısını paylaşırken oyuncuya farklı fiziksel maliyet verir.
- Motion-reduction tercihi kamera lens etkisini kapatabilir; hız maliyeti ve görünür fiziksel engel korunur.
- Büyük kutu placement/rotation/stacking, taşıma arabası ve authoritative Inventory bu kararla uygulanmış sayılmaz.
- Profil değerleri ileride oynanış testiyle yeniden dengelenebilir; aralıklar ve davranış değişikliği ayrı test/karar güncellemesi ister.

## Doğrulama

- EditMode: profil sınırları, sprint/FOV hesabı, sahne boyut/kimlikleri ve carry/release kimlik kararlılığı.
- Gerçek Input System PlayMode: keyboard ve gamepad pickup/drop, sprint engeli, hareket/FOV bedeli, iki-el durumu, büyük-kutuda placement girişinin reddi, blocked drop fail-closed ve disable recovery.
- GarageGraybox: turuncu bantlı büyük kutu, ayrı pedestal, connected PlayerRig ve `large-carry=ok` runtime işareti.
