using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models
{
    public static class Sabitler
    {
        
    }
    public static class Roller
    {
        public const string Root = "Root";
    }
    public class Response<T> where T : class
    {
        public Response()
        {

        }
        public Response(string Mesaj)
        {
            Hata = true;
            this.Mesaj = Mesaj;
        }
        public bool Hata { get; set; } = false;
        public string Mesaj { get; set; }
        public List<T> Sonuclar { get; set; } = new();
        public T Sonuc { get; set; }
    }
    public class TokenView
    {
        public TokenView()
        {
        }

        public DateTime Expires { get; set; }
        public string Token { get; set; }
        public IList<string> Roles { get; set; }
    }
}
