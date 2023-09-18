using Atomize.Thunk;

static Thunk<T> Fix<T>(Thunk<Func<Thunk<T>, T>> lazy) =>
    lazy.Map(f => f(Fix(lazy)));

static