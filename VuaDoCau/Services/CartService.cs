using VuaDoCau.Extensions;
using VuaDoCau.Models;

namespace VuaDoCau.Services;

public class CartService
{
    private const string KEY = "CART";
    private readonly IHttpContextAccessor _http;
    public CartService(IHttpContextAccessor http) => _http = http;

    private ISession S => _http.HttpContext!.Session;

    public List<CartItem> Get()
        => S.GetObject<List<CartItem>>(KEY) ?? new List<CartItem>();

    private void Save(List<CartItem> items) => S.SetObject(KEY, items);

    public void AddOrIncrease(CartItem item)
    {
        var items = Get();
        var exist = items.FirstOrDefault(x => x.ProductId == item.ProductId);
        if (exist == null) items.Add(item);
        else exist.Quantity += item.Quantity;
        Save(items);
    }

    public void Update(int productId, int qty)
    {
        var items = Get();
        var it = items.FirstOrDefault(x => x.ProductId == productId);
        if (it != null)
        {
            it.Quantity = Math.Max(1, qty);
            Save(items);
        }
    }

    public void Remove(int productId)
    {
        var items = Get();
        items.RemoveAll(x => x.ProductId == productId);
        Save(items);
    }

    public void Clear() => Save(new());
}