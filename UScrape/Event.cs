using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UScrape
{
    [Serializable]
    public class Event : ISavable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string City { get; set; }
        public string Adress { get; set; }
        public DateTime Date { get; set; }
        public string Price { get; set; }
        public string Image { get; set; }

        public Event(string name, string description, string category, string city, string adress, DateTime date, string price, string image)
        {
            Name = name;
            Description = description;
            Category = category;
            City = city;
            Adress = adress;
            Date = date;
            Price = price;
            Image = image;
        }

        public string ToJSON()
        {
            string jsonString = JsonSerializer.Serialize(this);
            return jsonString;
        }

        public string ToSQL(string table)
        {
            string sqlString = $"INSERT INTO {table} (nom, description, category, ville, adresse, date, prix, image) VALUES ('{Name.Replace("'", "''")}', '{Description.Replace("'", "''")}', '{Category.Replace("'", "''")}', '{City.Replace("'", "''")}', '{Adress.Replace("'", "''")}', '{Date}', '{Price.Replace("'", "''")}', '{Image.Replace("'", "''")}');";
            return sqlString;
        }
    }
}
