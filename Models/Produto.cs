public class Produto //classe que representa um produto, com propriedades e dados
{
    public string Id { get; set; }  //atributos da classe (ler e definir valor)
    public string CodigoBarras { get; set; }
    public string Nome { get; set; }
    public string Descricao { get; set; }
    public string Tamanho { get; set; }
    public decimal ValorCompra { get; set; }
    public decimal ValorRevenda { get; set; }
    public int Quantidade { get; set; }
}