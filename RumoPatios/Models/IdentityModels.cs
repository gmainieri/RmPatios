using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace RumoPatios.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            // Add custom user claims here
            return userIdentity;
        }
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public DbSet<Carregamento> Carregamentos { get; set; }
        public DbSet<Partida> Partidas { get; set; }
        public DbSet<Chegada> Chegadas { get; set; }
        public DbSet<Linha> Linhas { get; set; }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            //é melhor deixar esta convencao ativada, pois quando da erro, ele avisa logo no update database que já existe um cascade delete (e na mensagem de erro ele fala exatamente qual o FK que da problema), então é só mudar para cascadeDelete para 'false' no migration que acabou de ser criado
            //modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>(); 
            //ao remover esta condição, o erro só aparece no delete quando algo fica orfão
        }
    }

}