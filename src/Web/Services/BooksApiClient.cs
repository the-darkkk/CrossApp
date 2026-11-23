using System.Net;
using System.Net.Http.Json;
using Contracts;

namespace Web.Services;

public sealed class BooksApiClient(IHttpClientFactory factory)
{
    private HttpClient Http => factory.CreateClient("BooksApi");

    public async Task<PagedResult<BookDto>> GetBooksAsync(
        string? search, string? author, string? sortBy,
        bool sortDesc, int page, int pageSize)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(search)) qs.Add($"search={Uri.EscapeDataString(search)}");
        if (!string.IsNullOrWhiteSpace(author)) qs.Add($"author={Uri.EscapeDataString(author)}");
        if (!string.IsNullOrWhiteSpace(sortBy)) qs.Add($"sortBy={Uri.EscapeDataString(sortBy)}");
        if (sortDesc) qs.Add("sortDesc=true");
        qs.Add($"page={page}");
        qs.Add($"pageSize={pageSize}");

        var url = "/api/books?" + string.Join("&", qs);
        return await Http.GetFromJsonAsync<PagedResult<BookDto>>(url)
               ?? new PagedResult<BookDto>([], 0, page, pageSize);
    }

    public async Task<BookDto?> GetBookAsync(string id)
    {
        var resp = await Http.GetAsync($"/api/books/{id}");
        if (resp.StatusCode == HttpStatusCode.NotFound) return null;
        resp.EnsureSuccessStatusCode();
        return await resp.Content.ReadFromJsonAsync<BookDto>();
    }

    public async Task<List<BookDto>> GetLowStockAsync(int threshold = 5)
    {
        return await Http.GetFromJsonAsync<List<BookDto>>($"/api/books/low-stock?threshold={threshold}")
               ?? [];
    }

    public async Task<(BookDto? book, Dictionary<string, string[]>? errors, string? conflictMessage)>
        CreateBookAsync(SaveBookRequest req)
    {
        var resp = await Http.PostAsJsonAsync("/api/books", req);
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<BookDto>(), null, null);
        if (resp.StatusCode == HttpStatusCode.Conflict)
            return (null, null, await resp.Content.ReadAsStringAsync());
        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var errs = await resp.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
            return (null, errs, null);
        }
        return (null, null, $"Помилка сервера: {resp.StatusCode}");
    }

    public async Task<(BookDto? book, Dictionary<string, string[]>? errors, bool notFound)>
        UpdateBookAsync(string id, SaveBookRequest req)
    {
        var resp = await Http.PutAsJsonAsync($"/api/books/{id}", req);
        if (resp.IsSuccessStatusCode)
            return (await resp.Content.ReadFromJsonAsync<BookDto>(), null, false);
        if (resp.StatusCode == HttpStatusCode.NotFound)
            return (null, null, true);
        if (resp.StatusCode == HttpStatusCode.BadRequest)
        {
            var errs = await resp.Content.ReadFromJsonAsync<Dictionary<string, string[]>>();
            return (null, errs, false);
        }
        return (null, null, false);
    }

    public async Task<bool> DeleteBookAsync(string id)
    {
        var resp = await Http.DeleteAsync($"/api/books/{id}");
        return resp.IsSuccessStatusCode;
    }
}
