using Atomize;
using static Atomize.Macros;

var macro =
    Channel(-1).Bind(
        _ => Write("#HTML utility macro:").Bind(
            _ => Var("i", 0).Bind(
                _ => Var(
                    "H2",
                    (string title) =>
                        Get<int>("i", index =>
                            Update("i", index + 1).Bind(
                                _ => Text($"<h2>{index}. {title}</h2>")))).Bind(
                    _ => Channel(1).Bind(
                        _ => Get<Func<string, Macro>>("H2", H2 => H2("First Section")).Bind(
                            section1 => Get<Func<string, Macro>>("H2", H2 => H2("Second Section")).Bind(
                                section2 => Get<Func<string, Macro>>("H2", H2 => H2("First Section")).Bind(
                                    conclusion => WriteLine(section1, section2, conclusion).Bind(
                                        _ => Read(1).Bind(text => Text($"<HTML>\n{text}</HTML>")))))))))));

var macro2 = Sequence(
    Channel(-1),
    Write("#HTML utility macro:"),
    Var("i", 0),
    Var(
        "H2", 
        (string title) => Get<int>(
            "i", 
            i => Update("i", i + 1).Bind(_ => Text($"<h2>{i}. {title}</h2>"))))

    )
    .Bind(
        _ => .Bind(
            _ => .Bind(
                _ => .Bind(
                    _ => Channel(1).Bind(
                        _ => Get<Func<string, Macro>>("H2", H2 => H2("First Section")).Bind(
                            section1 => Get<Func<string, Macro>>("H2", H2 => H2("Second Section")).Bind(
                                section2 => Get<Func<string, Macro>>("H2", H2 => H2("First Section")).Bind(
                                    conclusion => WriteLine(section1, section2, conclusion).Bind(
                                        _ => Read(1).Bind(text => Text($"<HTML>\n{text}</HTML>")))))))))));
