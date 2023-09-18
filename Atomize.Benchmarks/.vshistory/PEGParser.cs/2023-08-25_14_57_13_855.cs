using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Parser;

using PEGParser = Atomize.Parser<Atomize.Benchmarks.PEGToken>;

namespace Atomize.Benchmarks;

public readonly struct PEGToken
{
    public string Label { get; }
    public string Value { get; }
    
    public IList<PEGToken> Children { get; }
}

public static class PEGTokenizer
{
    public static readonly PEGParser GrammarRule;
    public static readonly PEGParser RuleRule;
    public static readonly PEGParser ChoiceRule;
    public static readonly PEGParser SequenceRule;
    public static readonly PEGParser GreedyRule;
    public static readonly PEGParser PredicateRule;
    public static readonly PEGParser AtomRule;

    static PEGTokenizer()
    {
        AtomRule = Map(
            Choose(
                Map(
                    Choose(
                        Token(Pattern.Text.Identifier),
                        Token(Pattern.Text.RegularExpression),
                        Token(Pattern.Text.SingleQuotedString)),

                    )
    }
}
