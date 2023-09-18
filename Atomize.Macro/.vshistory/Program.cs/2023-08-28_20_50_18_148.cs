using Atomize.Macro;

static Thunk<T> Fix<T>(Thunk<Func<Thunk<T>, T>> lazy) =>
    lazy.Map(f => )