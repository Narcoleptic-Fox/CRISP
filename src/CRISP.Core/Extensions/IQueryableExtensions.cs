using CRISP.Core.Extensions;
using System.Linq.Expressions;

namespace CRISP.Core.Extensions
{
    /// <summary>
    /// Provides queryable helpers for sorting and paging collections using shared query defaults.
    /// </summary>
    public static partial class IQueryableExtensions
    {
        /// <summary>
        /// Sorts a queryable source by the requested property and direction.
        /// </summary>
        /// <typeparam name="T">The type of element in the queryable source.</typeparam>
        /// <param name="source">The queryable source to sort.</param>
        /// <param name="propertyName">The property name to sort by, or <c>null</c> to use <see cref="Query.SortBy"/>.</param>
        /// <param name="descending">Whether to sort descending, or <c>null</c> to use <see cref="Query.SortDescending"/>.</param>
        /// <returns>The queryable source ordered by the selected property and direction.</returns>
        public static IQueryable<T> SortBy<T>(this IQueryable<T> source, string? propertyName, bool? descending = false) =>
            (descending ?? Query.SortDescending) ? source.OrderByDescending(propertyName ?? Query.SortBy) : source.OrderBy(propertyName ?? Query.SortBy);

        /// <summary>
        /// Sorts a queryable source in ascending order by a property name.
        /// </summary>
        /// <typeparam name="T">The type of element in the queryable source.</typeparam>
        /// <param name="source">The queryable source to sort.</param>
        /// <param name="propertyName">The property name to sort by.</param>
        /// <returns>The queryable source ordered ascending by the selected property.</returns>
        public static IQueryable<T> OrderBy<T>(this IQueryable<T> source, string propertyName)
        {
            return source.OrderBy(ToLambda<T>(propertyName));
        }

        /// <summary>
        /// Sorts a queryable source in descending order by a property name.
        /// </summary>
        /// <typeparam name="T">The type of element in the queryable source.</typeparam>
        /// <param name="source">The queryable source to sort.</param>
        /// <param name="propertyName">The property name to sort by.</param>
        /// <returns>The queryable source ordered descending by the selected property.</returns>
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

        /// <summary>
        /// Applies zero-based paging to a queryable source.
        /// </summary>
        /// <typeparam name="T">The type of element in the queryable source.</typeparam>
        /// <param name="source">The queryable source to page.</param>
        /// <param name="page">The zero-based page number, or <c>null</c> to use <see cref="Query.Page"/>.</param>
        /// <param name="pageSize">The number of items per page, or <c>null</c> to use <see cref="Query.PageSize"/>.</param>
        /// <returns>The queryable source with <see cref="Queryable.Skip{TSource}(IQueryable{TSource}, int)"/> and <see cref="Queryable.Take{TSource}(IQueryable{TSource}, int)"/> applied.</returns>
        public static IQueryable<T> ToPaged<T>(this IQueryable<T> source, int? page, int? pageSize) =>
            source.Skip((page ?? Query.Page) * (pageSize ?? Query.PageSize)).Take(pageSize ?? Query.PageSize);
    }
}