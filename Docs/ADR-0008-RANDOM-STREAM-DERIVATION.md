# ADR-0008 — Kayıtlı ana tohumdan bağlamsal rastgele akış türetme

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 13 Ağustos 2026

## Bağlam

Pazar fiyatı, müşteri gelişleri, teslimat hasarı ve benzeri simülasyon kararları aynı şirket kaydı ve aynı kalıcı olay bağlamında tekrar üretilebilmelidir. Tek bir global PRNG çağrı sırası değişince bütün sonuçları kaydırır. Saat, cihaz entropy'si veya eksik save alanında otomatik yeni seed ise kayıt yükleyerek sonucu yeniden atma açığı doğurur.

## Karar

- Şirket ana tohumu `RandomRootSeed` ile bütün `ulong` alanını taşır ve save metadata'sında tam 16 haneli lowercase hex olarak saklanır. Prefix, boşluk, uppercase, eksik ya da uzun değer reddedilir.
- Türetme kimliği `sha256-framed-be-pcg32-v1`, hedef PRNG kimliği `pcg32-xsh-rr-64-32-v1`dir.
- Domain ve occurrence context yalnız `StableId<RandomStreamDomainScope>` ile `StableId<RandomStreamContextScope>` kabul eder. Localized metin, koleksiyon index'i, çağrı sayısı ve CLR type adı kimlik değildir.
- SHA-256 girdisi tam olarak şu sıradadır:

  `Frame("pse.random-stream-derivation.v1") || U64BE(rootSeed) || Frame(Pcg32Algorithm.Id) || Frame(domain) || Frame(context)`

  `Frame(s) = U32BE(UTF8 byte length) || stable-ID bytes` biçimindedir. Stable ID sözleşmesi girdileri lowercase ASCII ile sınırlar.
- Digest'in ilk 8 byte'ı big-endian `initialState`; sonraki 8 byte'ı big-endian okunup `0x7fff_ffff_ffff_ffff` ile maskelenen `streamSelector` olur. Son 16 byte v1'de ayrılmıştır.
- Aynı root/domain/context üçlüsü çağrı sırasından ve kültür ayarından bağımsız aynı initialization ve draw dizisini üretir. Yeni occurrence yeni kalıcı context ID alır.
- Eksik/bozuk seed, bilinmeyen derivation ID veya bilinmeyen PRNG ID açık yükleme hatasıdır; zaman/entropy fallback'i yoktur.
- Uzun süren ve değişken sayıda draw tüketen iş ayrıca `Pcg32State` saklar. Gerçekleşmiş fiyat, sipariş veya hasar gibi dünya olguları doğrudan save edilir; derivation save'in yerine geçmez.

## Golden drift kapıları

Root `0000000000000000`, domain `tests.golden.v1`, context `event.0001`:

- SHA-256: `92868a93c2ce5d67d75602a2ceef8afc841523e8cf9d12732f29f600105ff722`
- Initial state: `92868a93c2ce5d67`
- Selector: `575602a2ceef8afc`
- İlk altı draw: `825f9a3f, 9e3a5650, ded60ec6, f277362a, 10c6d09a, ceaa040a`

İkinci production-benzeri ekonomi vektörü de testlerde digest, initialization ve altı draw ile kilitlidir. Bu binary sözleşmedeki herhangi bir değişiklik yeni derivation ID ve açık save migration kararı gerektirir.

## Sınırlar

- SHA-256 burada kararlı türetme içindir; root seed secret değildir ve HMAC gerekmez. PCG32 kriptografik generator değildir.
- Keyfî context uzayından 63-bit selector'a matematiksel birebir eşleme yoktur. Initial state + selector birlikte yaklaşık 127-bit initialization alanı sağlar; katı bağımsızlık veya imkânsız collision iddiası kurulmaz.
- Root seed yalnız New Company application boundary'sinde bir kez üretilip ilk rastgele sonuçtan önce atomik save'e yazılacaktır; bu paket entropy üreticisini veya tam save serializer'ını içermez.
- macOS Unity golden paketi geçmiştir. İlk Windows IL2CPP kapısında aynı vektörler yeniden doğrulanacaktır.

## Kaynak ve provenans

- NIST Secure Hash Standard: <https://csrc.nist.gov/pubs/fips/180-4/upd1/final>
- .NET `SHA256` API: <https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.sha256>

Uygulama .NET BCL kullanır; repository'ye üçüncü taraf SHA kodu veya paket kopyalanmamıştır.
