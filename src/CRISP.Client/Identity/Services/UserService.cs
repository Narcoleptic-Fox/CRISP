using CRISP.Core.Common;
using CRISP.Core.Exceptions;
using CRISP.Core.Identity;
using System.Net.Http.Json;

namespace CRISP.Client.Identity.Services;

internal sealed class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    public UserService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async ValueTask<User> Send(SingularQuery<User> query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        return await _httpClient.GetFromJsonAsync<User>($"{query.Id}", cancellationToken) ?? throw new NotFoundException(query.Id, nameof(User));
    }
    public async ValueTask<User> Send(GetUserByEmail query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        return await _httpClient.GetFromJsonAsync<User>($"email/{query.Email}", cancellationToken) ?? throw new NotFoundException(query.Email, nameof(User));
    }
    public async ValueTask<PagedResponse<Users>> Send(GetUsers query, CancellationToken cancellationToken = default)
    {
        if (query == null)
            throw new ArgumentNullException(nameof(query));
        string queryString = query.ToQueryString();
        HttpResponseMessage response = await _httpClient.GetAsync(queryString, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to retrieve users: {response.ReasonPhrase}");
        }
        return await response.Content.ReadFromJsonAsync<PagedResponse<Users>>(cancellationToken: cancellationToken)
               ?? new();
    }
    public async ValueTask Send(UpdateUser command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        HttpResponseMessage response = await _httpClient.PutAsJsonAsync("", command, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to update user: {response.ReasonPhrase}");
        }
    }
    public async ValueTask Send(DeleteCommand command, CancellationToken cancellationToken = default)
    {
        if (command == null)
            throw new ArgumentNullException(nameof(command));
        HttpResponseMessage response = await _httpClient.DeleteAsync($"{command.Id}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to delete user: {response.ReasonPhrase}");
        }
    }
}
