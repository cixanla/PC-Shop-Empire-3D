# ADR-0007 — Sürümlü deterministik rastgele akış

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 13 Ağustos 2026

## Bağlam

Pazar, müşteri, çalışan, teslimat ve olay simülasyonları tekrar üretilebilir rastgele sonuçlara ihtiyaç duyar. `System.Random`, `UnityEngine.Random`, işletim sistemi zamanı veya cihaz entropy'si; kayıt devamı, hata yeniden üretimi ve farklı desteklenen platformlarda aynı karar zincirini korumak için yeterli bir sözleşme değildir.

Bu ADR yalnız temel PRNG akışını kilitler. Root seed'in save'e yazılması, stable context kimliğinden bağımsız stream türetme ve gameplay entegrasyonu sonraki paketlerdir.

## Karar

- Algoritma kimliği `pcg32-xsh-rr-64-32-v1` olarak sabittir.
- PCG set-sequence LCG; 64-bit state, 64-bit odd increment, `6364136223846793005` multiplier ve 32-bit XSH-RR output kullanır.
- Başlatma sırası: `state=0`; `increment=(streamSelector<<1)|1`; bir transition; `state += initialState`; bir transition.
- Toplama, çarpma ve bit işlemleri açık `unchecked` wrap semantiğindedir.
- `initialState`, `0..ulong.MaxValue` aralığının tamamını kabul eder.
- Set-sequence yalnız 63 selector bitiyle `2^63` benzersiz stream adresler. Bu nedenle public `streamSelector`, `0..0x7FFF_FFFF_FFFF_FFFF` aralığıyla sınırlıdır. High bit sessizce atılmaz; alias yaratacak değer `ArgumentOutOfRangeException` üretir.
- Devam snapshot'ı raw `State` ve odd `Increment` değerlerini taşır. `default(Pcg32State)` ve even increment geçersizdir; sessiz `|1` düzeltmesi yapılmaz.
- Mutable generator `sealed class`tır. Fark edilmeden kopyalanan mutable struct veya global singleton kullanılmaz; thread-safe olduğu iddia edilmez.
- İlk public draw yüzeyi `NextUInt32()` ve pozitif exclusive bound alan `NextInt32(int)` ile sınırlıdır.
- Bounded integer modulo bias üretmez: unsigned threshold `unchecked(0U-bound)%bound`; threshold altındaki draw'lar reddedilir.
- Geçersiz bound exception üretir ve state'i değiştirmez. `bound=1` bile replay çekim sayısını korumak için tam bir draw tüketir.
- Bu generator simülasyon/replay içindir; kriptografi, token, parola, secret veya güvenlik kararı için kullanılamaz.

## Drift kapıları

Resmî PCG minimal C örneğinin `initialState=42`, `streamSelector=54` vektörü sabit testtir:

`a15c02b7, 7b47f409, ba1d3330, 83d2f293, bfa4784b, cbed606e`

Altı draw sonrasındaki raw state `beb6d0b73fdb974a`, increment `000000000000006d` olmalıdır. Algoritma ID'si; initialization, transition, output permutation ve bounded rejection davranışının birlikte sürümüdür. Bunlardan biri değişirse yeni ID ve save migration kararı gerekir.

Test paketi ayrıca 1.000 draw eşitliği, farklı stream tekrarlanabilirliği, snapshot/restore devamı, selector/state sınırları, exception sonrası state değişmezliği, bound range ve deterministik rejection yolunu kapsar.

## Bilinçli sınırlar

- Root seed saklama, context hashing/stream derivation ve reload-reroll engeli bu pakette tamamlanmış sayılmaz.
- Float/double dağılımı, weighted choice, shuffle, normal distribution ve draw count yoktur.
- Save serializer/migration henüz yoktur. Gelecekte JSON içinde 64-bit state/increment precision kaybetmeyecek fixed-width hex/string biçimiyle yazılmalıdır.
- Network lockstep veya platformlar arası floating-point determinizm iddiası yoktur.
- Mevcut golden suite macOS'taki Unity 6000.3.21f1 üzerinde çalıştırılmıştır; gerçek Windows x64 doğrulaması ilk Windows test kapısında ayrıca yapılacaktır.

## Kaynak ve provenans

- Resmî minimal kullanım ve golden vector: <https://www.pcg-random.org/using-pcg-c-basic.html>
- Resmî minimal algoritma indirmesi: <https://www.pcg-random.org/download.html>
- PCG ailesi makalesi: <https://www.pcg-random.org/pdf/toms-oneill-pcg-family-v1.02.pdf>

Referans minimal C kodu Apache License 2.0 altında yayımlanır. Bu repository'deki C# uygulaması proje mimarisine göre özgün yazılmıştır; algoritma/provenans kaydı `Docs/PROVENANCE.md` içinde tutulur.
