using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Entities
{
    public class UserToken : BaseEntity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public string FcmToken { get; set; }
    }
}
