using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBookUi.Data;
using MyModel.DTOs;
using MyModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyUtility.CartRepository
{
    public class CartRepository : ICartRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public CartRepository(ApplicationDbContext context, UserManager<IdentityUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<int> AddItem(int bookId, int qty)
        {
            using var transaction = _context.Database.BeginTransaction();
            string userId = GetUserId();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                {
                    cart = new ShoppingCart
                    {
                        UserId = userId
                    };
                    _context.ShoppingCarts.Add(cart);
                }
                _context.SaveChanges();
                var cartItem = _context.CartDetails
                                  .FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if(cartItem is not null)
                {
                    cartItem.Quantity += qty;
                }
                else
                {
                    var book = _context.Books.Find(bookId);
                    cartItem = new CartDetail
                    {
                        BookId = bookId,
                        ShoppingCartId = cart.Id,
                        Quantity = qty,
                        UnitPrice = book.Price

                    };
                    _context.CartDetails.Add(cartItem);
                }
                _context.SaveChanges();
                transaction.Commit();
              
            }
            catch (Exception ex)
            {
                
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }


        public async Task<int> RemoveItem(int bookId)
        {
            //   using var transaction = _context.Database.BeginTransaction();
            string userId = GetUserId();

            try
            {
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("user is not logged-in");

                var cart = await GetCart(userId);
                if (cart is null)
                {
                    throw new Exception("Invalid Cart");
                }
                _context.SaveChanges();
                var cartItem = _context.CartDetails.FirstOrDefault(a => a.ShoppingCartId == cart.Id && a.BookId == bookId);
                if (cartItem is null)
                    throw new Exception("no item in cart");
                else if (cartItem.Quantity == 1)
                {
                    _context.CartDetails.Remove(cartItem);
                }
                else
                {
                    cartItem.Quantity  = cartItem.Quantity - 1;
                }
              
                _context.SaveChanges();
         
             
            }
            catch (Exception ex)
            {
              
            }
            var cartItemCount = await GetCartItemCount(userId);
            return cartItemCount;
        }

        public async Task<ShoppingCart> GetUserCart()
        {
            var userId = GetUserId();
            if (userId == null)
                throw new Exception("Invalid userId");
            var shoppingCart = await _context.ShoppingCarts
                               .Include(a => a.CartDetails)
                               .ThenInclude(a => a.Book)
                               .ThenInclude(a=>a.Stock)
                               .Include(a=>a.CartDetails)
                               .ThenInclude(a => a.Book)
                               .ThenInclude(a => a.Genre)
                               .Where(a => a.UserId == userId).FirstOrDefaultAsync();
            return shoppingCart;
        }


        public async Task<ShoppingCart> GetCart(string userId)
        {
            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(x => x.UserId == userId);
            return cart;
        }

        public async Task<int> GetCartItemCount(string userId = "")
        {
            if (string.IsNullOrEmpty(userId))
            {
                userId =GetUserId();
            }
            var data = await (from cart in _context.ShoppingCarts
                              join CartDetail in _context.CartDetails
                              on cart.Id equals CartDetail.ShoppingCartId
                              where cart.UserId == userId
                              select new { CartDetail.Id}
                              ).ToListAsync();
            return data.Count;
        }


        public async Task<bool> DoCheckout(CheckoutModel model)
        {
            using var transaction = _context.Database.BeginTransaction();
            try
            {
                //enty ==> order, orderdetail
                //remove ==>cartdetail

                var userId = GetUserId();
                if (string.IsNullOrEmpty(userId))
                    throw new Exception("User is not logged-in");
                var cart = await GetCart(userId);
                if (cart is null)
                    throw new Exception("Cart is empty");

                var cartDetail = _context.CartDetails
                        .Where(a=> a.ShoppingCartId == cart.Id).ToList();
                if (cartDetail.Count == 0)
                    throw new Exception("Cart is empty");
                var pendingRecord = _context.orderStatuses.FirstOrDefault
                                     (s => s.StatusName == "Pending");
                if (pendingRecord is null)
                    throw new Exception("Order status does not have Pending status");

                var order = new Order
                {
                    UserId = userId,
                    CreateDate = DateTime.UtcNow,
                    Name = model.Name,
                    Email = model.Email,
                    MobileNumber = model.MobileNumber,
                    PaymentMethod = model.PaymentMethod,
                    Address = model.Address,
                    IsPaid = false,
                    OrderStatusId =pendingRecord.Id
                };
                _context.Orders.Add(order); 
                _context.SaveChanges();

                foreach(var item in cartDetail)
                {
                    var orderDetail = new OrderDetail
                    {
                        BookId = item.BookId,
                        OrderId = order.Id,
                        Quantity = item.Quantity,   
                       UnitPrice = item.UnitPrice

                    };
                    _context.OrderDetails.Add(orderDetail);

                    var stock = await _context.Stocks.FirstOrDefaultAsync(a => a.BookId == item.BookId);
                    if (stock == null)
                    {
                        throw new InvalidOperationException("Stock is null");
                    }

                    if (item.Quantity > stock.Quantity)
                    {
                        throw new InvalidOperationException($"Only {stock.Quantity} items(s) are available in the stock");
                    }
                    // decrease the number of quantity from the stock table
                    stock.Quantity -= item.Quantity;
                }
            
             //   _context.SaveChanges();



                //removing cartdetail
                _context.CartDetails.RemoveRange(cartDetail);
                _context.SaveChanges();
                transaction.Commit();
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        private string GetUserId()
        {
            var principal = _httpContextAccessor.HttpContext.User;
            string userId = _userManager.GetUserId(principal);
            return userId;
        }

    }
}
