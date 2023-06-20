using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookShoppingCart.Repositories
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpcontextAccessor;

        public CartRepository(ApplicationDbContext db, UserManager<IdentityUser> userManager, IHttpContextAccessor httpcontextAccessor)
        {
            _db = db;
            _userManager = userManager;
            _httpcontextAccessor = httpcontextAccessor;
        }

        public async Task<bool> AddItem(int bookId, int qty) 
        {
            using var transaction = _db.Database.BeginTransaction();
            try
            {
                string userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return false;
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _db.ShoppingCarts.Add(cart);
                }
                _db.SaveChanges();
                // cart details section
                var cartItem = _db.CartDetails.FirstOrDefault(a=>a.ShoppingCartId==cart.Id && a.BookId ==bookId);
                if (cartItem is not null) 
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty
                    };
                    _db.CartDetails.Add(cartItem);
                }
                _db.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> RemoveItem(int bookId)
        {
            //using var transaction = _db.Database.BeginTransaction();
            try
            {
                string userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    return false;
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    return false;
                }
                // cart details section
                var cartItem = _db.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                {
                    return false;
                }
                else if  (cartItem.Quantity==1)
                {
                    _db.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity= cartItem.Quantity - 1;
                }
                _db.SaveChanges();
                //transaction.Commit();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<IEnumerable<ShoppingCart>> GetUserCart()
        {
           var userId= GetUserId();
            if (userId == null)
                throw new Exception("Invalid userid");
           var  shoppingCart =_db.ShoppingCarts
                                 .Include(a=>a.CartDetails)
                                 .ThenInclude(a=>a.Book)
                                 .ThenInclude(a=>a.Genre)
                                 .Where(a=>a.UserId==userId).ToList();  
                return shoppingCart;        
        }

        private async Task<ShoppingCart> GetCart(string userId) 
        {
            var  cart = await _db.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);  
            return cart;
        }

        private string GetUserId()
        {
            var principal = _httpcontextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }
    }
}

