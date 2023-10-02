using static Atomize.Failure;

namespace Atomize;

public static partial class Parse
{
   public static Parser<U> Bind<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);

          if (!result.IsMatch)
             return Undo<U>(scanner, result.Offset, at, result.Why);

          var boundResult = bind(result.Value!)(scanner);

          if (!boundResult.IsMatch)
             return Undo<U>(scanner, boundResult.Offset, at, boundResult.Why);

          return new Token<U>(at, scanner.Offset - at, boundResult.Value!);
       };

   public static Parser<U> If<T, U>(Parser<T> parser, Parser<U> then, Parser<U> alt) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);

          scanner.Offset = at;

          if (!result.IsMatch)
          {
             var handleResult = alt(scanner);

             if (!handleResult.IsMatch)
                return Undo<U>(scanner, handleResult.Offset, at, handleResult.Why);

             return new Token<U>(at, scanner.Offset - at, handleResult.Value!);
          }

          var boundResult = then(scanner);

          if (!boundResult.IsMatch)
             return Undo<U>(scanner, boundResult.Offset, at, boundResult.Why);

          return new Token<U>(at, scanner.Offset - at, boundResult.Value!);
       };

   public static Parser<U> Map<T, U>(Parser<T> parser, Func<T, U> map) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var result = parser(scanner);

          if (!result.IsMatch)
             return Undo<U>(scanner, result.Offset, at, result.Why);

          return new Token<U>(result.Offset, result.Length, map(result.Value!));
       };

   public static Parser<(T Partial, IParseResult<U> Full)> Partial<T, U>(Parser<T> parser, Func<T, Parser<U>> bind) =>
       (TextScanner scanner) =>
       {
          var at = scanner.Offset;
          var firstResult = parser(scanner);

          if (!firstResult.IsMatch)
             return Undo<(T, IParseResult<U>)>(scanner, firstResult.Offset, at, firstResult.Why);

          var secondResult = bind(firstResult.Value!)(scanner);

          if (!secondResult.IsMatch)
          {
             scanner.Offset = at + firstResult.Length;

             return new Token<(T, IParseResult<U>)>(
                  at,
                  firstResult.Length,
                  (firstResult.Value!, secondResult));
          }

          return new Token<(T, IParseResult<U>)>(
               at,
               firstResult.Length + secondResult.Length,
               (firstResult.Value!, secondResult));
       };

   public static Parser<U> Select<T, U>(this Parser<T> parser, Func<T, U> map) =>
      Map(parser, map);

   public static Parser<U> SelectMany<T, U>(this Parser<T> parser, Func<T, Parser<U>> then) =>
      Bind(parser, then);

   public static Parser<V> SelectMany<T, U, V>(this Parser<T> parser, Func<T, Parser<U>> then, Func<T, U, V> combine) =>
      Bind(
         parser,
         t => Map(
            then(t),
            u => combine(t, u)));

   public static Parser<T> Where<T>(this Parser<T> parser, Func<T, bool> satisfies) =>
      Satisfies(parser, satisfies);
}