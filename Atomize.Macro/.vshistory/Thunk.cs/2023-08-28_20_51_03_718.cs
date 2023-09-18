using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize.Thunk;

public class Thunk<T>
{
    private readonly Func<T> _func;

    public Thunk(Func<T> func) => _func = func;

    public Thunk<U> Map<U>(Func<T, U> f) => new(() => f(_func()));
}
