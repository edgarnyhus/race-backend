using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Dtos
{
    public abstract class EntityBaseDto
    {
        public string? Id { get; protected internal set; }
    }
}
