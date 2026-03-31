namespace EcommerceMVC.Models
{
    public class Produto
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public string Descricao { get; set; }
        public int Estoque { get; set; }
        public decimal Preco { get; set; }
        public string Imagem { get; set; }
    }
}
