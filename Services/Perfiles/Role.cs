using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Perfiles
{
    public class Role : Auth
    {
        public IList<Auth> children;
        public Role(object[] pObject)
        {
            Name = pObject[0].ToString(); Id = int.Parse(pObject[1].ToString());
            children = new List<Auth>();
        }
        public override IList<Auth> Children
        {
            get
            {
                return children.ToArray();
            }
        }
        public override void EmptyChildren()
        {
            children = new List<Auth>();
        }
        public override void AddChild(Auth pAuth)
        {
            children.Add(pAuth);
        }
    }
}
