using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize;

public interface IParseResult<T>
{
    public bool IsToken { get; }
    public int Length { get; }
    public int Offset { get; }
    public string Conflict { get; }
    public T? SemanticValue { get; }
}
