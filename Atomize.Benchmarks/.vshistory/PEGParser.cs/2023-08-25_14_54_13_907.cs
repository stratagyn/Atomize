using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PEGParser = Atomize.Parser<(string Label, Atomize.IParseResult<string>)>;

namespace Atomize.Benchmarks;

public readonly struct PEGToken
{
    public readonly string Label;
    public readonly string Value;

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
        AtomRule = Map()
    }
}
