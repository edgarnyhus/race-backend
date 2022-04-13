using System.Collections.Generic;
using Domain.Models;

namespace Domain.Interfaces;

public interface ISignpostRepository : IRepository<Signpost>
{
    List<KeyValuePair<int, string>> GetSignpostStates();
}