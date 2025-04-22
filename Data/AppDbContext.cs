using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;


namespace Backend_Vestetec_App.Models;

public partial class AppDbContext : DbContext
{
    public AppDbContext()
    {
    }

    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Admin> Admins { get; set; }

    public virtual DbSet<Aluno> Alunos { get; set; }

    public virtual DbSet<Categoria> Categorias { get; set; }

    public virtual DbSet<Empresa> Empresas { get; set; }

    public virtual DbSet<Encomenda> Encomendas { get; set; }

    public virtual DbSet<Escola> Escolas { get; set; }

    public virtual DbSet<HistoricoEstoque> HistoricoEstoques { get; set; }

    public virtual DbSet<Itensencomendado> Itensencomendados { get; set; }

    public virtual DbSet<Modelo> Modelos { get; set; }

    public virtual DbSet<Produto> Produtos { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Tecido> Tecidos { get; set; }

    public virtual DbSet<Teste> Testes { get; set; }

    public virtual DbSet<CodigoVerificacao> CodigosVerificacao {get; set;}
   

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseMySql("server=localhost;database=controle_de_estoque_bd;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_general_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Admin>(entity =>
        {
            entity.HasKey(e => e.IdAdm).HasName("PRIMARY");

            entity.HasOne(d => d.IdEmpresaNavigation).WithMany(p => p.Admins)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("admins_ibfk_1");
        });

        modelBuilder.Entity<Aluno>(entity =>
        {
            entity.HasKey(e => e.IdAluno).HasName("PRIMARY");
        });

        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.HasKey(e => e.IdCategoria).HasName("PRIMARY");

            entity.Property(e => e.IdCategoria).ValueGeneratedNever();
        });

        modelBuilder.Entity<Empresa>(entity =>
        {
            entity.HasKey(e => e.IdEmpresa).HasName("PRIMARY");

            entity.Property(e => e.IdEmpresa).ValueGeneratedNever();
        });

        modelBuilder.Entity<Encomenda>(entity =>
        {
            entity.HasKey(e => e.IdEncomenda).HasName("PRIMARY");

            entity.HasOne(d => d.IdAlunoNavigation).WithMany(p => p.Encomenda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_alunos_enco");

            entity.HasOne(d => d.IdEscolaNavigation).WithMany(p => p.Encomenda)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_escola");
        });

        modelBuilder.Entity<Escola>(entity =>
        {
            entity.HasKey(e => e.IdEscola).HasName("PRIMARY");

            entity.Property(e => e.IdEscola).ValueGeneratedNever();
        });

        modelBuilder.Entity<HistoricoEstoque>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Id).ValueGeneratedNever();

            entity.HasOne(d => d.IdAdmNavigation).WithMany(p => p.HistoricoEstoques)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_adm");
        });

        modelBuilder.Entity<Itensencomendado>(entity =>
        {
            entity.HasKey(e => e.IdItem).HasName("PRIMARY");

            entity.Property(e => e.IdItem).ValueGeneratedNever();
        });

        modelBuilder.Entity<Modelo>(entity =>
        {
            entity.HasKey(e => e.IdModelo).HasName("PRIMARY");

            entity.Property(e => e.IdModelo).ValueGeneratedNever();
        });

        modelBuilder.Entity<Produto>(entity =>
        {
            entity.HasKey(e => e.IdProd).HasName("PRIMARY");

            entity.Property(e => e.IdProd).ValueGeneratedNever();

            entity.HasOne(d => d.IdCategoriaNavigation).WithMany(p => p.Produtos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_categoria");

            entity.HasOne(d => d.IdModeloNavigation).WithMany(p => p.Produtos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_modelo");

            entity.HasOne(d => d.IdStatusNavigation).WithMany(p => p.Produtos)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_status");

            entity.HasOne(d => d.IdTecidoNavigation).WithMany(p => p.Produtos).HasConstraintName("fk_tecido");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("PRIMARY");

            entity.Property(e => e.IdStatus).ValueGeneratedNever();
        });

        modelBuilder.Entity<Tecido>(entity =>
        {
            entity.HasKey(e => e.IdTecido).HasName("PRIMARY");

            entity.Property(e => e.IdTecido).ValueGeneratedNever();
        });

            modelBuilder.Entity<CodigoVerificacao>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
        });

        



        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
