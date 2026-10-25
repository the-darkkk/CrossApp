using Core.Domain;
using Core.Events;
using Core.Services;
using Core.Storage;
using Xunit;

namespace Core.Tests;

public sealed class CatalogServiceTests
{
    [Fact]
    public void Receive_WithNegativeQuantity_Throws_AndKeepsState()
    {
        var store = new InMemoryBookStore();
        var service = new CatalogService(store);
        var p = service.Add("SKU-1", "Кабель", 2020, "Автор", initialCopies: 10);

        Assert.Throws<ArgumentOutOfRangeException>(() => service.Receive(p.Id, -5));
        Assert.Equal(10, store.GetById(p.Id)!.Quantity);
    }

    [Fact]
    public void Add_RaisesChangedExactlyOnce()
    {
        var service = new CatalogService(new InMemoryBookStore());
        var events = new List<CatalogChangedEventArgs>();
        service.Changed += (_, e) => events.Add(e);

        service.Add("SKU-2", "Гвинт", 2024, "Автор", initialCopies: 7);

        Assert.Single(events);
        Assert.Equal(CatalogChangeKind.BookAdded, events[0].Kind);
    }

    [Fact]
    public void ReturnCopy_WhenCopyNotIssued_ThrowsDomainException_AndKeepsState()
    {
        var store = new InMemoryBookStore();
        var service = new CatalogService(store);
        var book = service.Add("978-0-13-235088-4", "Clean Code", 2008, "Robert Martin", initialCopies: 1);
        var copy = book.Copies[0];

        // створення активної видачі для невиданого примірника
        var dummyLoan = Loan.Create("L-DUMMY", copy.Id, "R-001", DateTime.Now.AddDays(-2), null, LoanStatus.Active);

        var ex = Assert.Throws<DomainException>(() => service.ReturnCopy(book.Id, copy.Id, dummyLoan));
        Assert.Contains("не був виданий", ex.Message);
        Assert.False(copy.IsIssued);
    }

    [Fact]
    public void IssueCopy_WhenAlreadyIssued_ThrowsDomainException()
    {
        var store = new InMemoryBookStore();
        var service = new CatalogService(store);
        var book = service.Add("978-0-13-235088-4", "Clean Code", 2008, "Robert Martin", initialCopies: 1);
        var copy = book.Copies[0];

        // перша видача успішна
        service.IssueCopy(book.Id, copy.Id, "R-001");

        // повторна видача того ж примірника має викликати DomainException
        var ex = Assert.Throws<DomainException>(() => service.IssueCopy(book.Id, copy.Id, "R-002"));
        Assert.Contains("вже виданий", ex.Message);
    }
}
