using Microsoft.AspNetCore.Mvc;

public class UsuarioController : Controller
{
    private DatabaseContext db;

    public UsuarioController(DatabaseContext db)
    {
        this.db = db;
    }

    private bool IsAdmin()
    {
        return HttpContext.Session.GetString("UsuarioTipo") == "admin";
    }

    private bool UsuarioLogado()
    {
        return HttpContext.Session.GetString("UsuarioId") != null;
    }

    public ActionResult Index()
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Produto");
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
        var existe = db.Usuario.Any(x => x.Login == u.Login);
        if (existe)
        {
            TempData["Erro"] = "Este login já está em uso. Escolha outro";
            return RedirectToAction("Cadastro");
        }
        u.Id = Guid.NewGuid().ToString();
        u.Tipo = "funcionario"; // novo usuário sempre começa como funcionário
        db.Usuario.Add(u);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário \"{u.Nome}\" cadastrado com sucesso";
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
            ViewBag.Erro = "Login ou senha inválidos";
            return View();
        }

        HttpContext.Session.SetString("UsuarioTipo", usuario.Tipo);
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
        if (!IsAdmin()) return RedirectToAction("Index", "Produto");
        var usuario = db.Usuario.Single(u => u.Id == id);
        db.Usuario.Remove(usuario);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário \"{usuario.Nome}\" excluído com sucesso";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public ActionResult Editar(string id)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Produto");
        var usuario = db.Usuario.Single(u => u.Id == id);
        return View(usuario);
    }

    [HttpPost]
    public ActionResult Editar(Usuario u)
    {
        if (!IsAdmin()) return RedirectToAction("Index", "Produto");
        db.Usuario.Update(u);
        db.SaveChanges();
        TempData["Sucesso"] = $"Usuário \"{u.Nome}\" atualizado com sucesso";
        return RedirectToAction("Index");
    }

    [HttpGet]
public ActionResult TrocarSenha()
{
    if (!UsuarioLogado()) return RedirectToAction("Login");
    return View();
}

[HttpPost]
public ActionResult TrocarSenha(string senhaAtual, string novaSenha)
{
    if (!UsuarioLogado()) return RedirectToAction("Login");

    var id = HttpContext.Session.GetString("UsuarioId");
    var usuario = db.Usuario.Single(u => u.Id == id);

    if (usuario.Senha != senhaAtual)
    {
        TempData["Erro"] = "Senha atual incorreta";
        return RedirectToAction("TrocarSenha");
    }

    if (string.IsNullOrEmpty(novaSenha))
    {
        TempData["Erro"] = "A nova senha não pode ser vazia";
        return RedirectToAction("TrocarSenha");
    }

    usuario.Senha = novaSenha;
    db.Usuario.Update(usuario);
    db.SaveChanges();
    TempData["Sucesso"] = "Senha alterada com sucesso";
    return RedirectToAction("TrocarSenha");
}
}