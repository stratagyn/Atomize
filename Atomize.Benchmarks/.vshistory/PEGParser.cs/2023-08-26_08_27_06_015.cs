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
        AtomRule = Handle(
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
            _ => Island(Token('('), ChoiceRule!, Token(')')));

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
                Value = $"[{string.Join(", ", tokens.Select(token => token.Value))}]",
                Children = tokens.ToArray()
            });

        ChoiceRule = Map(
            Split(
                SequenceRule, 
                Island(
                    Optional(Token(Pattern.Text.Whitespace)), 
                    Token('|'), 
                    Optional(Token(Pattern.Text.Whitespace)))),
            tokens => new PEGToken
            {
                Label = "CHOICE",
                Value = $"{string.Join("| ", tokens.Select(token => token.Value))}",
                Children = tokens.ToArray()
            });

        RuleRule = Bind(
            Token(Pattern.Text.Identifier),
            identifier => Bind(
                Ignore(
                    Island(
                        Optional(Token(Pattern.Text.Whitespace)), 
                        Token(':'), 
                        Optional(Token(Pattern.Text.Whitespace)))),
                _ => Map(
                    ChoiceRule,
                    choice =>
                    new PEGToken
                    {
                        Label = "RULE",
                        Value = $"{new string(identifier.Value.Span)}:{choice.Value}",
                        Children = new PEGToken[] { 
                            new PEGToken {
                                Label = "IDENTIFIER",
                                Value = new string(identifier.Value.Span),
                                Children = Array.Empty<PEGToken>()
                                },
                            choice }
                    })));

        GrammarRule = Bind(
            Minimum(
                1,
                Bind(
                    RuleRule,
                    rule =>
                        Bind(
                            Ignore(
                                Island(
                                    Optional(Token(Pattern.Text.Whitespace)), 
                                    Token(';'), 
                                    Optional(Token(Pattern.Text.Whitespace)))),
                            _ => Succeed(rule)))),
            rules => Bind(
                Optional(Token('$')),
                eot => eot.Length > 0
                    ? Bind(
                        EndOfText,
                        _ => new PEGToken
                        {
                            Label = "GRAMMAR",
                            Value = $"{string.Join(";\n", rules.Value!.Select(token => token.Value))}",
                            Children = rules.Value!.ToArray()
                        })
                    : Succeed(
                        new Lexeme<PEGToken>(
                            rules.Offset, 
                            rules.Length, 
                            new PEGToken
                            {
                                Label = "GRAMMAR",
                                Value = $"{string.Join(";\n", rules.Value!.Select(token => token.Value))}",
                                Children = rules.Value!.ToArray()
                            }))));
    }

    public static IParseResult<PEGToken> Parse(string grammar) => 
        GrammarRule(new TextScanner(grammar));
}
