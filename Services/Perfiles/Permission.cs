using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Perfiles
{
    public class Permission : Auth
    {
        public Permission(object[] pObject)
        {
            Name = pObject[0].ToString(); Id = int.Parse(pObject[1].ToString());
            Permission = (PermissionType)Enum.Parse(typeof(PermissionType), pObject[2].ToString());
        }

        public override IList<Auth> Children
        {
            get
            {
                return new List<Auth>();
            }

        }

        public override void AddChild(Auth c)
        {

        }

        public override void EmptyChildren()
        {

        }
    }
}
