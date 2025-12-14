using CRISP.Client.Common;
using CRISP.Core.Common;
using Humanizer;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CRISP.Client.Components
{
    public abstract class BaseAuditableTableComponent<TModel, TState, TPagedQuery, TAddDialog, TCreateCommand, TService> : BaseCrudTableComponent<TModel, TState, TPagedQuery, TAddDialog, TCreateCommand, TService>
        where TModel : BaseAuditableModel
        where TState : IState, new()
        where TPagedQuery : PagedQuery<TModel>, new()
        where TAddDialog : ComponentBase
        where TCreateCommand : CreateCommand
        where TService : IQueryService<TPagedQuery, PagedResponse<TModel>>, ICreateService<TCreateCommand>, IModifyService<DeleteCommand>, IModifyService<ArchiveCommand>
    {
        public virtual async void Archive(TModel model)
        {
            IDialogReference dialog = await DialogService.ShowAsync<ConfirmationDialog>("Warning!", new DialogParameters
            {
                { nameof(ConfirmationDialog.ContentText), $"Are you sure you want to archive {ModelName}({model})?" },
                { nameof(ConfirmationDialog.IsArchiving), true }
            });
            DialogResult? result = await dialog.Result;
            if (result?.Data is not null &&
                result.Data is string reason)
            {
                try
                {
                    await ModelService.Send(new ArchiveCommand(model.Id, reason));
                    Snackbar.Add($"{ModelName}({model}) archived successfully.", Severity.Success);
                    await dataGrid.ReloadServerData();
                }
                catch (Exception e)
                {
                    Snackbar.Add($"Failed to archive {ModelName}({model}): {e.Message}", Severity.Error);
                }
            }
        }
    }
    public abstract class BaseCrudTableComponent<TModel, TState, TPagedQuery, TAddDialog, TCreateCommand, TService> :
        BaseStateTableComponent<TModel, TState>
        where TModel : BaseModel
        where TState : IState, new()
        where TPagedQuery : PagedQuery<TModel>, new()
        where TAddDialog : ComponentBase
        where TCreateCommand : CreateCommand
        where TService : IQueryService<TPagedQuery, PagedResponse<TModel>>, ICreateService<TCreateCommand>, IModifyService<DeleteCommand>
    {

        protected string ModelName => typeof(TModel).Name.Singularize().Titleize();

        [Inject]
        protected TService ModelService { get; set; } = default!;

        [Inject]
        protected IDialogService DialogService { get; set; } = default!;

        [Inject]
        protected ISnackbar Snackbar { get; set; } = default!;

        public override async void Add()
        {
            IDialogReference dialog = await DialogService.ShowAsync<TAddDialog>();
            DialogResult? result = await dialog.Result;
            if (result?.Data is not null &&
                result.Data is TCreateCommand command)
            {
                try
                {
                    await ModelService.Send(command);
                    Snackbar.Add($"{ModelName} created successfully.", Severity.Success);
                    await dataGrid.ReloadServerData();
                }
                catch (Exception e)
                {
                    Snackbar.Add($"Failed to create {ModelName}: {e.Message}", Severity.Error);
                }
            }
        }

        public override async void Remove(TModel model)
        {
            IDialogReference dialog = await DialogService.ShowAsync<ConfirmationDialog>("Warning!", new DialogParameters
        {
            { nameof(ConfirmationDialog.ContentText), $"Are you sure you want to delete {ModelName}({model})?" }
        });
            DialogResult? result = await dialog.Result;
            if (result?.Canceled != true)
            {
                try
                {
                    await ModelService.Send(new DeleteCommand(model.Id));
                    Snackbar.Add($"{ModelName}({model}) deleted successfully.", Severity.Success);
                    await dataGrid.ReloadServerData();
                }
                catch (Exception e)
                {
                    Snackbar.Add($"Failed to delete {ModelName}({model}): {e.Message}", Severity.Error);
                }
            }
        }
    }


    public abstract class BaseStateTableComponent<TModel, TState> : BaseTableComponent<TModel>
        where TModel : BaseModel
        where TState : IState, new()
    {
        protected TState State { get; set; } = new();

        [Inject]
        protected StateContainer<TState> StateContainer { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            State = await StateContainer.GetStateFromStorage();
        }

        public async ValueTask DisposeAsync()
        {
            await StateContainer.SetStateToStorage(State);
        }
    }

    public abstract class BaseTableComponent<TModel> : BaseComponent
        where TModel : BaseModel
    {
        protected MudDataGrid<TModel> dataGrid = default!;
        public abstract void Add();
        public abstract void Remove(TModel model);
        protected abstract Task<GridData<TModel>> ServerData(GridState<TModel> state);
    }
}
