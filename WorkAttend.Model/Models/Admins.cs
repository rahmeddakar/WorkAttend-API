using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkAttend.Model.Models
{
    public class workattendadmin
    {
        public int adminID { get; set; }
        public string name { get; set; }
        public string createdBy { get; set; }
        public DateTime? createdOn { get; set; }
        public string email { get; set; }
        public bool isDeleted { get; set; }
        public string password { get; set; }
        public string updatedBy { get; set; }
        public DateTime? updatedOn { get; set; }
    }

    public class Admins
    {
        public string adminEmail { get; set; }
        public string adminPassword { get; set; }
        public int roleID { get; set; }
        public string adminName { get; set; }
    }
    public class adminMod
    {
        public int adminID { get; set; }
        public string adminEmail { get; set; }
        public string roleName { get; set; }
        public DateTime createdOn { get; set; }
        public string createdBy { get; set; }
    }
    public class AdminModel
    {
        public List<adminMod> adminList { get; set; }
        public int roleID { get; set; }
        public SelectList roles { get; set; }
    }
    public class AdminPermissions
    {
        public int adminID { get; set; }
       public SelectList admins { get; set; }
       public List<permission> permissions { get; set; }
    }
    public class AdminPerms
    {
        public int AdminId { get; set; }
        public string permissionName { get; set; }
        public int permission { get; set; }
        public bool isAllowed { get; set; }
    }
    public class rolesData
    {
        public int roleID { get; set; }
        public string roleName { get; set; }
        public int numberOfAdmins { get; set; }
        public string roleDescription { get; set; }
        public string policy { get; set; }
    }
    public class rolesDataModel
    {
        public string roleName { get; set; }
        public string roleDescription { get; set; }
        public string policy { get; set; }
    }
    public class rolesCompany
    {
        public List<rolesData> roleDataList { get; set; }
        public List<permission> permissions { get; set; }
        public List<actions> actions { get; set; }
    }
}