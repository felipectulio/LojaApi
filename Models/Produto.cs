using System.ComponentModel.DataAnnotations;

namespace LojaApi.Models
{
    public class Produto
    {
        [Key]
        public int Codigo { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public decimal Preco { get; set; }
    }
}