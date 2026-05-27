using Microsoft.AspNetCore.Mvc;

public class UsuarioController : Controller
{
    private DatabaseContext db;

    public UsuarioController(DatabaseContext db)
    {
        this.db = db;
    }

    public ActionResult Index()
    {
        return View("Gerenciar", db.Usuario.ToList());
    }

    [HttpGet]
    public ActionResult Cadastro()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Cadastro(Usuario u)
    {
        u.Id = Guid.NewGuid().ToString();
        db.Usuario.Add(u);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário {u.Nome} cadastrado com sucesso!";
        return RedirectToAction("Login");
    }

    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public ActionResult Login(string login, string senha)
    {
        var usuario = db.Usuario
            .SingleOrDefault(u => u.Login == login && u.Senha == senha);

        if (usuario == null)
        {
            ViewBag.Erro = "Login ou senha inválidos.";
            return View();
        }

        HttpContext.Session.SetString("UsuarioNome", usuario.Nome);
        HttpContext.Session.SetString("UsuarioId", usuario.Id);
        return RedirectToAction("Index", "Produto");
    }

    public ActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    public ActionResult Delete(string id)
    {
        var idLogado = HttpContext.Session.GetString("UsuarioId");
        if (id == idLogado)
        {
            TempData["Erro"] = "Você não pode excluir o usuário logado!";
            return RedirectToAction("Index");
        }

        var usuario = db.Usuario.Single(u => u.Id == id);
        db.Usuario.Remove(usuario);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário {usuario.Nome} excluído com sucesso!";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Editar(string id)
    {
        var usuario = db.Usuario.Single(u => u.Id == id);
        return View(usuario);
    }

    [HttpPost]
    public ActionResult Editar(Usuario u)
    {
        db.Usuario.Update(u);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário {u.Nome} atualizado com sucesso!";
        return RedirectToAction("Index");
    }
}