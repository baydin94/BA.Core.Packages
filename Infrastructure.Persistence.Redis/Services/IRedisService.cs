using StackExchange.Redis;

namespace Infrastructure.Persistence.Redis.Services;

/// <summary>
/// Ham Redis işlemleri ve tenant bazlı JSON kayıtları için erişim sağlar.
/// </summary>
public interface IRedisService
{
    /// <summary>
    /// Yapılandırılmış varsayılan Redis veritabanına erişim sağlar.
    /// </summary>
    /// <returns>Redis komutlarını çalıştırmak için kullanılan veritabanı nesnesi.</returns>
    IDatabase GetDatabase();

    /// <summary>
    /// Belirtilen Redis anahtarını siler.
    /// </summary>
    /// <param name="key">Silinecek tam Redis anahtarı.</param>
    /// <returns>Anahtar silindiyse <c>true</c>, mevcut değilse <c>false</c>.</returns>
    Task<bool> KeyDeleteAsync(RedisKey key);

    /// <summary>
    /// Belirtilen Redis anahtarının mevcut olup olmadığını kontrol eder.
    /// </summary>
    /// <param name="key">Kontrol edilecek tam Redis anahtarı.</param>
    /// <returns>Anahtar mevcutsa <c>true</c>, aksi hâlde <c>false</c>.</returns>
    Task<bool> KeyExistsAsync(RedisKey key);

    /// <summary>
    /// Verilen değeri belirtilen anahtara Redis string olarak yazar.
    /// </summary>
    /// <param name="key">Yazılacak tam Redis anahtarı.</param>
    /// <param name="value">Doğrudan Redis'e yazılacak değer.</param>
    /// <param name="expiration">Kayıt için uygulanacak sona erme ayarı.</param>
    /// <param name="when">Yazmanın hangi koşulda yapılacağını belirler.</param>
    /// <param name="flags">Redis komutunun çalıştırılma seçenekleri.</param>
    /// <returns>
    /// Sunucu yanıtı bekleniyorsa, yazma gerçekleştiğinde <c>true</c>,
    /// yazma koşulu sağlanmadığında <c>false</c>.
    /// </returns>
    /// <remarks>
    /// Değer JSON'a dönüştürülmeden yazılır. <see cref="CommandFlags.FireAndForget"/>
    /// kullanıldığında dönüş değeri yazmanın gerçekleştiğini doğrulamaz.
    /// </remarks>
    Task<bool> StringSetAsync(
        RedisKey key,
        RedisValue value,
        Expiration expiration = default,
        When when = When.Always,
        CommandFlags flags = CommandFlags.None);

    /// <summary>
    /// Belirtilen anahtardaki ham Redis string değerini okur.
    /// </summary>
    /// <param name="key">Okunacak tam Redis anahtarı.</param>
    /// <returns>Kayıt varsa ham değer, anahtar yoksa <see cref="RedisValue.Null"/>.</returns>
    Task<RedisValue> StringGetAsync(RedisKey key);

    /// <summary>
    /// Verilen değeri JSON'a dönüştürüp <c>tenantId:group:key</c> anahtarına yazar.
    /// </summary>
    /// <typeparam name="T">JSON'a dönüştürülecek değerin türü.</typeparam>
    /// <param name="tenantId">Kaydın ait olduğu tenant kimliği.</param>
    /// <param name="group">Kaydın ait olduğu grup.</param>
    /// <param name="key">Grup içindeki kayıt anahtarı.</param>
    /// <param name="value">JSON olarak saklanacak değer.</param>
    /// <param name="expiration">Kayıt için uygulanacak sona erme ayarı.</param>
    /// <param name="when">Yazmanın hangi koşulda yapılacağını belirler.</param>
    /// <param name="flags">Redis komutunun çalıştırılma seçenekleri.</param>
    /// <returns>
    /// Sunucu yanıtı bekleniyorsa, yazma gerçekleştiğinde <c>true</c>,
    /// yazma koşulu sağlanmadığında <c>false</c>.
    /// </returns>
    /// <remarks>
    /// JSON metni verilirse o metin bir string olarak JSON'a dönüştürülür.
    /// Hazır JSON'u doğrudan saklamak için <see cref="StringSetAsync"/> kullanılır.
    /// <see cref="CommandFlags.FireAndForget"/> kullanıldığında dönüş değeri
    /// yazmanın gerçekleştiğini doğrulamaz.
    /// </remarks>
    Task<bool> SetAsync<T>(
        string tenantId,
        string group,
        string key,
        T value,
        Expiration expiration = default,
        When when = When.Always,
        CommandFlags flags = CommandFlags.None);

    /// <summary>
    /// <c>tenantId:group:key</c> anahtarındaki JSON kaydını belirtilen türe dönüştürür.
    /// </summary>
    /// <typeparam name="T">JSON kaydının dönüştürüleceği tür.</typeparam>
    /// <param name="tenantId">Kaydın ait olduğu tenant kimliği.</param>
    /// <param name="group">Kaydın ait olduğu grup.</param>
    /// <param name="key">Grup içindeki kayıt anahtarı.</param>
    /// <returns>Dönüştürülen değer. Kayıt bulunamazsa <c>default(T)</c>.</returns>
    /// <exception cref="System.Text.Json.JsonException">
    /// Saklanan değer geçerli JSON değilse veya <typeparamref name="T"/> türüne dönüştürülemiyorsa.
    /// </exception>
    /// <remarks>
    /// Bu metot okuma sırasında kaydın süresini yenilemez.
    /// Değer türlerinde eksik kaydı ayırt etmek için nullable tür kullanılabilir.
    /// Örneğin <c>GetAsync&lt;int?&gt;</c> ile eksik kayıt <c>null</c> döner.
    /// </remarks>
    Task<T?> GetAsync<T>(string tenantId, string group, string key);

    /// <summary>
    /// Seçilen Redis sunucusunda <c>tenantId:group:*</c> desenine uyan anahtarları
    /// tarar ve bulduğu anahtarları tek tek siler.
    /// </summary>
    /// <param name="tenantId">Grubun ait olduğu tenant kimliği.</param>
    /// <param name="group">Kayıtları silinecek grup.</param>
    /// <remarks>
    /// Silme işlemi atomik değildir. Tarama sırasında eklenen kayıtlar kalabilir.
    /// Mevcut uygulama taramayı bağlantı fabrikasının seçtiği tek sunucuda yapar.
    /// </remarks>
    Task RemoveGroupAsync(string tenantId, string group);
}