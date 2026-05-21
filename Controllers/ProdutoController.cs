using Microsoft.AspNetCore.Mvc;

public class ProdutoController : Controller //herança do controller; produto é uma entidade
{
    private DatabaseContext db; //atributo que apenas o controller acessa (pra acessar o bd)

    public ProdutoController(DatabaseContext db) //construtor - inj. de dep.: o controller recebe o bd como parâmetro, pra usar os dados do bd
    {
        this.db = db;
    }

    public ActionResult Index() //método do crud, READ - busca os produtos e manda pra view
    {
        ViewBag.ProdutoTamanhos = db.ProdutoTamanho.ToList(); // passa os tamanhos pra view
        return View(db.Produto.ToList());
    }

    [HttpGet]
    public ActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Create(Produto p, string[] Tamanhos, int[] Quantidades)
    {
        p.Id = Guid.NewGuid().ToString(); // gera id único pro produto
        db.Produto.Add(p); // salva o produto

        // percorre os tamanhos e quantidades e salva cada um
        for (int i = 0; i < Tamanhos.Length; i++)
        {
            if (!string.IsNullOrEmpty(Tamanhos[i])) // ignora campos vazios
            {
                var pt = new ProdutoTamanho
                {
                    Id = Guid.NewGuid().ToString(),
                    IdProduto = p.Id,
                    Tamanho = Tamanhos[i],
                    Quantidade = Quantidades[i]
                };
                db.ProdutoTamanho.Add(pt); // salva o tamanho
            }
        }

        db.SaveChanges(); // commit de tudo junto
        return RedirectToAction("Index");
    }

    public ActionResult Delete(string id) //método do crud, DELETE
    {
        // remove os tamanhos do produto antes de remover o produto
        var tamanhos = db.ProdutoTamanho.Where(pt => pt.IdProduto == id).ToList();
        db.ProdutoTamanho.RemoveRange(tamanhos);

        var produto = db.Produto.Single(p => p.Id == id);
        db.Produto.Remove(produto);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Update(string id) //método do crud, UPDATE
    {
        var produto = db.Produto.Single(p => p.Id == id);
        // passa os tamanhos do produto pra view
        ViewBag.Tamanhos = db.ProdutoTamanho.Where(pt => pt.IdProduto == id).ToList();
        return View(produto);
    }

    [HttpPost]
    public ActionResult Update(Produto p, string[] Tamanhos, int[] Quantidades, string[] TamanhoIds)
    {
        db.Produto.Update(p); // atualiza os dados do produto

        // atualiza os tamanhos existentes
        for (int i = 0; i < TamanhoIds.Length; i++)
        {
            if (!string.IsNullOrEmpty(TamanhoIds[i]))
            {
                var pt = db.ProdutoTamanho.Single(x => x.Id == TamanhoIds[i]);
                pt.Tamanho = Tamanhos[i];
                pt.Quantidade = Quantidades[i];
                db.ProdutoTamanho.Update(pt);
            }
        }

        db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Movimentar()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Movimentar(string codigoBarras, string tamanho, int quantidade, string tipo)
    {
        var produto = db.Produto.SingleOrDefault(p => p.CodigoBarras == codigoBarras);

        if (produto == null)
        {
            TempData["Erro"] = "Produto não encontrado. Verifique o código de barras.";
            return RedirectToAction("Movimentar");
        }

        var produtoTamanho = db.ProdutoTamanho
            .SingleOrDefault(pt => pt.IdProduto == produto.Id && pt.Tamanho == tamanho);

        // se tamanho não existe e é entrada, cria automaticamente
        if (produtoTamanho == null && tipo == "entrada")
        {
            produtoTamanho = new ProdutoTamanho
            {
                Id = Guid.NewGuid().ToString(),
                IdProduto = produto.Id,
                Tamanho = tamanho,
                Quantidade = quantidade
            };
            db.ProdutoTamanho.Add(produtoTamanho);
            db.SaveChanges();
            TempData["Sucesso"] = $"Tamanho {tamanho} criado! {produto.Nome} agora tem {quantidade} unidades.";
            return RedirectToAction("Movimentar");
        }

        if (produtoTamanho == null && tipo == "saida")
        {
            TempData["Erro"] = "Tamanho não encontrado para esse produto.";
            return RedirectToAction("Movimentar");
        }

        if (tipo == "saida" && quantidade > produtoTamanho.Quantidade)
        {
            TempData["Erro"] = $"Quantidade insuficiente. Estoque atual: {produtoTamanho.Quantidade} unidades.";
            return RedirectToAction("Movimentar");
        }

        if (tipo == "entrada")
            produtoTamanho.Quantidade += quantidade;
        else if (tipo == "saida")
            produtoTamanho.Quantidade -= quantidade;

        db.ProdutoTamanho.Update(produtoTamanho);
        db.SaveChanges();

        TempData["Sucesso"] = $"Estoque atualizado! {produto.Nome} tamanho {tamanho} agora tem {produtoTamanho.Quantidade} unidades.";
        return RedirectToAction("Movimentar");
    }
}