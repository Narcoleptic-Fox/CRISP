using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;
using FluentValidation;

namespace CRISP.Validators
{
    /// <summary>
    /// Base validator for query operations that return a specific response type.
    /// </summary>
    /// <typeparam name="TQuery">The type of query to validate.</typeparam>
    /// <typeparam name="TResponse">The type of response returned by the query.</typeparam>
    /// <remarks>
    /// Provides a foundation for implementing validation rules for read operations.
    /// </remarks>
    internal abstract class QueryValidator<TQuery, TResponse> : AbstractValidator<TQuery>
        where TQuery : Query<TResponse>
    {
    }

    /// <summary>
    /// Validator for queries that retrieve a single entity by identifier.
    /// </summary>
    /// <typeparam name="TQuery">The type of singular query to validate.</typeparam>
    /// <typeparam name="TResponse">The type of model returned by the query.</typeparam>
    /// <remarks>
    /// Includes built-in validation to ensure the entity identifier is provided.
    /// </remarks>
    internal abstract class SingularQueryValidator<TQuery, TResponse> : QueryValidator<TQuery, TResponse>
        where TQuery : SingularQuery<TResponse>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SingularQueryValidator{TQuery, TResponse}"/> class
        /// with validation for the entity identifier.
        /// </summary>
        public SingularQueryValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    /// <summary>
    /// Validator for queries that return filtered collections of entities.
    /// </summary>
    /// <typeparam name="TQuery">The type of filtered query to validate.</typeparam>
    /// <typeparam name="TResponse">The type of model in the filtered collection.</typeparam>
    /// <remarks>
    /// Use this validator for implementing validation rules specific to filtered list operations.
    /// </remarks>
    internal abstract class FilteredQueryValidator<TQuery, TResponse> : QueryValidator<TQuery, FilteredResponse<TResponse>>
        where TQuery : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {
    }

    /// <summary>
    /// Validator for queries that return paginated collections of entities.
    /// </summary>
    /// <typeparam name="TQuery">The type of paged query to validate.</typeparam>
    /// <typeparam name="TResponse">The type of model in the paginated collection.</typeparam>
    /// <remarks>
    /// Includes built-in validation for pagination parameters to ensure they are within valid ranges.
    /// </remarks>
    internal abstract class PagedQueryValidator<TQuery, TResponse> : FilteredQueryValidator<TQuery, TResponse>
        where TQuery : PagedQuery<TResponse>
        where TResponse : BaseModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedQueryValidator{TQuery, TResponse}"/> class
        /// with validation for pagination parameters.
        /// </summary>
        public PagedQueryValidator()
        {
            RuleFor(x => x.PageIndex)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Page must be greater than or equal to 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0.");
        }
    }
}
