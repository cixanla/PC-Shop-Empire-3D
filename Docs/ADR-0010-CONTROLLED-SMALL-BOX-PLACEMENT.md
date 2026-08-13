# ADR-0010 — Kontrollü Küçük Kutu Placement Sözleşmesi

**Durum:** Kabul edildi ve uygulandı
**Tarih:** 13 Ağustos 2026
**İlgili işler:** GitHub #6, #31

## Bağlam

İlk pickup/drop prototipi, küçük kutuyu aynı stable kimlikle elde taşıyıp yakındaki güvenli zemine bırakabiliyordu. Stoklama için yalnız serbest rigidbody bırakma yeterli değildir: oyuncunun niyetini açıkça göstermesi, yerin geçerli olup olmadığını önceden görmesi ve kutunun duvar, oyuncu veya başka nesneyle çakışmadan deterministik bir pozda kalması gerekir.

## Karar

- `PrimaryAction` (`Mouse Left / Gamepad RT`) placement önizlemesini açıp kapatır. Mod kapalıyken `G / Gamepad East` mevcut güvenli drop davranışını korur; mod açıkken aynı giriş placement'ı onaylar.
- Kontrollü placement yalnız `PlacementSurface` ile işaretlenmiş yüzeylerde geçerlidir. İlk graybox yüzeyi `prototype.stock-floor-small-box-a` kimliği, yüzeye göre `0,25 m` grid ve `90°` yaw snap kullanır.
- Solver sabit aday mesafelerini aynı sırada dener; yataya yakın normal, beş noktalı tam taban desteği ve obstruction `CheckBox` geçmeden poz geçerli sayılmaz.
- Default/world, Interactable ve Player katmanları obstruction kapsamındadır. Geçersiz sonuç fail-closed'dur; aynı kutu elde kalır.
- Ghost collider taşımaz, gölge üretmez ve yeşil/kırmızı malzemenin yanında `GEÇERLİ/ENGELLİ` metni kullanır; renk tek sinyal değildir.
- Onaylı placement, dünya layer/collider durumunu geri yükler ve kutuyu gravity-off kinematic pozda sabitler. Normal drop özgün rigidbody snapshot'ını geri yüklemeye devam eder. Stable item ID ve son güvenli poz recovery sözleşmesi değişmez.

## Sonuçlar

- Küçük kutu placement'ı fizik darbesi veya settling nedeniyle grid dışına kaymaz.
- Serbest/adımlı rotation inputu, istifleme, raf planogramı, büyük kutu taşıma profili ve authoritative Inventory bu kararla uygulanmış sayılmaz.
- Placement yüzeyleri gelecekte kapasite, erişim ve Inventory konum komutlarına bağlanabilir; mevcut component yalnız dünya doğrulama/projeksiyon katmanıdır.

## Doğrulama

- EditMode: yüzey kimliği, grid/yaw snap, işaretsiz yüzey ve obstruction davranışı.
- Gerçek Input System PlayMode: mouse-left ve gamepad RT ile moda giriş; engelli ghost'ta elde tutma; keyboard `G` ve gamepad East ile aynı stable ID'yi yerleştirme; fixed-step poz kararlılığı.
- GarageGraybox: görünür stok yüzeyi, sabit engel, connected PlayerRig içindeki ghost ve dinamik prompt.
