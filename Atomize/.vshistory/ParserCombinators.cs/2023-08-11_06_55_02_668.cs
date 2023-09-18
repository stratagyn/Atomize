using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Atomize;

public static partial class Parser
{
    public static Parser<T> Choose<T>(params Parser<T>[] rules) => 
        (TokenReader reader) =>
        {
            if (rules.Length == 0)
                return new EmptyToken<T>(reader.Offset);

            var offset = reader.Offset;
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                var result = rule(reader);

                if (result.IsSuccess)
                    return new Lexeme(result.Match, offset);

                errors.Add(result.ReasonForFailure);
            }

            return new Failure($"{{{string.Join(" | ", errors)}}}", offset);
        };
}
