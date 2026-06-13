namespace LojaApi.Models
{
    public class SalaReuniao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int Capacidade { get; set; }
        public bool PossuiProjetor { get; set; }
    }
}