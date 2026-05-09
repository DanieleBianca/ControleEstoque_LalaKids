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
        return View(db.Produto.ToList());  //view.index.cshtml
    }

    [HttpGet] //atributo do método, indica que ele responde a requisições get (abre a view)
    public ActionResult Create() //método do crud, CREATE - mostra a view pra criar um produto
    {
        return View();
    }

    [HttpPost] //atributo do método, indica que ele responde a requisições post (submete o formulário)
    //data binding: os dados do formulário são convertidos em um objeto do tipo Produto e passados como parâmetro pro método
    public ActionResult Create(Produto p) // método do crud, CREATE - recebe os dados do produto e salva no banco de dados
    {
        p.Id = Guid.NewGuid().ToString(); //gera id único pra cada produto
        db.Produto.Add(p); //INSERT INTO Produto VALUES (p...)
        db.SaveChanges(); //commit
        return RedirectToAction("Index");
    }

    public ActionResult Delete(string id) //método do crud, DELETE - recebe o id do produto a ser deletado, busca no bd e deleta
    {
        var produto = db.Produto.Single(p => p.Id == id); //LinQ (busca produto com id igual ao recebido)
        db.Produto.Remove(produto); //DELETE FROM Produto WHERE Id = id
        db.SaveChanges();
        return RedirectToAction("Index"); //após a ação, redireciona o usuário de volta pra lista
    }

    [HttpGet]
    public ActionResult Update(string id) //método do crud, UPDATE - recebe o id do produto, busca no bd e mostra a view pra atualizar
    {
        var produto = db.Produto.Single(p => p.Id == id); //LinQ; SELECT * FROM Produto WHERE Id = id
        return View(produto);
    }

    [HttpPost]
    public ActionResult Update(Produto p) //método do crud, UPDATE - recebe os dados do produto atualizado e salva no bd
    {
        db.Produto.Update(p); //UPDATE SET em todos os campos
        db.SaveChanges(); //commit
        return RedirectToAction("Index");
    }
}