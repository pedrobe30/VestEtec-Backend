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

    public virtual DbSet<ProdutoImagem> ProdutoImagens { get; set; }

    public virtual DbSet<Status> Statuses { get; set; }

    public virtual DbSet<Tecido> Tecidos { get; set; }

    public virtual DbSet<Teste> Testes { get; set; }
    public virtual DbSet<Estoque> Estoques { get; set; }

    public virtual DbSet<CodigoVerificacao> CodigosVerificacao { get; set; }


    // protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    //    => optionsBuilder.UseMySql("server=localhost;database=controle_de_estoque_bd;user=root", Microsoft.EntityFrameworkCore.ServerVersion.Parse("10.4.32-mariadb"));

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

            entity.Property(e => e.IdEncomenda).ValueGeneratedOnAdd();

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

            entity.Property(e => e.IdItem).ValueGeneratedOnAdd();
        });

        modelBuilder.Entity<Modelo>(entity =>
        {
            entity.HasKey(e => e.IdModelo).HasName("PRIMARY");

            entity.Property(e => e.IdModelo).ValueGeneratedNever();
        });

        modelBuilder.Entity<Produto>(entity =>
            {
                entity.ToTable("produto");
                entity.HasKey(e => e.IdProd).HasName("PRIMARY");

                entity.Property(e => e.IdProd)
                      .HasColumnName("ID_prod")
                      .ValueGeneratedOnAdd(); // mudar de ValueGeneratedNever para ValueGeneratedOnAdd

                entity.Property(e => e.Preco)
                      .HasColumnName("preco")
                      .HasPrecision(10, 2);

                entity.Property(e => e.IdCategoria)
                      .HasColumnName("ID_categoria");

                entity.Property(e => e.IdModelo)
                      .HasColumnName("ID_modelo");

                entity.Property(e => e.IdTecido)
                      .HasColumnName("ID_tecido");

                entity.Property(e => e.IdStatus)
                      .HasColumnName("ID_status");

                entity.Property(e => e.ImgUrl)
                      .HasColumnName("img_url")
                      .HasMaxLength(255);

                entity.Property(e => e.descricao)
                      .HasColumnName("descricao")
                      .HasMaxLength(255);

                entity.HasOne(d => d.IdCategoriaNavigation)
                      .WithMany(p => p.Produtos)
                      .HasForeignKey(d => d.IdCategoria)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_categoria");

                entity.HasOne(d => d.IdModeloNavigation)
                      .WithMany(p => p.Produtos)
                      .HasForeignKey(d => d.IdModelo)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_modelo");

                entity.HasOne(d => d.IdStatusNavigation)
                      .WithMany(p => p.Produtos)
                      .HasForeignKey(d => d.IdStatus)
                      .OnDelete(DeleteBehavior.ClientSetNull)
                      .HasConstraintName("fk_status");

                entity.HasOne(d => d.IdTecidoNavigation)
                      .WithMany(p => p.Produtos)
                      .HasForeignKey(d => d.IdTecido)
                      .HasConstraintName("fk_tecido");

                entity.HasMany(e => e.Estoque)
                      .WithOne(e => e.IdProdutoNavigation)
                      .HasForeignKey(e => e.IdProduto)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("fk_estoque_produto");

                // NOVA CONFIGURAÇÃO PARA PRODUTO IMAGENS
                entity.HasMany(e => e.ProdutoImagens)
                      .WithOne(e => e.IdProdutoNavigation)
                      .HasForeignKey(e => e.IdProduto)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("fk_produto_imagem");
            });

        // NOVA CONFIGURAÇÃO DA TABELA PRODUTO_IMAGEM
        modelBuilder.Entity<ProdutoImagem>(entity =>
        {
            entity.ToTable("produto_imagem");
            entity.HasKey(e => e.IdProdutoImagem).HasName("PRIMARY");

            entity.Property(e => e.IdProdutoImagem)
                  .HasColumnName("ID_produto_imagem")
                  .ValueGeneratedOnAdd();

            entity.Property(e => e.IdProduto)
                  .HasColumnName("ID_produto");

            entity.Property(e => e.ImgUrl)
                  .HasColumnName("img_url")
                  .HasMaxLength(255)
                  .IsRequired();

            entity.Property(e => e.OrdemExibicao)
                  .HasColumnName("ordem_exibicao")
                  .HasDefaultValue((byte)1);

            entity.Property(e => e.IsPrincipal)
                  .HasColumnName("is_principal")
                  .HasDefaultValue(false);

            entity.Property(e => e.DataCriacao)
                  .HasColumnName("data_criacao")
                  .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.IdProdutoNavigation)
                  .WithMany(p => p.ProdutoImagens)
                  .HasForeignKey(d => d.IdProduto)
                  .OnDelete(DeleteBehavior.Cascade)
                  .HasConstraintName("fk_produto_imagem");

            // Índice para otimizar consultas por produto
            entity.HasIndex(e => e.IdProduto, "idx_produto_imagem_produto");

            // Índice para otimizar consultas por imagem principal
            entity.HasIndex(e => new { e.IdProduto, e.IsPrincipal }, "idx_produto_imagem_principal");
        });

        modelBuilder.Entity<Status>(entity =>
        {
            entity.HasKey(e => e.IdStatus).HasName("PRIMARY");

            entity.Property(e => e.IdStatus).ValueGeneratedNever();
        });

        modelBuilder.Entity<Estoque>(entity =>
            {
                entity.ToTable("estoque");
                entity.HasKey(e => e.IdEstoque).HasName("PRIMARY");

                entity.Property(e => e.IdEstoque)
                      .HasColumnName("id_estoque")
                      .ValueGeneratedOnAdd(); // auto-increment

                entity.Property(e => e.IdProduto)
                      .HasColumnName("id_produto");

                entity.Property(e => e.Tamanho)
                      .HasColumnName("tamanho")
                      .HasMaxLength(10)
                      .IsRequired();

                entity.Property(e => e.Quantidade)
                      .HasColumnName("quantidade");

                entity.HasOne(e => e.IdProdutoNavigation)
                      .WithMany(p => p.Estoque)
                      .HasForeignKey(e => e.IdProduto)
                      .OnDelete(DeleteBehavior.Cascade)
                      .HasConstraintName("fk_estoque_produto");
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