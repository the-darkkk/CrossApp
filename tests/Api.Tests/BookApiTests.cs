using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.Contracts;
using Core;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Api.Tests;

public class BookApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public BookApiTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        var response = await _client.GetAsync("/health");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("ok", body);
    }

    [Fact]
    public async Task Post_ValidBook_Returns201Created_AndGetByLocation_Returns200Ok()
    {
        var request = new SaveBookRequest("978-0-13-235088-4", "Чистий код", 2008, "Роберт Мартін", 3);

        var postResponse = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

        var location = postResponse.Headers.Location;
        Assert.NotNull(location);

        var created = await postResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(created);
        Assert.Equal(request.Isbn, created.Isbn);
        Assert.Equal(request.Title, created.Title);
        Assert.Equal(request.Year, created.Year);
        Assert.Equal(request.Author, created.Author);
        Assert.Equal(request.Copies, created.Copies);

        var getResponse = await _client.GetAsync(location);
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetched = await getResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched.Id);
        Assert.Equal(created.Title, fetched.Title);
    }

    [Fact]
    public async Task Post_InvalidBook_Returns400BadRequest_WithProblemDetails()
    {
        // Невалідний запит: порожні isbn та назва, від'ємна кількість примірників, некоректний рік
        var request = new SaveBookRequest("", "", 1200, null, -1);

        var response = await _client.PostAsJsonAsync("/api/books", request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("errors", body, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Get_NonExistentBook_Returns404NotFound()
    {
        var response = await _client.GetAsync("/api/books/non-existent-book-999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_ExistingBook_UpdatesAndReturns200Ok()
    {
        var createReq = new SaveBookRequest("978-0-201-61622-4", "The Pragmatic Programmer", 1999, "Hunt & Thomas", 2);
        var postResponse = await _client.PostAsJsonAsync("/api/books", createReq);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(created);

        var updateReq = new SaveBookRequest("978-0-201-61622-4", "The Pragmatic Programmer (20th Anniv)", 2019, "Hunt & Thomas", 5);
        var putResponse = await _client.PutAsJsonAsync($"/api/books/{created.Id}", updateReq);
        Assert.Equal(HttpStatusCode.OK, putResponse.StatusCode);

        var updated = await putResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(updated);
        Assert.Equal("The Pragmatic Programmer (20th Anniv)", updated.Title);
        Assert.Equal(2019, updated.Year);
        Assert.Equal(5, updated.Copies);
    }

    [Fact]
    public async Task Put_NonExistentBook_Returns404NotFound()
    {
        var updateReq = new SaveBookRequest("978-0-123456", "Невідома книга", 2020, "Автор", 1);
        var response = await _client.PutAsJsonAsync("/api/books/no-such-id-404", updateReq);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Put_InvalidBook_Returns400BadRequest()
    {
        var createReq = new SaveBookRequest("978-0-321-12521-7", "DDD", 2003, "Eric Evans", 1);
        var postResponse = await _client.PostAsJsonAsync("/api/books", createReq);
        var created = await postResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(created);

        var invalidUpdate = new SaveBookRequest("", "", 1000, null, -5);
        var putResponse = await _client.PutAsJsonAsync($"/api/books/{created.Id}", invalidUpdate);
        Assert.Equal(HttpStatusCode.BadRequest, putResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ExistingBook_Returns204NoContent_AndSubsequentGetReturns404()
    {
        var createReq = new SaveBookRequest("978-0-596-52068-7", "JavaScript: The Good Parts", 2008, "Douglas Crockford", 1);
        var postResponse = await _client.PostAsJsonAsync("/api/books", createReq);
        Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);
        var created = await postResponse.Content.ReadFromJsonAsync<BookDto>(JsonOptions);
        Assert.NotNull(created);

        var delResponse = await _client.DeleteAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, delResponse.StatusCode);

        var getResponse = await _client.GetAsync($"/api/books/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_NonExistentBook_Returns404NotFound()
    {
        var response = await _client.DeleteAsync("/api/books/no-such-book-to-del");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Get_WithPagination_ReturnsPagedResult()
    {
        var response = await _client.GetAsync("/api/books?page=1&pageSize=2");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var paged = await response.Content.ReadFromJsonAsync<PagedResult<BookDto>>(JsonOptions);
        Assert.NotNull(paged);
        Assert.Equal(1, paged.Page);
        Assert.Equal(2, paged.PageSize);
        Assert.True(paged.Total > 0);
        Assert.True(paged.Items.Count <= 2);
    }

    [Fact]
    public async Task Get_WithAuthorFilter_ReturnsFiltered()
    {
        var request = new SaveBookRequest("978-0-13-110362-7", "The C Programming Language", 1988, "Kernighan", 2);
        await _client.PostAsJsonAsync("/api/books", request);

        var response = await _client.GetAsync("/api/books?author=Kernighan");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<BookDto>>(JsonOptions);
        Assert.NotNull(items);
        Assert.NotEmpty(items);
        Assert.All(items, b => Assert.Contains("Kernighan", b.Author, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task Get_WithSearch_ReturnsMatchingBooks()
    {
        var response = await _client.GetAsync("/api/books?search=Code");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<BookDto>>(JsonOptions);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task LowStock_ReturnsBooksWithFewCopies()
    {
        var response = await _client.GetAsync("/api/books/low-stock?threshold=5");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var items = await response.Content.ReadFromJsonAsync<List<BookDto>>(JsonOptions);
        Assert.NotNull(items);
        Assert.All(items, b => Assert.True(b.Copies < 5));
    }

    [Fact]
    public async Task Post_WhenDomainConflictOccurs_Returns409Conflict()
    {
        var clientWithConflict = _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<ICatalogStore, ConflictCatalogStore>();
            });
        }).CreateClient();

        var request = new SaveBookRequest("978-0-13-235088-4", "Конфліктна книга", 2021, "Автор", 1);
        var response = await clientWithConflict.PostAsJsonAsync("/api/books", request);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("Domain Rule Conflict", body);
        Assert.Contains("Конфлікт бізнес-правила", body);
    }

    private sealed class ConflictCatalogStore : ICatalogStore
    {
        public IReadOnlyList<Book> List() => [];
        public Book? GetById(string id) => null;
        public void Add(Book item) => throw new DomainException("Конфлікт бізнес-правила: дублікат сутності книги");
        public void Update(Book item) => throw new DomainException("Конфлікт бізнес-правила при оновленні");
        public bool Remove(string id) => false;
        public IReadOnlyList<Book> Search(Func<Book, bool> predicate) => [];
        public IReadOnlyList<Book> GetLowStock(int threshold = 5) => [];
    }
}
