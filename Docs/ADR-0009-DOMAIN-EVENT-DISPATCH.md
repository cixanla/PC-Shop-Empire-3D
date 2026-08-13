# ADR-0009 — Domain event correlation ve deterministik in-memory dispatch

**Durum:** Kabul edildi ve uygulandı<br>
**Tarih:** 13 Ağustos 2026

## Bağlam

Stok, para, sipariş, müşteri ve fiziksel dünya projeksiyonları aynı işlemi iki kez uygulamamalı; bir olayın hangi işleme ve doğrudan hangi ebeveyne ait olduğu açıklanabilmelidir. Recursive veya sıra garantisi olmayan bir event bus; save replay, Guardian raporu ve ekonomi invariant'larını belirsizleştirir.

## Karar

### Zarf bağlamı

- Her `DomainEventEnvelope<TEvent>` zorunlu `DomainEventContext` taşır.
- `CorrelationId` bir komut/transaction zincirini tanımlar; root olayda bile zorunludur.
- Root olayın `CausationId` değeri boştur. Çocuk olay correlation kimliğini ebeveynden miras alır ve doğrudan ebeveyn `EventId` değerini causation olarak taşır.
- Self-causation reddedilir. Dispatcher geçmiş ebeveyn varlığını veya tam DAG doğruluğunu kanıtlamaz; bu journal/Guardian sorumluluğudur.
- Generic zarf, journal ve tanılama için immutable `IDomainEventEnvelope` görünümü sağlar. Payload generic contract type ile tam eşleşmelidir; polymorphic dispatch yoktur.

### Dispatcher

- `InMemoryDomainEventDispatcher` tek simülasyon thread'i içindir; thread-safe olduğu iddia edilmez.
- Handler sırası registration sırası; event sırası bütün türler arasında global FIFO ve one-based journal sequence sırasıdır.
- Restore constructor'ı son committed sequence değerini alır; yalnız `last + 1` kabul edilir. Gap, reverse ve sequence exhaustion açık failure üretir.
- Handler içinden `Enqueue` kuyruğun sonuna eklenir. Önce mevcut olayın bütün handler'ları, sonra child olay çalışır; recursive dispatch yapılmaz.
- Her `Drain` pozitif `maxEvents` bütçesi ister. Reentrant `Drain` reddedilir; benzersiz sonsuz event zinciri tek turda oyunu donduramaz.
- Registration ilk drain ile kilitlenir. Unsubscribe, priority, async, reflection, `DynamicInvoke` ve çok-thread desteği bu pakette yoktur.

### Duplicate ve hata politikası

- Aynı Event ID ve aynı type/sequence/time/schema/context/payload-contract metadata'sı process içinde idempotent duplicate sayılır; ikinci kez kuyruğa veya handler'a girmez.
- Aynı Event ID farklı metadata ile `events.enqueue.duplicate-conflict` üretir.
- Bu pakette payload reflection/fingerprint karşılaştırması yapılmaz; kalıcı payload hash ve hydrate edilen receipt ledger save/journal paketine aittir.
- Handler'ın döndürdüğü failure veya fatal olmayan exception rapora eklenir; sonraki handler ve event devam eder.
- Handler başlamışsa event tüketilmiş sayılır ve otomatik retry yapılmaz. Kısmi side effect riski compensation/recovery olayıyla çözülür; dispatcher rollback sağlamaz.
- Fatal process hataları için devam garantisi yoktur.
- Rapor handler/event/type/sequence/correlation/causation, failure code ve exception type taşır. Raw payload, exception mesajı ve stack trace içermez; ayrıntılı redakte Guardian raporu ayrı katmandır.
- Handler'ı olmayan event de tüketilir ve sequence ilerler.

## Kanıt kapıları

Edit Mode paketi aşağıdakileri kilitler:

- Root/child correlation ve causation.
- Registration sırası, exact type ve global FIFO.
- Nested breadth-first sıra ve 1.000 olaylık bounded zincir.
- Restore cursor, gap/reverse/exhaustion.
- Idempotent duplicate ve metadata conflict.
- Failure/exception izolasyonu, no-retry ve reentrant drain.
- Handler'sız event tüketimi, registration lock ve iki eşdeğer çalıştırmada eşdeğer rapor.

## Bilinçli teknik borç

- Process-lifetime receipt sözlüğü sessiz eviction yapmaz ve uzun oturumda büyür. Kalıcı journal/receipt ledger, restore hydration ve bounded retention Issue #11 save altyapısında tasarlanacaktır.
- Dispatcher state'i tek başına save değildir. İşlemsel para/stok değişimi event yayınlanmadan önce atomik tamamlanmalıdır.
- Async iş, network replication ve cross-process queue yoktur.
