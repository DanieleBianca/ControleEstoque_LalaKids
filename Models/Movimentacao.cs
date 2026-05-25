public class Movimentacao
{
    public string Id { get; set; } = "";
    public string IdUsuario { get; set; } = ""; // FK → Usuario (1:N)
    public string Tipo { get; set; } = ""; // "entrada" ou "saida"
    public DateTime Data { get; set; } = DateTime.Now; // data automática
}