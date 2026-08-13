# ADR-0012 — Kontrollü Küçük Kutu Placement Rotation

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 13 Ağustos 2026<br>
**İlgili işler:** GitHub #6, #33

## Bağlam

Kontrollü küçük-kutu placement sistemi işaretli yüzey, grid/yaw snap, ghost ve fail-closed çakışma doğrulaması sağlıyordu. Ancak oyuncu kutunun yüzey üzerindeki yönünü kasıtlı olarak seçemiyordu. Serbest ve sürekli dönüş ilk dilimde gereksiz hassasiyet, fizik titremesi ve klavye/gamepad davranış ayrışması riski taşıyordu.

## Karar

- `RotatePlacement` ayrı bir Input System eylemidir: klavyede `R`, gamepad'de `Right Shoulder` varsayılan binding'ini kullanır ve etkin binding HUD'da gösterilir.
- Rotation yalnız eldeki `SmallBox` placement modundayken çalışır. Büyük kutu bu girişle placement moduna veya rotation durumuna geçmez.
- Her basış clockwise `90°` ekler. Durum quarter-turn integer olarak tutulur ve dört adımda deterministik biçimde sıfıra sarar.
- İstenen yaw, yüzey snap'inden önce hesaplanır. Ghost ve onay aynı `PlacementSolver` girdisini kullanır; oyuncunun gördüğü poz ile yerleşen poz ayrışmaz.
- Döndürülmüş kutunun gerçek yarı boyutları aynı tam-destek ve obstruction kontrollerinden geçer. Geçersiz sonuç fail-closed kalır ve nesne elde tutulur.
- Placement iptali, başarılı yerleştirme, güvenli bırakma veya recovery rotation durumunu sıfırlar. Stable item ID, fizik snapshot'ı, tek taşıma slotu ve recovery sözleşmeleri değişmez.
- Serbest/sürekli rotation, pitch/roll, büyük-kutu placement ve kutu üstü istifleme bu kararın dışındadır.

## Sonuçlar

- Klavye/fare ve gamepad aynı dört yönlü, tekrar üretilebilir placement davranışını kullanır.
- Dikdörtgen küçük kutu ve üst yön işareti dönüşü graybox içinde görünür kılar; bunlar final sanat değildir.
- Rotation sonrası footprint değişimi güvenlik doğrulamasına dahil olduğundan ghost'un geçerli/geçersiz durumu anında güncellenir.
- İstifleme ve raf planogramı ayrı acceptance paketleri olarak kalır; dünya projeksiyonu authoritative Inventory sayılmaz.

## Doğrulama

- EditMode: quarter-turn normalizasyonu, 90° farkı, sabit action ID/binding ve sahne kutu ölçüsü.
- Gerçek Input System PlayMode: klavyede `R`, gamepad'de `Right Shoulder`, etkin prompt, ghost/confirm poz eşitliği, döndürülmüş obstruction fail-closed, başarılı placement ve durum sıfırlama.
- GarageGraybox: dikdörtgen küçük kutu, turuncu yön işareti ve `rotation=ok` runtime tanısı.
