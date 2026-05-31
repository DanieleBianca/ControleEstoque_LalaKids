public class ProdutoTamanho
{
    public string Id { get; set; } = ""; // id único do registro
    public string IdProduto { get; set; } = ""; // FK → Produto
    public string Tamanho { get; set; } = ""; // ex: 0, 2, 4,
    public int Quantidade { get; set; } = 0; // quantidade disponível desse tamanho
}