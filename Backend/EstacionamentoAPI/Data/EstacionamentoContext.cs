using Microsoft.EntityFrameworkCore;
using EstacionamentoAPI.Models;

namespace EstacionamentoAPI.Data
{
    public class EstacionamentoContext : DbContext
    {
        public EstacionamentoContext(DbContextOptions<EstacionamentoContext> options)
            : base(options)
        {
        }

        public DbSet<Veiculo> Veiculos { get; set; }
        public DbSet<Sessao> Sessoes { get; set; }
        public DbSet<Configuracao> Configuracoes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuração de Veiculo
            modelBuilder.Entity<Veiculo>(entity =>
            {
                entity.HasKey(v => v.Id);
                entity.HasIndex(v => v.Placa).IsUnique();
                entity.Property(v => v.Placa).IsRequired().HasMaxLength(10);
                entity.Property(v => v.Modelo).HasMaxLength(100);
                entity.Property(v => v.Cor).HasMaxLength(50);
                entity.Property(v => v.Tipo).IsRequired();
            });

            // Configuração de Sessao
            modelBuilder.Entity<Sessao>(entity =>
            {
                entity.HasKey(s => s.Id);
                entity.HasOne(s => s.Veiculo)
                      .WithMany(v => v.Sessoes)
                      .HasForeignKey(s => s.VeiculoId)
                      .OnDelete(DeleteBehavior.Cascade);
                
                entity.Property(s => s.ValorCobrado).HasColumnType("decimal(10,2)");
            });

            // Configuração de Configuracao
            modelBuilder.Entity<Configuracao>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.HasIndex(c => c.Chave).IsUnique();
            });

            // Configuração de Usuario
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.Nome).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(100);
                entity.Property(u => u.SenhaHash).IsRequired();
            });

            // Seed de configurações padrão
            modelBuilder.Entity<Configuracao>().HasData(
                new Configuracao { Id = 1, Chave = "PrecoPrimeiraHora", Valor = "5.00", Descricao = "Valor cobrado pela primeira hora (R$)" },
                new Configuracao { Id = 2, Chave = "PrecoDemaisHoras", Valor = "3.00", Descricao = "Valor cobrado pelas demais horas (R$)" }
            );

            // Usuário admin será criado no Program.cs
        }
    }
}
