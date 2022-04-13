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
            "get:sign",
            "put:sign",
            "post:sign",
            "delete:sign",
            "get:race",
            "put:vehicles",
            "post:race",
            "delete:race",   
            "get:tenant",
            "put:tenant",
            "post:tenant",
            "delete:tenant",   
            "get:tenant/user",
            "put:tenant/user",
            "post:tenant/user",
            "delete:tenant/user",
            "get:tenant/roles",
            "get:tenant/user/roles",
            "put:tenant/user/roles",
            "delete:tenant/user/roles",
            "get:usersettings",
            "post:usersettings",
            "put:usersettings",
            "get:driver",
            "put:driver",
            "post:driver",
            "delete:driver",
        };
    }
}
