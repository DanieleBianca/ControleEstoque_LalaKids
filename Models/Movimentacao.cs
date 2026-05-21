public class Movimentacao
{
    public string Id { get; set; } = "";
    public string IdProduto { get; set; } = ""; // FK → Produto (1:N)
    public string IdUsuario { get; set; } = ""; // FK → Usuario (1:N)
    public string Tamanho { get; set; } = "";
    public int Quantidade { get; set; } = 0;
    public string Tipo { get; set; } = ""; // "entrada" ou "saida"
    public DateTime Data { get; set; } = DateTime.Now; // data automática
}