using CRISP.Core.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CRISP.Client.Components
{
    public abstract class BaseComponent : MudComponentBase
    {
        protected readonly Dictionary<Type, IEnumerable<BaseModel>> models = new();

        [Inject]
        protected IServiceProvider ServiceProvider { get; set; } = default!;
        public virtual async Task<IList<TReturn>> LoadModels<TReturn, TModel>(CancellationToken cancellationToken)
            where TModel : BaseModel
        {
            IEnumerable<BaseModel> items = await GetModels<TModel>(cancellationToken);
            switch (typeof(TReturn))
            {
                case Type id when id == typeof(int):
                case Type nId when nId == typeof(int?):
                    return items.Select(l => l.Id).Cast<TReturn>().ToList();
                case Type str when str == typeof(string):
                    return items.Select(l => l.ToString()).Distinct().Cast<TReturn>().ToList();
                default:
                    return items.Cast<TReturn>().ToList();
            }
        }

        public virtual async Task<IEnumerable<TReturn>> SearchModels<TReturn, TModel>(string? value, CancellationToken cancellationToken)
            where TModel : BaseModel
        {
            IEnumerable<BaseModel> items = await GetModels<TModel>(cancellationToken);

            items = from i in items
                    where string.IsNullOrEmpty(value) || i.ToString().Contains(value, StringComparison.OrdinalIgnoreCase)
                    select i;

            switch (typeof(TReturn))
            {
                case Type id when id == typeof(int):
                case Type nId when nId == typeof(int?):
                    return items.Select(l => l.Id).Cast<TReturn>();
                case Type str when str == typeof(string):
                    return items.Select(l => l.ToString()).Cast<TReturn>();
                default:
                    return items.Cast<TReturn>();
            }
        }

        public virtual string ModelToString<TInput, TModel>(TInput input) where TModel : BaseModel
        {
            if (input is null)
                return string.Empty;
            else if (input is Guid id)
                return models.TryGetValue(typeof(TModel), out IEnumerable<BaseModel>? items) ? items.FirstOrDefault(s => s.Id == id)?.ToString() ?? string.Empty : string.Empty;
            else if (input is string sId)
                return models.TryGetValue(typeof(TModel), out IEnumerable<BaseModel>? items) ? items.FirstOrDefault(s => s.ToString().Equals(sId, StringComparison.OrdinalIgnoreCase))?.ToString() ?? string.Empty : string.Empty;
            else
                return string.Empty;
        }
        private async Task<IEnumerable<BaseModel>> GetModels<TModel>(CancellationToken cancellationToken)
            where TModel : BaseModel
        {
            Type modelType = typeof(TModel);
            if (!models.TryGetValue(modelType, out IEnumerable<BaseModel>? items) || items?.Any() != true)
            {
                switch (modelType)
                {
                    default:
                        await Task.Delay(100, cancellationToken); // Simulate async loading
                        break;
                }
                models[modelType] = items ?? Enumerable.Empty<BaseModel>();
            }
            return items ?? Enumerable.Empty<BaseModel>();
        }
    }
}
