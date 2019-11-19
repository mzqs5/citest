using Abp.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace IF.Authorization.Users
{
    [Table("porsche_contacts")]
    public class Contacts : Entity<int>
    {
        public long UserId { get; set; }

        public string Name { get; set; } = "";

        public string Mobile { get; set; }
    }
}
