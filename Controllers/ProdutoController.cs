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

        // ORDENA ALFABETICAMENTE PELO NOME
        var produtos = db.Produto
            .OrderBy(p => p.Nome)
            .ToList();
            
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
        // verifica se já existe produto com esse código de barras
        var existe = db.Produto.Any(x => x.CodigoBarras == p.CodigoBarras);
        if (existe)
        {
            TempData["Erro"] = "Código de barras já cadastrado";
            return RedirectToAction("Create");
        }

        if (p.ValorCompra < 0 || p.ValorRevenda < 0)
        {
            TempData["Erro"] = "Valores de compra e revenda não podem ser negativos";
            return RedirectToAction("Create");
        }

        if (Quantidades.Any(q => q < 0))
        {
            TempData["Erro"] = "Quantidade não pode ser negativa";
            return RedirectToAction("Create");
        }

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
    if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");

    //impede editar valores para negativo
    if (p.ValorCompra < 0 || p.ValorRevenda < 0)
    {
        TempData["Erro"] = "Valores de compra e revenda não podem ser negativos";
        return RedirectToAction("Update", new { id = p.Id });
    }

    if (Quantidades.Any(q => q < 0))
    {
        TempData["Erro"] = "Quantidade não pode ser negativa";
        return RedirectToAction("Update", new { id = p.Id });
    }

    db.Produto.Update(p);

    for (int i = 0; i < TamanhoIds.Length; i++)
    {
        if (!string.IsNullOrEmpty(TamanhoIds[i]))
        {
            // tamanho já existe — atualiza
            var pt = db.ProdutoTamanho.Single(x => x.Id == TamanhoIds[i]);
            pt.Tamanho = Tamanhos[i];
            pt.Quantidade = Quantidades[i];
            db.ProdutoTamanho.Update(pt);
        }
        else if (!string.IsNullOrEmpty(Tamanhos[i]))
        {
            // tamanho novo — cria
            var pt = new ProdutoTamanho
            {
                Id = Guid.NewGuid().ToString(),
                IdProduto = p.Id,
                Tamanho = Tamanhos[i],
                Quantidade = Quantidades[i]
            };
            db.ProdutoTamanho.Add(pt);
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
    public ActionResult Movimentar(string[] CodigosBarras, string[] Tamanhos, int[] Quantidades, string tipo)
    {
    if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");

    var idUsuario = HttpContext.Session.GetString("UsuarioId") ?? "";
    var erros = new List<string>();

    var movimentacao = new Movimentacao
    {
        Id = Guid.NewGuid().ToString(),
        IdUsuario = idUsuario,
        Tipo = tipo,
        Data = DateTime.Now
    };
    db.Movimentacao.Add(movimentacao);

    for (int i = 0; i < CodigosBarras.Length; i++)
    {
        if (string.IsNullOrEmpty(CodigosBarras[i])) continue;

        var codigoBarras = CodigosBarras[i];
        var tamanho = Tamanhos[i];
        var quantidade = Quantidades[i];

        var produto = db.Produto.FirstOrDefault(p => p.CodigoBarras == codigoBarras);

        if (produto == null)
        {
            erros.Add($"Produto '{codigoBarras}' não encontrado");
            continue;
        }

        var produtoTamanho = db.ProdutoTamanho
            .FirstOrDefault(pt => pt.IdProduto == produto.Id && pt.Tamanho == tamanho);

        if (produtoTamanho != null && tipo == "entrada")
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
            erros.Add($"Tamanho '{tamanho}' não encontrado em '{produto.Nome}'");
            continue;
        }
        else
        {
            if (tipo == "saida" && quantidade > produtoTamanho.Quantidade)
            {
                erros.Add($"Estoque insuficiente para '{produto.Nome}' no tamanho {tamanho}. Disponível: {produtoTamanho.Quantidade}");
                continue;
            }

            if (tipo == "entrada")
                produtoTamanho.Quantidade += quantidade;
            else
                produtoTamanho.Quantidade -= quantidade;

            db.ProdutoTamanho.Update(produtoTamanho);
        }

        db.MovimentacaoItem.Add(new MovimentacaoItem
        {
            Id = Guid.NewGuid().ToString(),
            IdMovimentacao = movimentacao.Id,
            IdProduto = produto.Id,
            Tamanho = tamanho,
            Quantidade = quantidade
        });
    }

    db.SaveChanges();
    if (erros.Any())
        TempData["Erro"] = string.Join(" | ", erros);
    else
        TempData["Sucesso"] = "Estoque atualizado com sucesso";

    return RedirectToAction("Movimentar");
    }

    // abre a tela de seleção de relatórios
    public ActionResult Relatorios()
    {
        return View();
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


    // relatório: histórico de todas as movimentações com seus itens
    public ActionResult RelatorioHistoricoMovimentacoes()
    {
        var movimentacoes = db.Movimentacao
            .OrderByDescending(m => m.Data)
            .ToList();

        ViewBag.Itens = db.MovimentacaoItem.ToList();
        ViewBag.Produtos = db.Produto.ToList();
        ViewBag.Usuarios = db.Usuario.ToList();

        ViewBag.Movimentacoes = db.Movimentacao.ToList();
        return View(movimentacoes);

    }

    public ActionResult RelatorioHistoricoPorProduto(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            return View("SelecionarProduto", db.Produto.ToList());
        }

        var produto = db.Produto.Single(p => p.Id == id);
        var movimentacoes = db.MovimentacaoItem
            .Where(mi => mi.IdProduto == id)
            .ToList();

        ViewBag.Produto = produto;
        ViewBag.Movimentacoes = db.Movimentacao.ToList();
        return View("RelatorioHistoricoPorProduto", movimentacoes);
    }

    public ActionResult DesfazerMovimentacao(string id)
    {
    if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");

    var movimentacao = db.Movimentacao.SingleOrDefault(m => m.Id == id);
    if (movimentacao == null)
    {
        TempData["Erro"] = "Movimentação não encontrada";
        return RedirectToAction("RelatorioHistoricoMovimentacoes");
    }

    var itens = db.MovimentacaoItem.Where(i => i.IdMovimentacao == id).ToList();

    if (!itens.Any())
    {
        TempData["Erro"] = "Esta movimentação não possui itens para desfazer";
        return RedirectToAction("RelatorioHistoricoMovimentacoes");
    }

    // verifica se dá pra desfazer sem deixar estoque negativo
    foreach (var item in itens)
    {
        var produtoTamanho = db.ProdutoTamanho
            .FirstOrDefault(pt => pt.IdProduto == item.IdProduto && pt.Tamanho == item.Tamanho);

        if (produtoTamanho == null)
        {
            var produto = db.Produto.FirstOrDefault(p => p.Id == item.IdProduto);

            if(produto == null)
                TempData["Erro"] = "Não é possível desfazer - produto deletado depois dessa movimentação";
            else
                TempData["Erro"] = $"Não é possível desfazer - tamanho '{item.Tamanho}' de '{produto.Nome}' foi removido depois desta movimentação.";
        
            return RedirectToAction("RelatorioHistoricoMovimentacoes");
        }
        
        
        // desfazer entrada = subtrair — pode deixar negativo?
        if (movimentacao.Tipo == "entrada" && produtoTamanho.Quantidade - item.Quantidade < 0)
        {
            var produto = db.Produto.FirstOrDefault(p => p.Id == item.IdProduto);
            TempData["Erro"] = $"Não é possível desfazer - estoque atual de '{produto?.Nome}' no tamanho {item.Tamanho} é {produtoTamanho.Quantidade}, menor que {item.Quantidade}.";
            return RedirectToAction("RelatorioHistoricoMovimentacoes");
        }
    }

    // tudo ok — desfaz
    foreach (var item in itens)
    {
        var produtoTamanho = db.ProdutoTamanho
            .FirstOrDefault(pt => pt.IdProduto == item.IdProduto && pt.Tamanho == item.Tamanho);

        if (movimentacao.Tipo == "entrada")
            produtoTamanho.Quantidade -= item.Quantidade; // desfaz entrada
        else
            produtoTamanho.Quantidade += item.Quantidade; // desfaz saída

        db.ProdutoTamanho.Update(produtoTamanho);
        db.MovimentacaoItem.Remove(item);
    }

    db.Movimentacao.Remove(movimentacao);
    db.SaveChanges();

    TempData["Sucesso"] = "Movimentação desfeita com sucesso";
    return RedirectToAction("RelatorioHistoricoMovimentacoes");
    }

    public ActionResult DeletarTamanho(string id)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        var pt = db.ProdutoTamanho.Single(x => x.Id == id);
        var idProduto = pt.IdProduto;
        db.ProdutoTamanho.Remove(pt);
        db.SaveChanges();
        return RedirectToAction("Update", new { id = idProduto });
    }

}