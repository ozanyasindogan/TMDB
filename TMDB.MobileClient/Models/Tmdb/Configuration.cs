using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TMDB.MobileClient.Models.Tmdb;

public class Configuration
{
    [JsonPropertyName("images")]
    public Images? Images { get; set; }

    [JsonPropertyName("change_keys")]
    public List<string>? ChangeKeys { get; set; }
}

public class Images
{
    [JsonPropertyName("base_url")]
    public string? BaseUrl { get; set; }

    [JsonPropertyName("secure_base_url")]
    public string? SecureBaseUrl { get; set; }

    [JsonPropertyName("backdrop_sizes")]
    public List<string>? BackdropSizes { get; set; }

    [JsonPropertyName("logo_sizes")]
    public List<string>? LogoSizes { get; set; }

    [JsonPropertyName("poster_sizes")]
    public List<string>? PosterSizes { get; set; }

    [JsonPropertyName("profile_sizes")]
    public List<string>? ProfileSizes { get; set; }

    [JsonPropertyName("still_sizes")]
    public List<string>? StillSizes { get; set; }
}