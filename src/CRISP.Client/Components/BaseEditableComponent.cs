using CRISP.Core.Common;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace CRISP.Client.Components
{
    public abstract class BaseCreateComponent<TCommand, TCommandValidator, TCommandService> : BaseEditableComponent<TCommand, TCommandValidator>
        where TCommand : CreateCommand, new()
        where TCommandValidator : BaseValidator<TCommand>, new()
        where TCommandService : ICreateService<TCommand>
    {
        protected bool CanGoBack => MudDialog is not null;
        [Inject]
        protected TCommandService CreateService { get; set; } = default!;

        [Inject]
        protected NavigationManager NavigationManager { get; set; } = default!;

        protected override async Task Submit()
        {
            if (MudDialog is not null)
                MudDialog.Close(DialogResult.Ok(command));
            else
            {
                ISnackbar snackbar = ServiceProvider.GetRequiredService<ISnackbar>();
                try
                {
                    Guid id = await CreateService.Send(command);
                    snackbar.Add($"{ModelName} created", Severity.Success);
                    NavigationManager.NavigateTo(NavigationManager.Uri.Replace("/new", $"/{id}"));
                }
                catch (Exception e)
                {
                    snackbar.Add($"Error creating {ModelName}: {e.Message}", Severity.Error);
                }
            }
        }
    }

    public abstract class BaseModifyComponent<TCommand, TCommandValidator, TCommandService> : BaseEditableComponent<TCommand, TCommandValidator>
        where TCommand : ModifyCommand, new()
        where TCommandValidator : BaseValidator<TCommand>, new()
        where TCommandService : ICommandService<TCommand>
    {
        protected bool _loading = true;
        protected bool _editing = false;
        protected Variant variant = Variant.Text;

        [Parameter]
        public int Id { get; set; }

        [Inject]
        protected TCommandService ModifyService { get; set; } = default!;


        protected override async Task Submit()
        {
            if (MudDialog is not null)
                MudDialog.Close(DialogResult.Ok(command));
            else
            {
                try
                {
                    ToggleEdit(false);
                    await ModifyService.Send(command);
                    Snackbar.Add($"{ModelName} updated successfully.", Severity.Success);
                    await OnInitializedAsync(); // Refresh the data
                }
                catch (Exception e)
                {
                    Snackbar.Add($"Error updating {ModelName}: {e.Message}", Severity.Error);
                }
            }
        }

        public void ToggleEdit(bool edit)
        {
            _editing = edit;
            variant = edit ? Variant.Outlined : Variant.Text;
        }
    }

    public abstract class BaseEditableComponent<TCommand, TCommandValidator> : BaseComponent
        where TCommand : class, new()
        where TCommandValidator : BaseValidator<TCommand>, new()
    {
        protected TCommand command = new();
        protected TCommandValidator validator = new();

        [CascadingParameter]
        public IMudDialogInstance? MudDialog { get; set; }

        protected abstract string ModelName { get; }

        [Inject]
        protected ISnackbar Snackbar { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            if (MudDialog is not null)
            {
                await MudDialog.SetOptionsAsync(MudDialog.Options with
                {
                    CloseButton = true,
                });
            }
        }

        protected virtual void Clear()
        {
            command = new();
        }

        protected abstract Task Submit();

    }
}
