using Atomize.Thunk;

static Thunk<T> Fix<T>(Thunk<Func<Thunk<T>, T>> lazy) =>
    lazy.Map(f => f(Fix(lazy)));

static Func<Thunk<int>, Thunk<int>> Fibr(Thunk<Func<Thunk<int>, int>> lazyFib) =>
    lazyN =>
        lazyFib.Bind(
            fib => lazyN.Map(
                n => n < 2 
                    ? n
                    : fib(new(() => n - 2)) + fib(new(() => n - 1))));

static
