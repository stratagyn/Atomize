using static Atomize.Failure;

namespace Atomize
{

    public delegate IParseResult<T> Parser<T>(TextScanner scanner);
}