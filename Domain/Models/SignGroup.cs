using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Models;

public class SignGroup : EntityBase
{
    [ForeignKey("TenantId")]
    public Guid? TenantId { get; set; }
    public string? Name { get; set; }
    public ICollection<Sign>? Signs { get; set; }
    public string? Notes { get; set; }
    public Guid? OrganizationId { get; set; }
    public virtual Organization? Organization { get; set; }
}