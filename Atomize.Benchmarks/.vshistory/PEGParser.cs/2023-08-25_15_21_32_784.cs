using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Parse;

using PEGParser = Atomize.Parse<Atomize.Benchmarks.PEGToken>;

namespace Atomize.Benchmarks;

public readonly struct PEGToken
{
    public string Label { get; init; }

    public string Value { get; init; }
    
    public PEGToken[] Children { get; init; }
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
        AtomRule = Choice(
            Map(
                Choice(
                    Token(Pattern.Text.Identifier),
                    Token(Pattern.Text.RegularExpression),
                    Token(Pattern.Text.SingleQuotedString)),
                literal => new PEGToken()
                {
                    Label = "ATOM",
                    Value = new string(literal.Span),
                    Children = Array.Empty<PEGToken>()
                }),
            Island(
                Token('('),
                ChoiceRule!,
                Token(')')));

        PredicateRule = Bind(
            Optional(Choice(Token('&'), Token('!'))),
            predicate => Map(
                AtomRule,
                atom => predicate.Length == 0
                    ? atom
                    : new PEGToken
                    {
                        Label = "PREDICATE",
                        Value = $"{predicate.Value}",
                        Children = new PEGToken[] { atom }
                    }));

        GreedyRule = Bind(
            PredicateRule,
            predicate => Map(
                Optional(Choice(Token('*'), Token('+'), Token('?'))),
                greedy => greedy == '\0'
                    ? predicate.Value
                    : new PEGToken
                    {
                        Label = "GREEDY",
                        Value = $"{greedy}",
                        Children = new PEGToken[] { predicate.Value }
                    }));

        SequenceRule = Map(
            Split(GreedyRule, Token(' ')),
            tokens => new PEGToken
            {
                Label = "SEQUENCE",
                Value = 
            })
    }
}
