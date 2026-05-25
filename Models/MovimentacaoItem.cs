public class MovimentacaoItem
{
    public string Id { get; set; } = "";
    public string IdMovimentacao { get; set; } = ""; // FK → Movimentacao (N:1)
    public string IdProduto { get; set; } = "";      // FK → Produto (N:1)
    public string Tamanho { get; set; } = "";
    public int Quantidade { get; set; } = 0;
}