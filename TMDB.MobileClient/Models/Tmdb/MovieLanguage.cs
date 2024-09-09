using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TMDB.MobileClient.Models.Tmdb;

public partial class MovieLanguage
{
    [JsonPropertyName("iso_639_1")]
    public string? Iso { get; set; }

    [JsonPropertyName("english_name")]
    public string? EnglishName { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }
}