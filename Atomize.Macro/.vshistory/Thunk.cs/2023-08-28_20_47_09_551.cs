using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize.Macro;

public class Thunk<T>
{
    private readonly Func<T> _func;

    public Thunk(Func<T> func) => _func = func;

    public Thunk<U> Map<T, U>()
}
