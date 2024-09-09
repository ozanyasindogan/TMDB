using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace TMDB.MobileClient.Models.Tmdb
{
    public partial class MovieGenres
    {
        [JsonPropertyName("genres")]
        public List<Genre>? Genres { get; set; }
    }

    public partial class Genre
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}
