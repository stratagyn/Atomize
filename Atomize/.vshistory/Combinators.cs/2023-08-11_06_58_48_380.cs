using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Failure;

namespace Atomize;

public static partial class Parser
{
    public static Parser<T> Choose<T>(params Parser<T>[] rules) => 
        (TokenReader reader) =>
        {
            if (rules.Length == 0)
                return new EmptyToken<T>(reader.Offset);

            var startingAt = reader.Offset;
            var errors = new List<string>();

            foreach (var rule in rules)
            {
                var result = rule(reader);

                if (result.IsToken)
                    return result;

                errors.Add(result.Conflict);
            }

            return new Failure<T>(startingAt, $"{{{string.Join(" | ", errors)}}}");
        };

    public static Parser<IList<T>> Fallback<T>(params Parser<T>[] parsers) =>
        (TokenReader reader) =>
        {
            var startingAt = reader.Offset;

            if (parsers.Length == 0)
                return new EmptyToken<IList<T>>(startingAt);

            var firstResult = parsers[0](reader);

            if (!firstResult.IsToken)
                return Undo<IList<T>>(
                    reader, firstResult.Offset, startingAt, firstResult.Conflict);

            IList<T> matched = new List<T>() { firstResult. };

            for (var i = 1; i < parsers.Length; i++)
            {
                var result = parsers[i](reader);

                if (!result.IsToken)
                {
                    reader.Backtrack(reader.Offset - startingAt + firstResult.Length);

                    return ParseResult.Succeed<IList<T>>(
                        startingAt,
                        firstResult.Match,
                        UnitArray(firstResult.Token!));
                }

                if (result.Length > 0)
                    matched.Add(result.Token!);
            }

            return ParseResult.Succeed(startingAt, reader.Text[startingAt..reader.Offset], matched);
        };
}
