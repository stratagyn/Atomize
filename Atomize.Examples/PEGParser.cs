using Atomize;

using static Atomize.Parse;

using GrammarDict = 
   System.Collections.Generic.Dictionary<string, Atomize.Examples.IParsingExpression>;

namespace Atomize.Examples;

public enum ExpressionType
{
   OrderedChoice,
   Sequence,
   Repetition,
   Assertion,
   Literal,
   Identifier
}

public interface IParsingExpression
{
   public ExpressionType Type { get; }
}

public record OrderedChoice : IParsingExpression
{
   public OrderedChoice(params IParsingExpression[] expressions) =>
      Expressions = expressions;

   public IParsingExpression[] Expressions { get; }

   public ExpressionType Type => ExpressionType.OrderedChoice;

   public override string ToString()
   {
      var choices = string.Join(" | ", Expressions.Select(c => c.ToString()));
      return $"{{{Type}, {choices}}}";
   }
}


public record Sequence : IParsingExpression
{
   public Sequence(params IParsingExpression[] expressions) =>
      Expressions = expressions;

   public IParsingExpression[] Expressions { get; }

   public ExpressionType Type => ExpressionType.Sequence;

   public override string ToString()
   {
      var exprs = string.Join(" ", Expressions.Select(c => c.ToString()));
      return $"{{{Type}, {exprs}}}";
   }
}

public record Repetition : IParsingExpression
{
   public Repetition(IParsingExpression expression, int min, int max) =>
      (Expression, Min, Max) = (expression, min, max);

   public IParsingExpression Expression { get; }

   public int Max { get; }

   public int Min { get; }

   public ExpressionType Type => ExpressionType.Repetition;

   public override string ToString() =>
      (Min, Max) switch
      {
         (0, 1) => $"{{{Type}, {Expression}?}}",
         (0, -1) => $"{{{Type}, {Expression}*}}",
         _ => $"{{{Type}, {Expression}+}}"
      };
}

public record Assertion : IParsingExpression
{
   public Assertion(IParsingExpression expression, bool isPositive) =>
      (Expression, IsPositive) = (expression, isPositive);

   public IParsingExpression Expression { get; }

   public bool IsPositive { get; }

   public ExpressionType Type => ExpressionType.Assertion;

   public override string ToString() =>
      $"{{{Type}, {(IsPositive ? '&' : '!')}{Expression}}}";
}

public record Literal : IParsingExpression
{
   public Literal(ReadOnlyMemory<char> text, bool isRegex = false) =>
      (Text, IsRegex) = (text, isRegex);

   public ExpressionType Type => ExpressionType.Literal;

   public bool IsRegex { get; }

   public ReadOnlyMemory<char> Text { get; }

   public override string ToString() =>
      $"{{{Type}, '{Text}'}}";
}

public record Identifier : IParsingExpression
{
   public Identifier(ReadOnlyMemory<char> text) =>
      Text = text;

   public ExpressionType Type => ExpressionType.Literal;

   public ReadOnlyMemory<char> Text { get; }

   public override string ToString() =>
      $"{{{Type}, {Text}}}";
}

public static class PEGParser
{
   private static readonly
   Parser<IParsingExpression> AtomicRule =
      Choice(
         from str in Multiple.QuotedString
         select (IParsingExpression)new Literal(str[1..^1]),

         from pat in Multiple.RegularExpression
         select (IParsingExpression)new Literal(pat[1..^1], true),

         from idn in Multiple.Identifier
         select (IParsingExpression)new Identifier(idn),

         Grouped(Ref(() => OrderedChoiceRule!))
      );

   private static readonly
   Parser<IParsingExpression> AssertionRule =
      from expr in AtomicRule
      from asserts in Optional(Choice('&', '!'))
      select asserts == '\0'
         ? expr
         : new Assertion(expr, asserts == '&');

   private static readonly
   Parser<IParsingExpression> RepetitionRule =
      from expr in AssertionRule
      from op in Optional(Choice('*', '+', '?'))
      select op == '\0'
         ? expr
         : new Repetition(
            expr,
            op == '+' ? 1 : 0,
            op == '?' ? 1 : -1
         );

   private static readonly
   Parser<IParsingExpression> SequenceRule =
      from exprs in SeparatedBy(RepetitionRule, Multiple.Whitespace)
      select exprs.Count == 1
         ? exprs[0]
         : new Sequence(exprs.ToArray());

   private static readonly
   Parser<IParsingExpression> OrderedChoiceRule =
      from exprs in SeparatedBy(SequenceRule, Whitespaced(Atom('|')))
      select exprs.Count == 1
         ? exprs[0]
         : new OrderedChoice(exprs.ToArray());

   private static readonly
   Parser<KeyValuePair<string, IParsingExpression>> RuleRule =
      from name in (
         from mem in Multiple.Identifier
         select new string(mem.Span)
      )
      from _ in Whitespaced(Atom(':'))
      from expr in OrderedChoiceRule
      from __ in Whitespaced(Atom(';'))
      select new KeyValuePair<string, IParsingExpression>(name, expr);

   public static readonly
   Parser<GrammarDict> GrammarRule =
      from rules in Minimum(1, RuleRule)
      from _ in (Parser<char>)EndOfText
      select new GrammarDict(rules);

   private static Parser<T> Grouped<T>(Parser<T> parser) =>
   Island(
      Bind(Atom('('), _ => Optional(Multiple.Whitespace)),
      parser,
      Bind(Optional(Multiple.Whitespace), _ => Atom(')'))
   );

   private static Parser<T> Whitespaced<T>(Parser<T> parser) =>
      Island(
         Optional(Multiple.Whitespace),
         parser,
         Optional(Multiple.Whitespace)
      );
}
