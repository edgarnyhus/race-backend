using System;

namespace Application.Helpers
{
    public static class GuidExtensions
    {
        public static Guid? CheckGuid(Guid? id)
        {
            Guid? guid = id;
            if (id == null || id == Guid.Empty)
                guid = Guid.NewGuid();
            return guid;
        }

        public static string CheckGuid(string id)
        {
            Guid guid = Guid.Empty;
            var isValid = Guid.TryParse(id, out guid);
            if (!isValid && string.IsNullOrEmpty(id))
                guid = Guid.NewGuid();
            return guid.ToString();
        }
    }
}