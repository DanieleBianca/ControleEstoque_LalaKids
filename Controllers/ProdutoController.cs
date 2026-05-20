using Microsoft.AspNetCore.Mvc;

public class ProdutoController : Controller
{
    private DatabaseContext db;

    public ProdutoController(DatabaseContext db)
    {
        this.db = db;
    }

    private bool UsuarioLogado()
    {
        return HttpContext.Session.GetString("UsuarioNome") != null;
    }

    public ActionResult Index()
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        return View(db.Produto.ToList());
    }

    [HttpGet]
    public ActionResult Create()
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        return View();
    }

    [HttpPost]
    public ActionResult Create(Produto p)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        p.Id = Guid.NewGuid().ToString();
        db.Produto.Add(p);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    public ActionResult Delete(string id)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        var produto = db.Produto.Single(p => p.Id == id);
        db.Produto.Remove(produto);
        db.SaveChanges();
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Update(string id)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        var produto = db.Produto.Single(p => p.Id == id);
        return View(produto);
    }

    [HttpPost]
    public ActionResult Update(Produto p)
    {
        if (!UsuarioLogado()) return RedirectToAction("Login", "Usuario");
        db.Produto.Update(p);
        db.SaveChanges();
        return RedirectToAction("Index");
    }
<<<<<<< HEAD

    [HttpGet]
    public ActionResult Movimentar()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Movimentar(string codigoBarras, string tipo, int quantidade)
    {
        var produto = db.Produto.SingleOrDefault(p => p.CodigoBarras == codigoBarras);

        if (produto == null)
        {
            TempData["Erro"] = "Produto não encontrado. Verifique o código de barras.";
            return RedirectToAction("Movimentar");
        }

        if (tipo == "saida" && quantidade > produto.Quantidade)
        {
            TempData["Erro"] = $"Quantidade insuficiente. Estoque atual: {produto.Quantidade} unidades.";
            return RedirectToAction("Movimentar");
        }

        if (tipo == "entrada")
            produto.Quantidade += quantidade;
        else if (tipo == "saida")
            produto.Quantidade -= quantidade;

        db.Produto.Update(produto);
        db.SaveChanges();

        TempData["Sucesso"] = $"Estoque atualizado! {produto.Nome} agora tem {produto.Quantidade} unidades.";
        return RedirectToAction("Movimentar");
    }
}
=======
}
>>>>>>> parent of d041192 (adiciona movimentar estoque)
