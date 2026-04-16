using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Tpv.DTO;

namespace Tpv.Services
{
    public class ErreserbaService
    {
        private static string BaseUrl => ApiConfig.ApiBaseUrl + "/erreserbak";

        public async Task<bool> EguneratuErreserba(int mahaiaId, DateTime data, bool mota, ErreserbakSortuDto dto)
        {
            using (var client = new HttpClient())
            {
                var url = $"{BaseUrl}/mahaia/{mahaiaId}?data={data:yyyy-MM-dd}&mota={mota.ToString().ToLower()}";
                var json = JsonConvert.SerializeObject(dto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                try
                {
                    var response = await client.PutAsync(url, content);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }

        public async Task<bool> EzabatuErreserba(int mahaiaId, DateTime data, bool mota)
        {
            using (var client = new HttpClient())
            {
                var url = $"{BaseUrl}/mahaia/{mahaiaId}?data={data:yyyy-MM-dd}&mota={mota.ToString().ToLower()}";

                try
                {
                    var response = await client.DeleteAsync(url);
                    return response.IsSuccessStatusCode;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
