# ADR-0005 — Deterministik zaman ve alan olayı zarfı

**Durum:** Kabul edildi ve uygulandı

**Tarih:** 11 Ağustos 2026

## Karar

- Alan sistemleri işletim sistemi saatini, `DateTime.Now` değerini veya Unity frame zamanını doğrudan okumaz; yalnız `ISimulationClock` üzerinden monotonik oyun zamanı görür.
- `SimulationTimestamp`, başlangıçtan itibaren negatif olmayan sabit-adım numarası ile integer oyun milisaniyesini birlikte taşır. Floating-point zaman ve saat dilimi içermez.
- `SimulationClock` yalnız açık `Advance` çağrısıyla bir adım ilerler. Sıfır süre, pause veya taşma durumunda saat değişmeden makine-okunur failure döner.
- Dashboard pause durumunda tick ve oyun süresi ilerlemez. Resume aynı clock kaynağından devam eder.
- Her kalıcı alan olayı; stable event ID, stable event type, bir-tabanlı journal sequence, simulation timestamp, pozitif schema version ve null olmayan payload taşıyan immutable `DomainEventEnvelope<TEvent>` sınıfına alınır. Böylece değer tipinin sessiz `default` örneği invariant'ları atlayamaz.
- Event ID duplicate/idempotence kontrolü, sequence journal sırası, timestamp oyun içi nedensel inceleme içindir. Bunlar wall-clock telemetry zamanı değildir.

## Bilinçli sınırlar

- Unity fixed-update adaptörü, hız çarpanı, gün/takvim dönüşümü, seed'li RNG, event bus, serializer, correlation/causation ve journal kalıcılığı sonraki paketlerindir.
- Timestamp iki eksende de geriye gidemez; farklı tick ile süreyi keyfi biçimde yeniden sıralayan lexicographic karşılaştırma sunulmaz.
- Bu paket gameplay, sahne, ekonomi kuralı veya ağ determinismi vaadi eklemez; saf `PSE.Core` sınırında kalır.
