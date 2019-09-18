using System;

namespace DependencyInjectionWorkshop
{
    public class AlarmAttribute : Attribute
    {
        public string RoleId { get; set; }
    }
}