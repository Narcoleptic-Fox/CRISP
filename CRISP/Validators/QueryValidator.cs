using CRISP.Models;
using CRISP.Requests;
using CRISP.Responses;
using FluentValidation;

namespace CRISP.Validators
{
    internal abstract class QueryValidator<TQuery, TResponse> : AbstractValidator<TQuery>
        where TQuery : Query<TResponse>
    {
    }

    internal abstract class SingularQueryValidator<TQuery, TResponse> : QueryValidator<TQuery, TResponse>
        where TQuery : SingularQuery<TResponse>
        where TResponse : BaseModel
    {
        public SingularQueryValidator() => RuleFor(x => x.Id)
                .NotEmpty()
                .WithMessage("Id is required.");
    }

    internal abstract class FilteredQueryValidator<TQuery, TResponse> : QueryValidator<TQuery, FilteredResponse<TResponse>>
        where TQuery : FilteredQuery<TResponse>
        where TResponse : BaseModel
    {
    }

    internal abstract class PagedQueryValidator<TQuery, TResponse> : FilteredQueryValidator<TQuery, TResponse>
        where TQuery : PagedQuery<TResponse>
        where TResponse : BaseModel
    {
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
