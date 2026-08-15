# Yüklü Taşıma Arabası — Doğrulama Kanıtı

**Tarih:** 15 Ağustos 2026  
**Epic / issue:** [#6](https://github.com/cixanla/PC-Shop-Empire-3D/issues/6) / [#37](https://github.com/cixanla/PC-Shop-Empire-3D/issues/37)  
**Feature commit:** `82bf74f90fd5bce9f4f17244aea6afde4a7ef2c1`  
**Feature tree:** `1d48b75c74e5ae14ee92d4f0687a68ec35182ddd`

## Görünür sonuç

GarageGraybox artık dört tekerli, metal/rubber yüzeyli tek platform arabası içerir. Oyuncu büyük kutuyu `E / Gamepad South` ile yükler veya geri alır; boş ellerle `Mouse Left / Gamepad RT` kullanarak arabayı tutar/bırakır. Yüklü araba elde taşınan büyük kutudan hızlıdır, sprinti kapatır ve duvar/nesne ya da eksik zemin desteğinde son güvenli pozda kalır. HUD yüklü/boş durumu ile etkin bindingleri gösterir.

## Otomatik ve çalıştırılabilir kanıt

| Kanıt | Sonuç | SHA-256 |
|---|---|---|
| `cart-editmode-final.xml` | `136/136`, failed `0`, skipped `0` | `6de78a3e7be6d47e9780962bdabef4a64d5efe153bf3b572fee90c5da98c9bca` |
| `cart-playmode-final.xml` | `14/14`, failed `0`, skipped `0` | `87b3c4e42d73186191740b69971b482bb49ab68c882105f48e7aee39628ccea3` |
| `cart-macos-build.log` | Universal development build, `327.282.300` bayt | `1511e285a0cb051b1216c11d455efba7334fdda22ef75456e627451a7677f347` |
| Player executable | Mach-O `x86_64 + arm64` | `d6d5e7afdf5cae9d39c6696507bf9ea8c181b22a58299de31b8968889739ba27` |
| `cart-macos-runtime-final.log` | Apple M4/Metal, 1280×720, `transport-cart=ok`, `cart-flow=ok loaded=ok stable=ok` | `e2a5c113f28db09d4746182bb062031b29b601b0e188d8737fe5967ca5ef2a56` |
| `cart-macos-runtime-final.png` | 1280×748 oyun penceresi; yüklü araba ve dinamik HUD görünür | `816fec72ed909be4a5ab9244a888adbddce0743b1b42450ec27cabcf72bfc5d2` |

Ham kanıtlar repository dışındaki `../TestResults` klasöründedir. Feature Repository Guard [31859948692](https://github.com/cixanla/PC-Shop-Empire-3D/actions/runs/31859948692) başarılıdır.

## USB milestone

`/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-15_STAGE_B_LOADED_TRANSPORT_CART` hedefi checkpoint docs commit'i `148c6d1f2936307268237ae2c484743146f7e639` üzerinden oluşturuldu. `SOURCE` içinde 396 tracked dosya, `EVIDENCE` içinde 6 kanıt vardır. `SOURCE_COMMIT.txt` ile birlikte 403 satırlı manifest dosya hash'i ve boyutunu doğrular; manifest SHA-256 değeri `a9e1d8e5188d85503dbff923127ac3bd71c6d9e023acf17003beddadfe0444c3`dür. Source→USB birebir karşılaştırma, manifest readback ve yasak cache/build/credential taraması sıfır hatayla geçti.

## Korunan invariantlar ve sınır

- Küçük-kutu pickup/drop/placement/rotation/istif ile büyük-kutu elde taşıma regresyonları geçti.
- Hands → cart → hands → world geçişi aynı stable item kimliğini ve ilk physics snapshot'ını korur.
- Hareket yalnız tam zemin desteği ve temiz swept bounds ile uygulanır; başarısızlık yükü veya arabayı serbest fiziğe bırakmaz.
- Araba/carry controller disable durumları yükü son güvenli dünya pozuna kurtarır.
- Tek `LargeBox` kapasitesi bilinçli sınırdır; bu projection henüz raf veya authoritative Inventory değildir.
