# ADR-0015 — Yüklü Taşıma Arabası Graybox Akışı

**Durum:** Kabul edildi ve uygulandı  
**Tarih:** 15 Ağustos 2026  
**Bağlı işler:** Epic [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6), Issue [#37](https://github.com/cixanla/PC-Shop-Empire-3D/issues/37)

## Bağlam

Büyük kutuyu yalnız elde taşımak ilk fiziksel stok akışını kanıtlar, fakat teslimat ile stok alanı arasındaki daha uzun hareketi okunaklı ve verimli kılmaz. Serbest rigidbody araba ve fiziksel bağlantılı yük ise titreme, devrilme, duvardan geçme ve kayıp ürün riski taşır. İlk dilimde amaç araç fiziği simülasyonu değil, oyuncunun tek büyük kutuyu güvenle taşıdığı deterministik bir dünya projeksiyonudur.

## Karar

- İlk platform arabası tek `LargeBox` kapasiteli stable kimlikli bir `TransportCartProjection` olur.
- Elde tutulan büyük kutu `E / Gamepad South` ile boş arabaya yüklenir; aynı fizik snapshot'ı ve item kimliği korunur. Yük, dünya kökünde kinematic tutulur ve cargo anchor pozuna açıkça eşitlenir.
- Boş ellerle `Mouse Left / Gamepad RT` arabayı tutar ve bırakır. Yüklü hız `0,85×`, boş hız `0,90×` olur; her iki durumda sprint kapalıdır.
- Araba sürücüye deterministik pose ile bağlıdır. Dört köşe zemin desteği, hedef overlap ve swept box kontrolü geçmeden hareket uygulanmaz. Destek veya obstruction hatasında araba son güvenli pozda kalır ve tutuş otomatik bırakılır.
- Yük aynı `E / Gamepad South` girişiyle yeniden ellere alınır; mevcut büyük-kutu safe-drop ve recovery zinciri değişmez.
- Araba veya controller beklenmedik biçimde devre dışı kalırsa sürüş profili temizlenir; yüklü item son güvenli dünya pozuna geri alınır.
- HUD etkin klavye/gamepad bindinglerini, boş/yüklü durumu ve engelli hareketi gösterir. Görünür eller araba tutuş pozu kullanır.

## Sonuçlar

GarageGraybox içinde yükle → sür → bırak → geri al → güvenli bırak zinciri oynanabilir ve titremesizdir. Bu sistem ekonomik stok gerçeği değildir; yalnız fiziksel dünya projeksiyonudur. Çoklu slot, palet, serbest büyük-kutu placement, raf container'ı ve authoritative Inventory Issue #7/#8'e bırakılmıştır.

## Doğrulama

- EditMode: ownership transferi, stable kimlik/physics snapshot, kapasite/profil reddi, dört nokta destek, obstruction ve bounded grip menzili.
- PlayMode: gerçek Input System keyboard/mouse ve gamepad ile yükleme, sürme, sprint kilidi, engelde fail-closed duruş, bırakma, geri alma ve disable recovery.
- GarageGraybox: görünür metal/rubber platform arabası ve `transport-cart=ok` runtime tanısı.
- Development player: opt-in `-pse-cart-smoke` akışı normal başlangıcı değiştirmeden `cart-flow=ok loaded=ok stable=ok` kanıtı üretir.
