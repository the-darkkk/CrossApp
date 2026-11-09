using System.Security.Cryptography;
using Api.Contracts;
using Api.Validation;
using Core.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Api.Endpoints;

public static class BookEndpoints
{
    public static void MapBooks(this WebApplication app)
    {
        var group = app.MapGroup("/api/books").WithTags("Books");

        // GET /api/books (з підтримкою пагінації, пошуку та фільтрації за автором)
        group.MapGet("/", (ICatalogStore store, string? search, string? author, int? page, int? pageSize) =>
        {
            var list = store.List();

            if (!string.IsNullOrWhiteSpace(author))
            {
                list = list.Where(b => b.Author != null &&
                    b.Author.Contains(author, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                list = list.Where(b =>
                    b.Title.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    b.Isbn.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (b.Author != null && b.Author.Contains(search, StringComparison.OrdinalIgnoreCase))).ToList();
            }

            if (page.HasValue || pageSize.HasValue)
            {
                int p = page.GetValueOrDefault(1);
                if (p < 1) p = 1;
                int ps = pageSize.GetValueOrDefault(20);
                if (ps < 1) ps = 20;

                var pagedItems = list.Skip((p - 1) * ps).Take(ps)
                    .Select(ToDto).ToList();
                return Results.Ok(new PagedResult<BookDto>(pagedItems, list.Count, p, ps));
            }

            return Results.Ok(list.Select(ToDto).ToList());
        });

        // GET /api/books/low-stock?threshold=5
        group.MapGet("/low-stock", (ICatalogStore store, int? threshold) =>
        {
            int limit = threshold.GetValueOrDefault(5);
            var low = store.List().Where(b => b.Quantity < limit).Select(ToDto).ToList();
            return TypedResults.Ok(low);
        });

        // GET /api/books/{id}
        group.MapGet("/{id}", Results<Ok<BookDto>, NotFound> (string id, ICatalogStore store) =>
            store.GetById(id) is { } found
                ? TypedResults.Ok(ToDto(found))
                : TypedResults.NotFound());

        // POST /api/books
        group.MapPost("/", Results<Created<BookDto>, ValidationProblem> (SaveBookRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            var id = Convert.ToHexString(RandomNumberGenerator.GetBytes(4)).ToLowerInvariant();
            var book = Book.Create(id, request.Isbn, request.Title, request.Year, request.Author);
            for (int i = 1; i <= request.Copies; i++)
            {
                book.AddCopy($"BC-{id}-{i:D2}");
            }
            store.Add(book);

            return TypedResults.Created($"/api/books/{book.Id}", ToDto(book));
        }).AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // PUT /api/books/{id}
        group.MapPut("/{id}", Results<Ok<BookDto>, NotFound, ValidationProblem> (string id, SaveBookRequest request, ICatalogStore store) =>
        {
            if (!request.TryValidate(out var errors))
                return TypedResults.ValidationProblem(errors);

            if (store.GetById(id) is null)
                return TypedResults.NotFound();

            var replacement = Book.Create(id, request.Isbn, request.Title, request.Year, request.Author);
            for (int i = 1; i <= request.Copies; i++)
            {
                replacement.AddCopy($"BC-{id}-{i:D2}");
            }
            store.Update(replacement);

            return TypedResults.Ok(ToDto(replacement));
        }).AddEndpointFilter<ValidationFilter<SaveBookRequest>>();

        // DELETE /api/books/{id}
        group.MapDelete("/{id}", Results<NoContent, NotFound> (string id, ICatalogStore store) =>
            store.Remove(id)
                ? TypedResults.NoContent()
                : TypedResults.NotFound());
    }

    private static BookDto ToDto(Book b) =>
        new(b.Id, b.Isbn, b.Title, b.Year, b.Author, b.Copies.Count, b.Unit);
}
