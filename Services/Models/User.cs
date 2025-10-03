using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Web;

namespace Services.Models
{
    public class User
    {
        public int id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public int DNI { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsBlocked { get; set; }
        public int Attempts { get; set; }
        public int LanguageId { get; set; }
        public Perfiles.Profile profile;
        public User() { }
        public User(object[] itemArray) : base()
        {
            this.id = int.Parse(itemArray[0].ToString());
            this.Name = itemArray[1].ToString();
            this.Surname = itemArray[2].ToString();
            this.DNI = int.Parse(itemArray[3].ToString());
            this.Email = itemArray[4].ToString();
            this.Password = itemArray[5].ToString();
            this.LanguageId = int.Parse(itemArray[6].ToString());
        }
        public List<string> permissionList = new List<string>();

    }
}