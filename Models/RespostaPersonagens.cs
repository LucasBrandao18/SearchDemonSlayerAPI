using System.Text.Json.Serialization;

namespace SearchDemonSlayerAPI.Models;

public class RespostaPersonagens
{
    [JsonPropertyName("pagination")]
    public Paginacao? Paginacao { get; set; }

    [JsonPropertyName("content")]
    public List<Personagem?>? Personagens { get; set; }
}

public class Paginacao
{
    [JsonPropertyName("currentPage")]
    public int PaginaAtual { get; set; }

    [JsonPropertyName("totalPages")]
    public int TotalPaginas { get; set; }

    [JsonPropertyName("totalElements")]
    public int TotalPersonagens { get; set; }
}

public class Personagem
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Nome { get; set; }

    [JsonPropertyName("age")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public int? Idade { get; set; }

    [JsonPropertyName("race")]
    public string? Raca { get; set; }

    [JsonPropertyName("quote")]
    public string? Citacao { get; set; }

    [JsonPropertyName("img")]
    public string? Imagem { get; set; }
}
