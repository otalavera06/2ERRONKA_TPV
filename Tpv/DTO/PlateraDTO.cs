using System.Collections.Generic;

namespace Tpv.DTO
{
    public class PlateraDTO
    {
        public int Id { get; set; }
        public string Izena { get; set; }
        public string Mota { get; set; }
        public decimal Prezioa { get; set; }
        public string ArgazkiaUrl { get; set; }
    }

    public class ErantzunaDTO<T>
    {
        public int Code { get; set; }
        public string Message { get; set; }
        public T Datuak { get; set; }
    }
}
