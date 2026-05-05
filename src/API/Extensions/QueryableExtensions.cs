using API.Models;

namespace API.Extensions
{
    public static class QueryableExtensions
    {
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int first, int rows)
        {
            int pageSize = rows > 0 ? rows : 10;
            int skip = first >= 0 ? first : 0;

            return query.Skip(skip).Take(pageSize);
        }
    }
}
