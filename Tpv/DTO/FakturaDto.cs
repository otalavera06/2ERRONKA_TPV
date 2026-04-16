namespace Tpv.DTO
{
    public class FakturaDto
    {
        public int Id { get; set; }
        public int ZerbitzuaId { get; set; }
        public decimal PrezioTotala { get; set; }
        public bool Sortuta { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
