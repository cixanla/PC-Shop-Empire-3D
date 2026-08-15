# Developer Handoff — Başka Bilgisayarda veya Yeni Geliştiriciyle Devam

Bu belge, projeyi hiç bilmeyen bir geliştiricinin mevcut sağlam checkpoint'ten güvenle devam etmesi içindir.

## 1. Önce okuyun

1. Root `PROJECT_BIBLE.md`.
2. `Docs/ProjectBible/00_OKU_BENI.md`.
3. `Docs/ProjectBible/10_DEVAM_CHECKPOINT.md`.
4. `Docs/ProjectBible/11_BIRLESIK_CODEX_PROJE_HAFIZASI.md`.
5. Çalışacağınız alanın ayrıntılı Game Design Bible/Yol Haritası bölümü.
6. `CONTRIBUTING.md` ve `Docs/REPOSITORY-GOVERNANCE.md`.
7. [GitHub Development Roadmap Project](https://github.com/users/cixanla/projects/2) içindeki atanmış issue ve kabul ölçütü.

Önceki Codex görevlerindeki tam kullanıcı/Codex yazışmaları veya tarihsel dosya değişiklikleri gerektiğinde `Docs/CodexHistory/README.md` indeksinden bulunur. Normal geliştirme yalnız `PC Shop Empire 3D — ANA GÖREV` adlı tek Codex görevi üzerinden sürdürülür.

## 2. Gereken temel araçlar

- Git.
- Unity Hub.
- Unity Editor `6000.3.21f1`, hedef bilgisayarın native mimarisi.
- URP/paketler repository `Packages/manifest.json` üzerinden çözülür.
- Windows final doğrulaması için gerçek Windows x64 host ve ileride IL2CPP/C++ toolchain.
- IDE serbesttir; generated `.sln`/`.csproj` commit edilmez.

Blender, Steamworks SDK, ücretli asset/tool, telemetry SDK ve Apple signing araçları mevcut checkpoint için gerekli değildir; ayrı kapı olmadan kurulmaz.

## 3. Clone ve ilk doğrulama

```bash
git clone https://github.com/cixanla/PC-Shop-Empire-3D.git
cd PC-Shop-Empire-3D
git switch main
./Tools/verify-repository.sh
git status --short
```

Beklenen durum:

- Repo guard başarılı.
- Çalışma ağacı temiz.
- `ProjectSettings/ProjectVersion.txt` Unity `6000.3.21f1` gösterir.
- Legacy manifest 26/26 dosyayı doğrular.

## 4. Unity'yi açma

Unity Hub içinde **Add/Open project from disk** ile clone edilen repo kökünü seçin. `Assets`, `Packages` ve `ProjectSettings` aynı kökte görünmelidir.

İlk açılışta `Library` yeniden üretileceği için import sürebilir; bu klasör Git'e eklenmez. Paket çözümleme sırasında keyfi sürüm yükseltmeyin.

## 5. Test baseline

Unity Test Runner ile Edit Mode ve Play Mode testlerinin tamamını çalıştırın. Son sağlam baseline:

- Edit Mode `255/255` passed.
- Play Mode `18/18` passed.
- `0` failed.
- `0` skipped.

macOS batch örneği:

```bash
/Applications/Unity/Hub/Editor/6000.3.21f1/Unity.app/Contents/MacOS/Unity \
  -batchmode -nographics \
  -projectPath "$PWD" \
  -runTests -testPlatform EditMode \
  -testResults "$PWD/../TestResults/editmode.xml" \
  -logFile "$PWD/../TestResults/editmode.log"
```

Windows'ta Unity executable yolu kurulum dizinine göre değiştirilir. Test çalıştırırken Unity 6 Test Runner'ın tamamlanmasını bekleyin; `-quit` parametresini eklemek bu projedeki doğrulanmış batch akışında test başlamadan kapanmaya yol açmıştır.

## 6. Güncel kod sınırı

Tamamlanan saf Core sözleşmeleri:

- `PSE.Core` assembly, Unity/Editor referansı yok.
- `StableId<TScope>` ve canonical kimlik doğrulaması.
- `Failure` ve `OperationResult`.
- `SimulationDuration`, `SimulationTimestamp`, pause destekli `SimulationClock`.
- `IDomainEvent`, stable type/ID, one-based sequence ve immutable envelope.
- `pcg32-xsh-rr-64-32-v1`, official golden vector, raw state snapshot/restore ve bias'sız bounded integer.
- `sha256-framed-be-pcg32-v1`, canonical root seed, stable domain/context stream derivation ve reload-reroll engeli.
- Event correlation/direct-causation, global FIFO ve breadth-first nested enqueue uygulayan bounded in-memory dispatcher.
- `PSE.Catalog` assembly: immutable product definition, stable product/category kimliği, serialized/batch tracking policy, doğrulanmış görünür ad ve bounded garanti.
- `PSE.Inventory` assembly: authoritative serialized item/batch/container kayıtları, unit capacity, atomik transfer, claim reservation, release/consume, deterministic sorgu, revision ve invariant audit.
- Catalog yalnız Core; Inventory yalnız Core + Catalog; Orders Core + Catalog + Inventory; Retail Core + Catalog + Inventory; Actors ise Core + Catalog referanslıdır. Beş assembly de Unity/Editor bağımlılığı taşımaz.
- `PSE.Orders` assembly: stable purchase order/supplier/delivery kimliği, exact manifest, `Placed → Confirmed → InTransit → Arrived → Accepted` lifecycle ve atomik receiving kabulü.
- `PSE.Retail` assembly: stable shelf-offer, customer basket reservation, immutable checkout ve checkout completion authority'leri; exact serialized item/claim bağları, deterministic sorgu/snapshot, idempotent komutlar, drift denetimi ve failure no-mutation.
- `PSE.Actors` assembly: stable customer/intent/visit kimlikleri, immutable state/deadline/route kayıtları, bounded exact command receipt ledger'ı, deterministic query/revision ve invariant audit.
- Customer visit zinciri `Entering → Browsing → NavigatingToCheckout → AwaitingCheckout → Exiting → Exited`; route state başına iki deneme, `RouteUnavailable`, patience/exit timeout ve güvenli terminal fallback kullanır.
- `InventoryIntake` bütün manifest satırlarını identity/tracking/capacity bakımından preflight eder; başarıda tek revision, failure'da sıfır stok mutation üretir.
- `GarageStockFlowSession` exact serialized item için `Arrived → Receiving → ActorHands → Shelf/WorldFloor` prototype composition'ını kurar.
- `InventoryItemWorldBinding` aynı Inventory item/world item kimliğini, `InventoryPlacementZone` ise doğrulanmış surface/container eşlemesini taşır.
- `PlayerCarryController` bound item'larda domain-first world mutation uygular; world failure domain rollback yapar, recovery authoritative container ve fiziksel pozu birlikte düzeltir.
- `DeliveryParcelProjection` kapalı dış parcel ile revealed ürün ve Receiving'de kalan açık kabuğu ayırır; opening accepted exact manifest/location doğrulaması sonrası idempotenttir ve domain revision/quantity değiştirmez.
- Aynı Interact binding'i acceptance → unpack → pickup olarak sıralanır; HUD/dünya panosu/prompt parcel durumunu klavye ve gamepad için dinamik gösterir.
- Exact ürün RAF A Shelf container'ındayken aynı Interact binding'i kasıtlı offer publish yapar; etiket authority başarısından önce `FİYAT YOK`, sonra `549,99 EUR` gösterir. Publish Inventory/Orders revision veya quantity değiştirmez.
- Fiyatlanmış RAF A ürününde `G / Gamepad East` exact item'ı demo customer basket için ayırır; available quantity `0`, total quantity `1` kalır. Etiket/pano reservation'ı gösterir, `E / Gamepad South` pickup fail-closed olur ve aynı `G / East` release sonrası available quantity `1`e döner.
- Ayrılmış RAF A ürününde `Mouse Left / Gamepad RT` bütün basket satırlarını exact offer/item/reservation ile preflight edip integer price/currency/total'i immutable checkout snapshot'ına alır. Sonraki offer update'i açık `549,99 EUR` fiyatını değiştirmez; checkout aktif release ve pickup fail-closed kalır.
- Aktif checkout'ta ikinci `Mouse Left / Gamepad RT`, exact reservation setini tek Inventory revision'ında atomik tüketir; Basket ve Checkout revision'larını birer kez ilerletir, stable fulfillment kaydı bırakır ve ürün projection'ını raftan kaldırır. Shelf/HUD `TAMAMLANDI`, stok/sepet/reservation `0`; exact tekrar idempotent, drift failure no-mutation'dır. Bu kayıt ödeme/Economy settlement değildir.
- `PSE.World` ve `PSE.Presentation` assembly sınırları.
- GarageGraybox sahnesi, connected `PlayerRig` prefabı ve CharacterController tabanlı birinci şahıs hareket.
- Klavye/fare + gamepad Input System sözleşmesi, runtime action izolasyonu ve rebind override store.
- FOV/hassasiyet/invert/motion-reduce ayarları, görünür prototip eller, pause/cursor ve runtime-ready tanısı.
- Canonical fiziksel ürün kimliği, 2 m hedef çözümleme, tek taşıma slotu, kinematic carry ve güvenli drop/recovery.
- `E / Gamepad South` ile alma, `G / Gamepad East` ile bırakma ve effective binding'i gösteren HUD prompt'u.
- `Mouse Left / Gamepad RT` ile kontrollü küçük-kutu placement modu; `G / Gamepad East` ile onay.
- İşaretli `PlacementSurface`, 0,25 m grid/90° yaw snap, tam taban/overlap doğrulaması, yeşil-kırmızı ghost + metin ve stabil kinematic placement.
- Ayrı büyük-kutu carry profili: turuncu bantlı graybox, iki-el pozu, `0,65×` hareket, sprint kilidi ve motion-safe `6°` istenen FOV bedeli.
- Büyük kutu `G / Gamepad East` ile gerçek boyutuna göre fail-closed güvenli bırakılır; etkin binding, ağır-yük ve engelli-drop durumu HUD prompt'unda görünür.
- Büyük-kutu placement girişi kapalıdır; stable ID, tek slot, physics snapshot ve disable/world-floor recovery korunur.
- Küçük kutu placement modunda `R / Right Shoulder` ile deterministik `90°` döner; etkin binding/açı promptu, ghost/confirm poz eşitliği ve döndürülmüş footprint güvenlik kontrolü vardır.
- Stable küçük kutu desteğinde merkez/90° snap, beş noktalı tam footprint, overlap engeli, tek kat/tek üst ilişkisi ve dolu taban pickup kilidi vardır; gerçek keyboard/mouse ve gamepad zinciri testlidir.
- Stable platform arabası tek `LargeBox` kabul eder; `E / Gamepad South` ile hands→cart→hands transferi, `Mouse Left / Gamepad RT` ile tut/bırak, 0,85× yüklü ve 0,90× boş hız, sprint kilidi ve dinamik prompt uygulanır.
- Araba hareketi dört noktalı zemin desteği, hedef overlap ve swept bounds obstruction kapılarından geçer; engelde son güvenli pozda kalır. Cart/controller disable yükü son güvenli dünya pozuna kurtarır.
- `GarageCustomerFlowRuntime`, runtime `NavMeshSurface` üzerinde explicit giriş/RAF A/checkout/çıkış anchor'larını izler; offer ziyareti başlatır, reservation checkout'a, fulfillment `Fulfilled` çıkışına götürür.
- Pause integer simulation clock ve NavMeshAgent'ı dondurur. Route/patience fallback'i Inventory/Retail/Orders revision'larını değiştirmez; terminal müşteri projection'ı güvenle gizlenir.
- Görsel hedef `ADR-0013`teki okunaklı yarı gerçekçiliktir. Mevcut primitive garaj, kutu ve eller final sanat değil; mekanik kanıttır.
- Tek-köşe benchmarkında bevel'lı tezgâh/raf, prosedürel PBR yüzeyler, görev ışığı, ACES/bloom ve reflection probe uygulanmıştır; runtime tanısı `lookdev=ok` verir.
- Güncel USB milestone `2026-08-15_STAGE_B_DETERMINISTIC_CUSTOMER_VISIT`: source/docs checkpoint `d163328`, 535 tracked kaynak, 5 Unity test/build/runtime kanıtı ve source kaydı; 541 satırlı `c82fc76d…cfd` SHA-256 manifest/readback ile doğrulandı. 535/535 Git-blob eşliği geçti; hash/boyut/path mismatch, forbidden/credential ve AppleDouble sayısı `0`dır; payload 9.715.834 bayttır.

Henüz yapılmayanlar:

- Gelişmiş el animasyonu, çok satırlı/çok adetli parcel unpack layout'u, çok katlı/palet istifi ve çoklu/palet taşıma.
- Garajın bütününe yayılmış final sanat ve gelişmiş el modeli/animasyonu.
- Orders'ın satış/servis varyantları, Economy ve diğer domain assembly'leri; Catalog/Inventory/Orders event-save bağlantısı.
- Sayısal fiyat düzenleme UI'si, açıklanabilir çoklu-offer müşteri kararı, ödeme/Economy settlement, gerçek fiziksel sepet transferi ve item/cart projection'ının daha geniş container tiplerine yayılması.
- Save/Guardian runtime.
- Steam entegrasyonu ve native Windows IL2CPP doğrulaması.

Sıradaki bounded paket Issue #9 altında tek customer + tek shelf offer için açıklanabilir ve deterministic satın-al/ayrıl değerlendirmesidir. Utility sonucu stable reason code taşımalı; Inventory, ödeme, Economy ledger/COGS/nakit, çoklu müşteri/ürün, derin danışmanlık, Save ve final karakter sanatı ayrı kalır.

## 7. Çalışma akışı

```bash
git switch main
git pull --ff-only
git switch -c feature/ISSUE-kisa-ad
```

Değişiklikten sonra:

1. Otomatik testleri çalıştırın.
2. `./Tools/verify-repository.sh` çalıştırın.
3. `PROJECT_BIBLE.md`, ADR/provenans/changelog gereksinimini değerlendirin.
4. Küçük, açıklayıcı commit oluşturun.
5. Branch'i push edip pull request açın; PR şablonunu eksiksiz doldurun.

## 8. Legacy sınırı

`LegacyReference/PC-Shop-Empire-1.1.6/Source` hash doğrulanmış tarihsel snapshot'tır. Doğrudan yeni Unity gameplay kodu olarak port edilmez ve normal feature PR'ında düzenlenmez.

Legacy'den alınabilecekler:

- Tema ve işletme niyeti.
- Dashboard bölüm anlamları.
- Veri alanlarının semantiği.
- İlerleme ve ekonomi tasarım soruları.

Doğrudan taşınmayacaklar:

- Electron/DOM uygulama mimarisi.
- Eski UI/CSS/görsel tasarımın kopyası.
- Gerçek marka/model verisi veya doğrulanmamış asset.
- Tek tuşla otomatik PC üretme davranışı.

## 9. Bir problemde önce kontrol edin

- Yanlış Unity sürümü mü?
- Paket lock değişmiş mi?
- `Library`/cache yanlışlıkla track edilmiş mi?
- Core assembly Unity referansı almış mı?
- Test raporu gerçekten oluşmuş mu, yoksa Editor erken mi kapandı?
- Git çalışma ağacı başka bir süreç tarafından değişiyor mu?
- Legacy manifest veya Project Bible kopyası ayrışmış mı?
- Secret/credential loga veya dosyaya yazılmış mı?

Sorunu düzeltmek için `main` history'sini force-push/reset etmeyin. Yeni branch, fix veya revert commit kullanın.

## 10. Devralma tamamlanma ölçütü

Yeni geliştirici şu beş şeyi gösterebildiğinde devir başarılıdır:

1. Projeyi clone edip doğru Unity sürümünde açtı.
2. Repo guard, 255 Edit Mode ve 18 Play Mode baseline testi geçti.
3. Vizyon ile vertical slice sınırını kendi cümlesiyle açıklayabildi.
4. GitHub Project'te sıradaki issue/acceptance kriterini buldu.
5. Küçük bir docs/test PR'ını yaşayan belge kurallarına uygun açabildi.
