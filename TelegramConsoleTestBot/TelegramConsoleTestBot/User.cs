using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelegramConsoleTestBot
{
    class User
    {
        public String Name { get; set; }
        public bool AppealFlag { get; set; }
        public int ID { get; set; }
        public String UserName { get; set; }
        public DateTime AppealDate { get; set; }
        public String Adress { get; set; }
        public int UserID { get; set; }
    }
}
