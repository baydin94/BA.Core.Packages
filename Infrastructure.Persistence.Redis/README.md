# Redis önbellek kütüphanesi

Bu .NET paketi Redis bağlantısını ve `ICacheService` implementasyonunu sağlar.
Aşağıdaki örnekler kütüphaneyi kullanan uygulamaya aittir.

## Yapılandırma

Tüketici uygulama kendi `appsettings.json` dosyasına bir bölüm ekler.

```json
{
  "Redis": {
    "Host": "localhost",
    "Port": 6379,
    "InstanceName": "catalog-development",
    "Database": 0,
    "Ssl": false
  },
  "Cache": {
    "AbsoluteExpiration": "00:05:00"
  }
}
```

Cluster için aynı kümenin birden fazla başlangıç adresini verebilirsiniz.
İstemci, ilk düğüm kapalıyken erişilebilir bir adresten topolojiyi keşfeder.

```json
{
  "Redis": {
    "Endpoints": ["redis-a.internal:6379", "redis-b.internal:6379"],
    "InstanceName": "catalog-production",
    "Database": 0,
    "Ssl": true
  }
}
```

Dolu bir `Endpoints` listesi önceliklidir. Liste boşsa veya verilmezse
`Host` ve `Port` kullanılır. Her adreste portu belirtin. IPv6 adreslerini
`[::1]:6379` biçiminde yazın. TLS kullanırken adresleri sunucu
sertifikasındaki adlarla eşleştirin.

Her uygulama ve ortam için ayrı bir `InstanceName` seçin.
`Ssl` bağlantı şifrelemesini belirler. Üretimde TLS'i etkinleştirin.
Kimlik doğrulaması gerekiyorsa `Redis__User` ve `Redis__Password` değerlerini
host'un güvenli yapılandırma kaynağından verin.

Gruplu cache işlemleri Redis üzerinde Lua script çalıştırır. Kısıtlı bir
Redis kullanıcısı tanımlarken script çalıştırma ve scriptin kullandığı
`GET`, `SET`, `DEL`, `HGET`, `HSET`, `PTTL`, `PEXPIRE` komutlarına
erişim verin. Cluster kullanıyorsanız `Database` değerini sıfır seçin.

## Servis kaydı

Örnekte `services` tüketici uygulamanın `IServiceCollection`,
`configuration` ise `IConfiguration` nesnesidir.

```csharp
using Infrastructure.Persistence.Redis;

services.AddRedisPersistenceServices(configuration);
```

MediatR pipeline'ını kullanan uygulama ayrıca `Core.Application` paketini
ekler. Mevcut `AddMediatR` kaydına aşağıdaki davranışı dahil eder.

```csharp
using Core.Application.Pipelines.Caching;

services.Configure<CacheSettings>(configuration.GetSection("Cache"));

services.AddMediatR(config =>
{
    config.RegisterServicesFromAssemblyContaining<GetProduct>();
    // Yetkilendirme ve doğrulama davranışlarını önce kaydedin.
    config.AddOpenBehavior(typeof(CachingBehavior<,>));
});
```

## Sorgu örneği

```csharp
using Core.Abstractions.Applications.Caching;
using MediatR;

public sealed record ProductDto(int Id, string Name);

public sealed record GetProduct(string TenantId, int Id)
    : IRequest<ProductDto?>, ICacheableRequest
{
    public CacheAddress CacheAddress => new(TenantId, "products", $"detail:v1:{Id}");
    public TimeSpan? AbsoluteExpiration => TimeSpan.FromMinutes(2);
}
```

Bu örnekte tüketici, sorguyu yetkilendirmeden sonra oluşturur.
`TenantId` doğrulanmış tenant bağlamından gelir. Yanıt kullanıcının yetkisine,
diline, filtrelere veya sayfaya göre değişiyorsa ilgili değerleri anahtara
katın. Yanıt şeması değiştiğinde anahtar sürümünü artırın.

Süre yazıldığı anda başlar. Okuma süreyi uzatmaz. Sorgu süre belirtmezse
`Cache:AbsoluteExpiration` kullanılır. Paket varsayılanı beş dakikadır.
`BypassCache` okuma ve yazmayı birlikte atlar. Transaction istekleri ve
aktif bir `TransactionScope` içinde çağrılan sorgular doğrudan handler'a
gider. Eski `CacheKey` ve `SlidingExpiration` üyelerini
kullanan sorguların cache'e katılması için `CacheAddress` tanımlayın.

## Doğrudan kullanım ve geçersiz kılma

```csharp
var address = new CacheAddress(tenantId, "products", $"detail:v1:{id}");
var result = await cache.GetAsync<ProductDto?>(address, cancellationToken);
if (result.IsHit)
    return result.Value; // Saklanan null da geçerli bir sonuçtur.
```

`cache` bağımlılığı `ICacheService` türündedir. Doğrudan yazma ve silme
aynı adresi kullanır.

```csharp
await cache.SetAsync(address, product, TimeSpan.FromMinutes(2), cancellationToken);
await cache.RemoveAsync(address, cancellationToken);
```

Ürün güncellemesini kalıcı depoya başarıyla commit ettikten sonra grubun
liste ve detay sonuçlarını birlikte geçersiz kılabilirsiniz.

```csharp
await cache.RemoveGroupAsync(tenantId, "products", cancellationToken);
```

Grup geçersiz kılındığında önceki grup sürümünde başlamış bir pipeline
okuması eski sonucu yeni sürüme yazamaz. Tek kayıt silme yalnızca o anda
saklanan kaydı kaldırır. Eşzamanlı bir sorgunun eski veriyi geri yazmasını
engellemek gerektiğinde grup geçersiz kılmayı kullanın.

Veritabanı commit'i ile Redis işlemi ayrı işlemlerdir. Geçersiz kılma
başarısız olursa hatayı çağıran alır. Teslim garantisi gereken uygulamalarda
yeniden denemeyi kalıcı bir iş veya outbox üzerinden yönetin.

## Hata davranışı

Pipeline, Redis bağlantı veya zaman aşımı hatasında sorgu handler'ını
çalıştırır. Yazma sırasında aynı hata oluşursa handler'ın sonucunu döner.
Handler hataları, iptal ve serileştirme hataları çağırana iletilir.
Doğrudan cache çağrılarında bağlantı kesintisi
`CacheUnavailableException` olarak bildirilir.

## Mevcut tüketiciler

`ICacheService` için kendi sağlayıcısını yazmış uygulamalar yeni adresli
okuma, yazma ve silme metotlarını da uygulamalıdır. Redis'e özgü düşük
seviyeli işlemler için `IRedisService` kullanılabilir. Gruplu uygulama
önbelleği kendi anahtar alanını yönetir. Önceden fiziksel
`tenant:group:key` biçiminde yazılmış JSON kayıtları `IRedisService`
üzerinden okunabilir. Yeni cache alanı ilk sorguda yeniden doldurulur.
Eski ve yeni uygulama sürümleri birlikte çalışıyorsa geçiş boyunca
iki alan için de geçersiz kılma planlayın.
