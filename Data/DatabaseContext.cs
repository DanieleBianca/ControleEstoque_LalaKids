using Microsoft.EntityFrameworkCore;

    public class DatabaseContext : DbContext //herança: classe herda tudo de dbcontext
    {
    public DatabaseContext(DbContextOptions<DatabaseContext> options)
        : base(options) {}

    public DbSet<Produto> Produto { get; set; } //tabela produto no banco de dados
    }
