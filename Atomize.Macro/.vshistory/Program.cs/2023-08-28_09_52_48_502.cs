using static Atomize.Macros;

var macro = 
    Channel(-1).Bind(
        _ => Write("#HTML utility macro:").Bind(
            _ => Var("i", 0).Bind(
                _ => Var(
                    "H2", 
                    (string title) =>
                        Get<>))))
