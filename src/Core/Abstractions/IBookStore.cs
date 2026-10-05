using Core.Domain;

namespace Core.Abstractions;

/// <summary>
/// Контракт сховища книг: визначає операції зберігання без зазначення
/// способу зберігання даних. Містить лише ті методи, які потрібні сервісу.
/// </summary>
public interface IBookStore
{
    /// <summary>
    /// Повертає список усіх книг у сховищі.
    /// IReadOnlyList — щоб зовнішній код не мутував сховище в обхід Add/Update/Remove.
    /// </summary>
    IReadOnlyList<Book> List();

    /// <summary>
    /// Повертає книгу за ідентифікатором або null, якщо не знайдено.
    /// Book? — nullable, бо запису може не бути.
    /// </summary>
    Book? GetById(string id);

    /// <summary>
    /// Додає нову книгу до сховища. Перевіряє унікальність id.
    /// </summary>
    void Add(Book item);

    /// <summary>
    /// Оновлює існуючу книгу у сховищі.
    /// </summary>
    void Update(Book item);

    /// <summary>
    /// Видаляє книгу зі сховища за ідентифікатором.
    /// Повертає true, якщо запис було знайдено і видалено.
    /// </summary>
    bool Remove(string id);

    /// <summary>
    /// Додаткове завдання 2: Пошук з предикатом — підготовка до тижнів 6-7.
    /// </summary>
    IReadOnlyList<Book> Search(Func<Book, bool> predicate);
}
