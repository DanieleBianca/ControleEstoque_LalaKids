using Microsoft.AspNetCore.Mvc;

public class ProdutoController : Controller //herança do controller; produto é uma entidade
{
    private DatabaseContext db; //atributo que apenas o controller acessa (pra acessar o bd)

    public ProdutoController(DatabaseContext db) //construtor - inj. de dep.: o controller recebe o bd como parâmetro, pra usar os dados do bd
    {
        this.db = db;
    }

    public ActionResult Index(string busca) //método do crud, READ - busca os produtos e manda pra view
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");  // apenas usuarios logados acessam
        ViewBag.ProdutoTamanhos = db.ProdutoTamanho.ToList(); // passa os tamanhos pra view
        ViewBag.Busca = busca;

        var produtos = db.Produto.ToList();
        if (!string. IsNullOrEmpty(busca)) 
        {
            produtos = produtos.Where(p =>
                p.Nome.Contains(busca, StringComparison.OrdinalIgnoreCase) ||
                p.CodigoBarras.Contains(busca, StringComparison.OrdinalIgnoreCase)
            ).ToList();
        }


    return View(produtos);
}
    

    private bool UsuarioLogado()
    {
        return HttpContext.Session.GetString("UsuarioId") != null;
    }

    [HttpGet]
    public ActionResult Create()
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
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
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
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
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
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
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        return View();
    }

    [HttpPost]
    public ActionResult Movimentar(string codigoBarras, string tamanho, int quantidade, string tipo)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
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
        }
        else if (produtoTamanho == null && tipo == "saida")
        {
            TempData["Erro"] = "Tamanho não encontrado para esse produto.";
            return RedirectToAction("Movimentar");
        }
        else
        {
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
        }

        var idUsuario = HttpContext.Session.GetString("UsuarioId") ?? "";

        // salva o histórico da movimentação
        var movimentacao = new Movimentacao
        {
            Id = Guid.NewGuid().ToString(),
            IdUsuario = idUsuario,  //que usuario movimentou
            IdProduto = produto.Id, // FK → Produto (relacionamento 1:N)
            Tamanho = tamanho,
            Quantidade = quantidade,
            Tipo = tipo,
            Data = DateTime.Now
        };
        db.Movimentacao.Add(movimentacao);
        db.SaveChanges();

        TempData["Sucesso"] = $"Estoque atualizado! {produto.Nome} tamanho {tamanho}.";
        return RedirectToAction("Movimentar");
    }

    // abre a tela de seleção de relatórios
    public ActionResult Relatorios()
    {
        return View();
    }

    // relatório: produtos que mais saem
    public ActionResult RelatorioProdutosMaisVendidos()
    {
        // agrupa as movimentações de saída por produto e soma as quantidades
        // isso é LINQ avançado: GroupBy agrupa, Sum soma, OrderByDescending ordena do maior pro menor
        var resultado = db.Movimentacao
            .Where(m => m.Tipo == "saida") // filtra só saídas
            .GroupBy(m => m.IdProduto) // agrupa por produto
            .Select(g => new {
                IdProduto = g.Key,
                TotalSaidas = g.Sum(m => m.Quantidade) // soma total de saídas
            })
            .OrderByDescending(x => x.TotalSaidas) // mais vendidos primeiro
            .ToList();

        ViewBag.Produtos = db.Produto.ToList();
        return View(resultado);
    }

    // relatório: estoque baixo (tamanhos com quantidade menor ou igual a 2)
    public ActionResult RelatorioEstoqueBaixo()
    {
        // busca tamanhos com quantidade baixa
        var tamanhosBaixos = db.ProdutoTamanho
            .Where(pt => pt.Quantidade <= 2) // limite de estoque baixo
            .OrderBy(pt => pt.Quantidade) // os mais críticos primeiro
            .ToList();

        ViewBag.Produtos = db.Produto.ToList();
        return View(tamanhosBaixos);
    }

    // relatório: histórico por produto — recebe o id do produto pela URL
    public ActionResult RelatorioHistoricoPorProduto(string id)
    {
        // se não recebeu id, mostra lista de produtos para escolher
        if (string.IsNullOrEmpty(id))
        {
            return View("SelecionarProduto", db.Produto.ToList());
        }

        var produto = db.Produto.Single(p => p.Id == id);

        // busca todas as movimentações desse produto específico — relacionamento 1:N
        var movimentacoes = db.Movimentacao
            .Where(m => m.IdProduto == id)
            .OrderByDescending(m => m.Data)
            .ToList();

        ViewBag.Produto = produto;
        return View(movimentacoes);
    }
}