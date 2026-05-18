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
    return View(db.Usuario.ToList());
    }

    // GET - abre formulário de cadastro
    [HttpGet]
    public ActionResult Cadastro()
    {
        return View();
    }

    // POST - salva o usuário
    [HttpPost]
    public ActionResult Cadastro(Usuario u)
    {
        u.Id = Guid.NewGuid().ToString();
        db.Usuario.Add(u);
        db.SaveChanges();
        return RedirectToAction("Login");
    }

    // GET - abre tela de login
    [HttpGet]
    public ActionResult Login()
    {
        return View();
    }

    // POST - verifica login e senha
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

        HttpContext.Session.SetString("UsuarioNome", usuario.Nome); //salva o nome do usuário na sessão para usar nas telas
        return RedirectToAction("Index", "Produto");
    }

    // Logout - limpa a sessão
    public ActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    //Deleta usuario cadastrado
    public ActionResult Delete(string id)
    {
    var usuario = db.Usuario.Single(u => u.Id == id);
    db.Usuario.Remove(usuario);
    db.SaveChanges();
    return RedirectToAction("Login");
    }
}
