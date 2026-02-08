using System;
using System.Collections.Generic;
using System.Text;

namespace ATMS.Admin.Contracts.Models;

public class RoleModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}
