# PC Shop Empire 3D — Devam ve Kullanım Güvenliği Checkpoint'i

**Tarih:** 11 Ağustos 2026  
**Durum:** Tamamlanmış ve kurtarılabilir checkpoint; yarım çalışan işlem yok  
**Son kullanıcı bildirimi:** Yaklaşık %30 kalan Plus kullanımı

## Kullanım güvenliği protokolü

- Hesapta kalan yüzde model tarafından doğrudan ve güvenilir biçimde okunamaz; Codex kullanım paneli veya kullanıcının bildirdiği değer authoritative kabul edilir.
- İşler küçük, tamamlanabilir paketlere bölünür. Her paket sonunda test/inceleme, Git checkpoint ve gereken milestone USB manifesti kapatılır.
- Bildirilen veya görünen kullanım %5 ya da altındaysa yeni uzun iş başlatılmaz.
- Yaklaşık %2'de aktif iş güvenli en yakın sınırda durdurulur; çalışma ağacı temizlenir veya açık değişiklik açıkça kaydedilir, bu dosya ile Proje Hafızası güncellenir, USB hash kontrolü yapılır ve kullanıcıya “kullanım bitmek üzere” denir.
- Belirsiz sürede indirme, build, araştırma veya büyük kod dönüşümü düşük kullanımda başlatılmaz.
- Daha az tüketim için gereksiz alt ajan, Computer Use, görsel üretim ve tekrar taramalarından kaçınılır; yalnız gerekli dosya ve kaynaklar okunur.

## Son sağlam teknik durum

- Unity proje kökü: `/Users/cixanla/Developer/PCShopEmpire3D/Game`
- Unity: `6000.3.21f1`, URP `17.3.0`, C#.
- Son Edit Mode sonucu: 42/42 geçti, başarısız 0 (`stage_b_time_events_editmode_20260811.xml`).
- macOS Universal development build ve headless smoke: geçti.
- Windows x64 Mono cross-build: geçti; gerçek Windows runtime/IL2CPP/Steam/DirectX testi hâlâ dış bağımlılık.
- Legacy canonical kaynak: 26/26 yol, boyut ve SHA-256 eşleşiyor.
- UVCS: private repo var; ilk check-in yok, `.plastic` workspace yok; bağlantı reseti nedeniyle beklemede.

## Yerel Git checkpoint

- Branch: `main`
- Root commit: `b7ac8c36d9fb7be1eacf08b8f2b273d9c2574166`
- Tree: `b4610805fec7eed0c5fe41ec36b2cd5e4b820fa4`
- Tag: `stage-a-baseline-2026-08-11`
- Commit kapsamı: 81 dosya, 273.462 mantıksal bayt.
- Bilinen secret kalıbı: 0.
- Generated/cache/build yolu: 0.
- `git fsck --full`: geçti.
- Çalışma ağacı: temiz.
- Remote: yok.
- Git LFS: kurulmadı; mevcut küçük ve metin ağırlıklı kaynak için henüz gerekli değil.

Stage B bounded checkpointleri:

- HEAD: `8ecb05df48257d22dc7f4549c8dbfe7b261772a9`
- Tree: `d6ff7d59ae8ff311e4e4f05b243fd4f2d7989d46`
- İçerik: Unity bağımsız `PSE.Core` assembly sınırı, anchor, iki assembly sınır testi ve ADR-0003.
- Gameplay davranışı veya asset eklenmedi.
- `git fsck --full`: geçti; çalışma ağacı temiz.
- İkinci paket HEAD'i: `4cd2d928dbfda1886632bacce4a141c2a43161df`
- İkinci paket tree'si: `1814b4a59f6de378130913733192261bc19802ba`
- İkinci paket: tür kapsamlı canonical `StableId<TScope>`, makine-okunur `Failure.Code`, generic/non-generic `OperationResult`, 18 yeni davranış testi ve ADR-0004.
- Toplam test: 24/24 geçti; çalışma ağacı temiz. Önceki `8ecb05d` commit'i ve Stage A etiketi korunur.
- Son HEAD: `8af2ad3d05906839c4b607e4958650e723060465`
- Son tree: `566c1884e681feb8fbf0f68e0fb0a7594b560012`
- Üçüncü paket: integer simulation timestamp/duration, açık-adımlı pause destekli clock, event ID/type/sequence ve immutable domain event envelope; 18 yeni davranış testi ve ADR-0005.
- Toplam test: 42/42 geçti; çalışma ağacı temiz. Önceki commitler ve Stage A etiketi korunur.

## USB güvenlik katmanı

USB milestone hedefi:

`/Volumes/cixanla/CIXANLA/90_BACKUPS/PCShopEmpire3D/2026-08-11_STAGE_A_BASELINE`

Snapshot yalnız yeniden üretilemeyen Unity kaynaklarını ve yaşayan plan belgelerini içerir. `.git`, `.plastic`, credential, `Library`, cache, log ve build çıktıları alınmaz. Kesin dosya/bayt/hash değerleri snapshot içindeki `MANIFEST.tsv` ve `MANIFEST.sha256` ile belirlenir; her milestone kapanışında kaynakla yeniden doğrulanır.

## Devam sırası

1. Kalan kullanımı panel veya kullanıcı bildiriminden kontrol et.
2. `git status`, `git fsck` ve baseline tag doğrulamasını yap.
3. USB `MANIFEST.sha256` ve kaynak checksum dry-run kontrolünü yap.
4. Sıradaki bounded Stage B paketi olarak kayıtlı seed ve olay bağlamından yeniden üretilebilir sonuç sağlayan deterministik RNG sözleşmesini testleriyle ekle; gameplay kapsamını aynı pakete alma.
5. Her paketi ayrı commit ve kabul testiyle kapat.
6. Büyük binary asset gelmeden önce Git remote + Git LFS kararını kesinleştir.

## Düşük kullanımda bırakılacak mesaj

> Kalan kullanım yaklaşık %2 seviyesine indi. Yeni uzun iş başlatmadım. Son tamamlanan commit, açık değişiklikler, test sonucu, USB manifesti ve sıradaki tek adım bu checkpoint dosyasında kayıtlı; ek kredi geldikten sonra buradan güvenle devam edebiliriz.
