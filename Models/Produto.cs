public class Produto //classe que representa um produto, com propriedades e dados
{
    public string Id { get; set; } = ""; //atributos da classe (ler e definir valor)
    public string CodigoBarras { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Marca { get; set; } = "";
    public decimal ValorCompra { get; set; } = 0;
    public decimal ValorRevenda { get; set; } = 0;
}