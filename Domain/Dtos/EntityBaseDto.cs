using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Dtos
{
    public abstract class EntityBaseDto
    {
        public string? id { get; protected internal set; }
    }
}
