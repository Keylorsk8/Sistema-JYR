using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Web;

namespace Sistema_JYR.Models
{
    public class RoleViewModels
    {
        public RoleViewModels() { }

        public RoleViewModels(ApplicationRole Role)
        {
            Id = Role.Id;
            name = Role.Name;
        }

        public string Id { get; set; }
        public string name { get; set; }
    }
}