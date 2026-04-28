namespace EcommerceMVC.Models
{
    public class Produto
    {
        public int Id { get; set; }
<<<<<<< HEAD
        public required string Nome { get; set; }
=======
        public required string Nome { get; set; } = String.Empty;
>>>>>>> 602669761de2ece11fb9f909b852eab354278a16
        public string? Descricao { get; set; }
        public int Estoque { get; set; }
        public decimal Preco { get; set; }
        public string? Imagem { get; set; }
    }
}
