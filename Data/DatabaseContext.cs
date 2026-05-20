using Microsoft.EntityFrameworkCore;

    public class DatabaseContext : DbContext //herança: classe herda tudo de dbcontext
    {
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) {}

    public DbSet<Produto> Produto { get; set; } //tabela produto no banco de dados
    
    public DbSet<Usuario> Usuario { get; set; } //tabela de usuario no banco de dados
    }
