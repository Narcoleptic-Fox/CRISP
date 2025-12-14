using CRISP.Core.Extensions;
using System.Linq.Expressions;

namespace CRISP.Core.Extensions
{
    public static partial class IQueryableExtensions
    {
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName, bool descending = false) =>
            descending ? source.OrderByDescending(propertyName) : source.OrderBy(propertyName);

        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        public static IQueryable<T> OrderByDescending<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderByDescending(ToLambda<T>(propertyName));
        }

        private static Expression<Func<T, object>> ToLambda<T>(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName) ||
                typeof(T).GetProperty(propertyName) is null)
                propertyName = Query.SortBy;

            ParameterExpression parameter = Expression.Parameter(typeof(T));
            MemberExpression property = Expression.Property(parameter, propertyName);
            UnaryExpression propAsObject = Expression.Convert(property, typeof(object));

            return Expression.Lambda<Func<T, object>>(propAsObject, parameter);
        }
    }
}