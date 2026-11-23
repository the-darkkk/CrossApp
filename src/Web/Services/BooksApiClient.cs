using System.Net;
using System.Net.Http.Json;
using Contracts;

namespace Web.Services;

public sealed class BooksApiClient(HttpClient http)
{
    private sealed record ProblemDetailsDto(string? Title, string? Detail);

    public async Task<PagedResult<BookDto>> GetBooksAsync(
        string? search, string? author, string? sortBy,
        bool sortDesc, int page, int pageSize,
        CancellationToken ct = default)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(author)) qs.Add($"author={Uri.EscapeDataString(author)}");
        if (!string.IsNullOrWhiteSpace(sortBy)) qs.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        if (sortDesc) qs.Add("sortDesc=true");
        qs.Add($"page={page}");
        qs.Add($"pageSize={pageSize}");

        var url = "/api/books?" + string.Join("&", qs);
        return await http.GetFromJsonAsync<PagedResult<BookDto>>(url, ct)
               ?? new PagedResult<BookDto>([], 0, page, pageSize);
    }

    public async Task<BookDto?> GetBookAsync(string id, CancellationToken ct = default)
    {
        var resp = await http.GetAsync($"/api/books/{id}", ct);
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<BookDto>(ct);
    }

    public async Task<List<BookDto>> GetLowStockAsync(int threshold = 5, CancellationToken ct = default)
    {
        return await http.GetFromJsonAsync<List<BookDto>>($"/api/books/low-stock?threshold={threshold}", ct)
               ?? [];
    }

    public async Task<(BookDto? book, Dictionary<string, string[]>? errors, string? conflictMessage)>
        CreateBookAsync(SaveBookRequest req, CancellationToken ct = default)
    {
        var resp = await http.PostAsJsonAsync("/api/books", req, ct);
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<BookDto>(ct), null, null);

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var problem = await resp.Content.ReadFromJsonAsync<ProblemDetailsDto>(ct);
            return (null, null, problem?.Detail ?? problem?.Title ?? "Конфлікт доменних правил.");
        }

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemResponse>(ct);
            return (null, problem?.Errors, null);
        }

        return (null, null, $"Помилка сервера: {(int)resp.StatusCode}");
    }

    public async Task<(BookDto? book, Dictionary<string, string[]>? errors, bool notFound)>
        UpdateBookAsync(string id, SaveBookRequest req, CancellationToken ct = default)
    {
        var resp = await http.PutAsJsonAsync($"/api/books/{id}", req, ct);
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<BookDto>(ct), null, false);

        if (resp.StatusCode == HttpStatusCode.NotFound)
            return (null, null, true);

        if (resp.StatusCode == HttpStatusCode.Conflict)
        {
            var problem = await resp.Content.ReadFromJsonAsync<ProblemDetailsDto>(ct);
            var msg = problem?.Detail ?? problem?.Title ?? "Конфлікт доменних правил.";
            return (null, new Dictionary<string, string[]> { [nameof(SaveBookRequest.Isbn)] = [msg] }, false);
        }

        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var problem = await resp.Content.ReadFromJsonAsync<ValidationProblemResponse>(ct);
            return (null, problem?.Errors, false);
        }

        return (null, null, false);
    }

    public async Task<bool> DeleteBookAsync(string id, CancellationToken ct = default)
    {
        var resp = await http.DeleteAsync($"/api/books/{id}", ct);
        return resp.IsSuccessStatusCode;
    }
}