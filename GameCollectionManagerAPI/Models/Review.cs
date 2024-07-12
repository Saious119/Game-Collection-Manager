using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameCollectionManagerAPI.Models
{
    public class Review
    {
        public string? name { get; set; }
        public string? date { get; set; }
        public int? grade { get; set; }
        public string? body { get; set; }

        public Review(string? name, string? date, int? grade, string? body)
        {
            this.name = name;
            this.date = date;
            this.grade = grade;
            this.body = body;
        }
    }
}
