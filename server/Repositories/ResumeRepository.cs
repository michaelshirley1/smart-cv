using SmartCv.Api.Models;

namespace SmartCv.Api.Repositories;

public interface IItemRepository
{
    IEnumerable<Item> GetAll();
    Item? GetById(int id);
}
