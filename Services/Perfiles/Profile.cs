using Services.Models;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Perfiles
{
    public class Profile : Auth
    {
        public IList<Auth> children;
        public Profile()
        {
            children = new List<Auth>();
        }
        public Profile(object[] pObject)
        {
            id = int.Parse(pObject[0].ToString()); Name = pObject[1].ToString();
            children = new List<Auth>();
        }
        public override IList<Auth> Children
        {
            get
            {
                return children.ToArray();
            }
        }

        public override void AddChild(Auth pAuth)
        {
            children.Add(pAuth);
        }

        public override void EmptyChildren()
        {
            throw new NotImplementedException();
        }
        public int id;
    }
}
