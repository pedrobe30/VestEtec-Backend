// DTOs/EncomendaDto.cs
using System;
using System.Collections.Generic;

namespace Backend_Vestetec_App.DTOs
{
    public class EncomendaDto
    {
        public int IdEncomenda { get; set; }
        public int IdAluno { get; set; }
        public string NomeAluno { get; set; }
        public string EmailAluno { get; set; }
        public int IdEscola { get; set; }
        public string NomeEscola { get; set; }
        public DateTime DataEncomenda { get; set; }
        public decimal PrecoEncomenda { get; set; }
        public string Situacao { get; set; }
        // CORREÇÃO: Mudança para DateTime para consistência
        public DateTime? DataEntrega { get; set; }
        public List<ItemEncomendaDto> Itens { get; set; } = new List<ItemEncomendaDto>();
    }

    public class ItemEncomendaDto
    {
        public int IdItem { get; set; }
        public int IdProduto { get; set; }
        public string NomeProduto { get; set; }
        public string CategoriaNome { get; set; }
        public string ModeloNome { get; set; }
        public string TecidoNome { get; set; }
        public string ImagemUrl { get; set; }
        public decimal PrecoUnitario { get; set; }
        public int Quantidade { get; set; }
        public decimal PrecoTotal { get; set; }
        public string Tamanho { get; set; }
    }

    public class CriarEncomendaDto
    {
        public int IdAluno { get; set; }
        public int IdEscola { get; set; }
        public List<ItemCarrinhoDto> Itens { get; set; } = new List<ItemCarrinhoDto>();
    }

    public class ItemCarrinhoDto
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
        public string Tamanho { get; set; }
    }

    public class AtualizarStatusEncomendaDto
    {
        public int IdEncomenda { get; set; }
        public string NovoStatus { get; set; }
        // CORREÇÃO: Mudança para DateTime para consistência
        public DateTime? DataEntrega { get; set; }
    }

    public class EncomendaResumoDto
    {
        public int IdEncomenda { get; set; }
        public string NomeAluno { get; set; }
        public string NomeEscola { get; set; }
        public DateTime DataEncomenda { get; set; }
        public decimal PrecoEncomenda { get; set; }
        public string Situacao { get; set; }
        // CORREÇÃO: Mudança para DateTime para consistência
        public DateTime? DataEntrega { get; set; }
        public int TotalItens { get; set; }
    }

    public class FiltroEncomendaDto
    {
        public string Status { get; set; }
        public int? IdEscola { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int Pagina { get; set; } = 1;
        public int ItensPorPagina { get; set; } = 10;
    }

    public class EncomendaAlunoDto
    {
        public int IdEncomenda { get; set; }
        public DateTime DataEncomenda { get; set; }
        public decimal PrecoEncomenda { get; set; }
        public string Situacao { get; set; }
        // CORREÇÃO: Mudança para DateTime para consistência
        public DateTime? DataEntrega { get; set; }
        public int TotalItens { get; set; }
        public List<ItemEncomendaDto> Itens { get; set; } = new List<ItemEncomendaDto>();
    }
}