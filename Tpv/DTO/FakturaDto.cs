namespace Tpv.DTO
{
    public class FakturaDto
    {
        public int Id { get; set; }
        public int ZerbitzuaId { get; set; }
        public decimal PrezioTotala { get; set; }
        public string Data { get; set; } = string.Empty;
        public string MahaiaIzena { get; set; } = string.Empty;
        public string EskaeraXehetasunak { get; set; } = string.Empty;
        public bool Sortuta { get; set; }
        public string Path { get; set; } = string.Empty;
    }
}
