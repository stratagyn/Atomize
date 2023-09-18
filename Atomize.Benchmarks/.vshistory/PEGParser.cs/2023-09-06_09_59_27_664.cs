using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using static Atomize.Parse;

using PEGParser = Atomize.Parser<Atomize.Benchmarks.PEGToken>;

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
        AtomRule = Handle(
            Map(
                Choice(
                    Text.Identifier ,
                    Text.RegularExpression,
                    Text.SingleQuotedString),
                literal => new PEGToken()
                {
                    Label = "ATOM",
                    Value = new string(literal.Span),
                    Children = Array.Empty<PEGToken>()
                }),
            _ => SurroundedBy.Parentheses(ChoiceRule!));

        PredicateRule = Bind(
            Optional(Choice(Atom('&'), Atom('!'))),
            predicate => Map(
                AtomRule,
                atom => predicate == '\0'
                    ? atom
                    : new PEGToken
                    {
                        Label = "PREDICATE",
                        Value = $"{predicate}{atom.Value}",
                        Children = new PEGToken[] { atom }
                    }));

        GreedyRule = Bind(
            PredicateRule,
            predicate => Map(
                Optional(Choice(Atom('*'), Atom('+'), Atom('?'))),
                greedy => greedy == '\0'
                    ? predicate
                    : new PEGToken
                    {
                        Label = "GREEDY",
                        Value = $"{predicate.Value}{greedy}",
                        Children = new PEGToken[] { predicate }
                    }));

        SequenceRule = Map(
            Split(GreedyRule, Atom(' ')),
            tokens => new PEGToken
            {
                Label = "SEQUENCE",
                Value = $"[{string.Join(" ", tokens.Select(token => token.Value))}]",
                Children = tokens.ToArray()
            });

        ChoiceRule = Map(
            Split(SequenceRule, SurroundedBy.Whitespace(Atom('|'))),
            tokens => new PEGToken
            {
                Label = "CHOICE",
                Value = $"({string.Join("|", tokens.Select(token => token.Value))})",
                Children = tokens.ToArray()
            });

        RuleRule = Bind(
            Text.Identifier,
            identifier => Bind(
                Ignore(SurroundedBy.Whitespace(Atom(':'))),
                _ => Bind(
                    ChoiceRule,
                    choice => Map(
                        Ignore(SurroundedBy.Whitespace(Atom(';'))),
                        _ => new PEGToken
                        {
                            Label = "RULE",
                            Value = $"{new string(identifier.Span)}:{choice.Value}",
                            Children = new PEGToken[] {
                                    new PEGToken {
                                        Label = "IDENTIFIER",
                                        Value = new string(identifier.Span),
                                        Children = Array.Empty<PEGToken>()
                                        },
                                    choice }
                        }))));

        GrammarRule = Bind(
            Minimum(1, RuleRule),
            rules => Map(
                EndOfText,
                _ => new PEGToken
                {
                    Label = "GRAMMAR",
                    Value = $"{string.Join(";\n", rules.Select(token => token.Value))}",
                    Children = rules.ToArray()
                }));
    }

    public static IParseResult<PEGToken> Parse(string grammar) => 
        GrammarRule(new TextScanner(grammar));
}
