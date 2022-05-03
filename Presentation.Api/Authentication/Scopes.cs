using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api
{
    public static class Scopes
    {
        // This array should match the permissions in Auth0
        public static readonly string[] scopes = new string[]
        {
            "read:tenants",
            "create:tenants",
            "update:tenants",
            "delete:tenants",
            "read:organizations",
            "create:organizations",
            "update:organizations",
            "delete:organizations",
            "read:users",
            "create:users",
            "update:users",
            "delete:users",
            "read:races",
            "create:races",
            "update:races",
            "delete:races",
            "read:waypoints",
            "create:waypoints",
            "update:waypoints",
            "delete:waypoints",
            "read:signs",
            "create:signs",
            "update:signs",
            "delete:signs",
            "read:signtypes",
            "create:signtypes",
            "update:signtypes",
            "delete:signtypes",
            "read:signgroups",
            "create:signgroups",
            "update:signgroups",
            "delete:signgroups",
            "read:drivers",
            "create:drivers",
            "update:drivers",
            "delete:drivers"
        };
    }
}
