# ADR-0004 — Kararlı kimlik ve sonuç sözleşmeleri

**Durum:** Kabul edildi ve uygulandı
**Tarih:** 11 Ağustos 2026

## Karar

- Domain kimlikleri `StableId<TScope>` ile tür kapsamlıdır; ürün, stok birimi veya sipariş kimlikleri aynı metni taşısa bile yanlışlıkla birbirinin yerine kullanılamaz.
- Kalıcı metin biçimi kültürden bağımsız ve dardır: 1–128 karakter; küçük ASCII harf/rakamla başlar ve biter; arada yalnız küçük ASCII harf, rakam, nokta, alt çizgi veya tire bulunabilir.
- Girdi sessizce küçültülmez veya düzeltilmez. Canonical olmayan değer reddedilir; böylece kayıt ve çapraz referans anahtarları ortama göre değişmez.
- Beklenen iş kuralı başarısızlıkları exception yerine `OperationResult` / `OperationResult<T>` ve makine-okunur `Failure.Code` ile taşınır.
- Başarısız sonuç boş hata kodu, başarılı generic sonuç null değer taşıyamaz. Varsayılan/başlatılmamış sonuç güvenli biçimde `core.uninitialized` hatasıdır.
- `Failure.Code` oyuncuya gösterilecek metin değildir. UI katmanı kodu yerelleştirilmiş mesaja dönüştürür; teknik ayrıntı ve kişisel veri bu temel sözleşmeye konmaz.

## Bilinçli sınırlar

- Kimlik üretme politikası, save serializer/converter, migration ve domain'e özgü scope tipleri sonraki paketlerindir.
- Exception hâlâ programlama hataları ve sözleşme ihlalleri içindir; normal stok yetersizliği, uyumsuz parça veya sipariş reddi gibi beklenen sonuçlar failure code kullanır.
- Bu paket gameplay, ekonomi veya Unity sunum davranışı eklemez ve `PSE.Core` assembly'sinin Unity bağımsızlığını değiştirmez.
