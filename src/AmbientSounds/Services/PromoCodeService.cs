using AmbientSounds.Models;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmbientSounds.Services;

public class PromoCodeService : IPromoCodeService
{
    private readonly string _promoCodeBaseUrl;
    private readonly HttpClient _client;

    public PromoCodeService(
        IAppSettings appSettings,
        HttpClient httpClient)
    {
        _promoCodeBaseUrl = appSettings.PromoCodesUrl;
        _client = httpClient;
    }

    /// <inheritdoc/>
    public async Task<string?> TryGetAddOnAsync(string promoCode)
    {
        if (promoCode is not { Length: > 0 })
        {
            return null;
        }

        string url = $"{_promoCodeBaseUrl}/{promoCode}";

        try
        {
            using Stream result = await _client.GetStreamAsync(url).ConfigureAwait(false);
            PromoCode? code = await JsonSerializer.DeserializeAsync(
                result,
                AmbieJsonSerializerContext.CaseInsensitive.PromoCode);
            return code?.MicrosoftStoreAddOnId;
        }
        catch
        {
            return null;
        }
    }
}
