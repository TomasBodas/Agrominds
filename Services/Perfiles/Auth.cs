using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Perfiles
{
    public abstract class Auth
    {
        public string Name { get; set; }
        public int Id;
        public abstract IList<Auth> Children { get; }
        public PermissionType Permission { get; set; }
        public abstract void AddChild(Auth c);
        public abstract void EmptyChildren();
    }
}
