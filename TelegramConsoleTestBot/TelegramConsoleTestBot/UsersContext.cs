using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;

namespace TelegramConsoleTestBot
{
    class UsersContext : DbContext
    {
        public UsersContext() : base("DbConnection")
        {

        }

        public DbSet<User> Users { get; set; }
    }
}
