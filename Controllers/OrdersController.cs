using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketResell.Data;
using TicketResell.Models;
using TicketResell.Models.Dto;
using TicketResell.Services;

namespace TicketResell.Controllers
{
    [Authorize]
    public class OrdersController : Controller
    {
        private const long MaxTicketFileBytes = 5 * 1024 * 1024;

        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public OrdersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }

        // GET: Orders/Buy/5
        public async Task<IActionResult> Buy(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var listing = await _context.TicketListings
                .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id.Value);
            if (listing == null)
            {
                return NotFound();
            }

            if (listing.SellerId == _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (listing.Status != ListingStatus.Active || listing.Quantity <= 0 || listing.Event!.StartsAt <= DateTime.Now)
            {
                TempData["Error"] = "That listing is no longer available.";
                return RedirectToAction("Index", "TicketListings");
            }

            return View(listing);
        }

        // POST: Orders/Buy/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Buy(int id, int quantity)
        {
            var listing = await _context.TicketListings
                .Include(l => l.Event)!.ThenInclude(e => e!.Venue)
                .Include(l => l.Seller)
                .FirstOrDefaultAsync(l => l.Id == id);
            if (listing == null)
            {
                return NotFound();
            }

            var buyerId = _userManager.GetUserId(User)!;
            if (listing.SellerId == buyerId)
            {
                return Forbid();
            }

            if (listing.Status != ListingStatus.Active || listing.Quantity <= 0 || listing.Event!.StartsAt <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "That listing is no longer available.");
            }
            else if (quantity < 1)
            {
                ModelState.AddModelError(nameof(quantity), "Choose at least one ticket.");
            }
            else if (quantity > listing.Quantity)
            {
                ModelState.AddModelError(nameof(quantity),
                    $"Only {listing.Quantity} ticket(s) are left on this listing.");
            }

            if (!ModelState.IsValid)
            {
                return View(listing);
            }

            var subtotal = quantity * listing.PricePerTicket;
            var buyerFee = FeeSchedule.BuyerFee(subtotal);

            var order = new Order
            {
                BuyerId = buyerId,
                TicketListingId = listing.Id,
                Quantity = quantity,
                SubtotalPrice = subtotal,
                BuyerFee = buyerFee,
                SellerCommission = FeeSchedule.SellerCommission(subtotal),
                TotalPrice = subtotal + buyerFee,
                OrderedAt = DateTime.Now,
                OrderStatus = OrderStatus.Pending
            };

            listing.Quantity -= quantity;
            if (listing.Quantity == 0)
            {
                listing.Status = ListingStatus.Sold;
            }

            _context.Orders.Add(order);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                TempData["Error"] = "Someone took those tickets a moment ago. Nothing was reserved for you.";
                return RedirectToAction("Details", "TicketListings", new { id });
            }

            return RedirectToAction(nameof(Checkout), new { id = order.Id });
        }

        // GET: Orders/Checkout/12
        public async Task<IActionResult> Checkout(int id)
        {
            var order = await LoadOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.BuyerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(order);
        }

        // POST: Orders/Checkout/12
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Checkout")]
        public async Task<IActionResult> Pay(int id, string? cardName, string? cardNumber, string? expiry, string? cvc)
        {
            var order = await LoadOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.BuyerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            var digits = new string((cardNumber ?? string.Empty).Where(char.IsDigit).ToArray());

            if (string.IsNullOrWhiteSpace(cardName))
            {
                ModelState.AddModelError(nameof(cardName), "Enter the name on the card.");
            }

            if (digits.Length != 16)
            {
                ModelState.AddModelError(nameof(cardNumber), "The card number must be 16 digits.");
            }

            var expiryParts = (expiry ?? string.Empty).Split('/', StringSplitOptions.TrimEntries);
            var isValidExpiry = false;
            if (expiryParts.Length == 2
                && int.TryParse(expiryParts[0], out var expiryMonth)
                && int.TryParse(expiryParts[1], out var expiryYear)
                && expiryMonth is >= 1 and <= 12
                && expiryYear is >= 0 and <= 99)
            {
                var endOfExpiryMonth = new DateTime(2000 + expiryYear, expiryMonth, 1).AddMonths(1);
                isValidExpiry = endOfExpiryMonth > DateTime.Now;
            }

            if (!isValidExpiry)
            {
                ModelState.AddModelError(nameof(expiry), "Use MM/YY, with a date in the future.");
            }

            if ((cvc ?? string.Empty).Count(char.IsDigit) != 3)
            {
                ModelState.AddModelError(nameof(cvc), "The security code is 3 digits.");
            }

            if (digits.Length == 16 && digits.EndsWith("0000"))
            {
                ModelState.AddModelError(string.Empty, "The payment was declined. Try another card.");
            }

            if (order.TicketListing?.Event?.StartsAt <= DateTime.Now)
            {
                ModelState.AddModelError(string.Empty, "That event has already started. You have not been charged.");
            }

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            order.OrderStatus = OrderStatus.Paid;
            order.PaidAt = DateTime.Now;
            order.PaymentReference = $"SIM-{Guid.NewGuid().ToString("N")[..10].ToUpperInvariant()}";

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Payment confirmed. Reference {order.PaymentReference}.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Orders/Details/12
        public async Task<IActionResult> Details(int id)
        {
            var order = await LoadOrderAsync(id, includeFiles: true);
            if (order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isBuyer = order.BuyerId == userId;
            var isSeller = order.TicketListing?.SellerId == userId;

            if (!isBuyer && !isSeller)
            {
                return Forbid();
            }

            return View(new OrderView { Order = order, IsBuyer = isBuyer, IsSeller = isSeller });
        }

        // POST: Orders/Cancel/12
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await LoadOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.BuyerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Pending)
            {
                TempData["Error"] = "Only an unpaid order can be cancelled.";
                return RedirectToAction(nameof(Details), new { id });
            }

            if (order.TicketListing != null)
            {
                Reservations.ReturnTickets(order, order.TicketListing);
            }

            order.OrderStatus = OrderStatus.Cancelled;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Order cancelled and the tickets went back on sale.";
            return RedirectToAction(nameof(MyOrders));
        }

        // POST: Orders/Refund/12
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Refund(int id)
        {
            var order = await LoadOrderAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            if (order.TicketListing?.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Paid && order.OrderStatus != OrderStatus.Delivered)
            {
                TempData["Error"] = "Only a paid order can be refunded.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var listing = order.TicketListing;
            if (listing != null && listing.Status != ListingStatus.Cancelled && listing.Event!.StartsAt > DateTime.Now)
            {
                Reservations.ReturnTickets(order, listing);
            }

            order.OrderStatus = OrderStatus.Refunded;
            order.RefundedAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Message"] = $"Order {order.Id} refunded.";
            return RedirectToAction(nameof(Sales));
        }

        // GET: Orders/Deliver/12
        public async Task<IActionResult> Deliver(int id)
        {
            var order = await LoadOrderAsync(id, includeFiles: true);
            if (order == null)
            {
                return NotFound();
            }

            if (order.TicketListing?.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Paid && order.OrderStatus != OrderStatus.Delivered)
            {
                TempData["Error"] = "Tickets can only be sent for a paid order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(order);
        }

        // POST: Orders/Deliver/12
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deliver(int id, List<IFormFile> tickets)
        {
            var order = await LoadOrderAsync(id, includeFiles: true);
            if (order == null)
            {
                return NotFound();
            }

            if (order.TicketListing?.SellerId != _userManager.GetUserId(User))
            {
                return Forbid();
            }

            if (order.OrderStatus != OrderStatus.Paid && order.OrderStatus != OrderStatus.Delivered)
            {
                TempData["Error"] = "Tickets can only be sent for a paid order.";
                return RedirectToAction(nameof(Details), new { id });
            }

            var uploads = tickets.Where(f => f.Length > 0).ToList();
            if (uploads.Count == 0)
            {
                ModelState.AddModelError(nameof(tickets), "Choose at least one file.");
            }

            if (order.TicketFiles.Count + uploads.Count > order.Quantity)
            {
                ModelState.AddModelError(nameof(tickets),
                    $"This order is for {order.Quantity} ticket(s), so you can upload at most that many files.");
            }

            foreach (var file in uploads)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!UploadTypes.IsAllowed(extension))
                {
                    ModelState.AddModelError(nameof(tickets), $"{file.FileName}: only {UploadTypes.AllowedList} files are accepted.");
                }

                if (file.Length > MaxTicketFileBytes)
                {
                    ModelState.AddModelError(nameof(tickets), $"{file.FileName}: files must be 5 MB or smaller.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(order);
            }

            var folder = Path.Combine(_environment.ContentRootPath, "App_Data", "tickets");
            Directory.CreateDirectory(folder);

            foreach (var file in uploads)
            {
                var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
                var storedName = $"{Guid.NewGuid():N}{extension}";

                await using (var stream = System.IO.File.Create(Path.Combine(folder, storedName)))
                {
                    await file.CopyToAsync(stream);
                }

                _context.TicketFiles.Add(new TicketFile
                {
                    OrderId = order.Id,
                    FileName = Path.GetFileName(file.FileName),
                    StoredName = storedName,
                    ContentType = UploadTypes.ContentTypeFor(extension),
                    SizeBytes = file.Length,
                    UploadedAt = DateTime.Now
                });
            }

            order.OrderStatus = OrderStatus.Delivered;
            order.DeliveredAt = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["Message"] = "Tickets sent to the buyer.";
            return RedirectToAction(nameof(Details), new { id });
        }

        // GET: Orders/Download/3
        public async Task<IActionResult> Download(int id)
        {
            var file = await _context.TicketFiles
                .Include(f => f.Order)!.ThenInclude(o => o!.TicketListing)
                .FirstOrDefaultAsync(f => f.Id == id);
            if (file?.Order == null)
            {
                return NotFound();
            }

            var userId = _userManager.GetUserId(User);
            var isBuyer = file.Order.BuyerId == userId;
            var isSeller = file.Order.TicketListing?.SellerId == userId;

            if (!isBuyer && !isSeller)
            {
                return Forbid();
            }

            var stillPaid = file.Order.OrderStatus == OrderStatus.Paid || file.Order.OrderStatus == OrderStatus.Delivered;
            if (isBuyer && !stillPaid)
            {
                TempData["Error"] = "That order was refunded, so its tickets can no longer be downloaded.";
                return RedirectToAction(nameof(Details), new { id = file.OrderId });
            }

            var path = Path.Combine(_environment.ContentRootPath, "App_Data", "tickets", file.StoredName);
            if (!System.IO.File.Exists(path))
            {
                return NotFound();
            }

            var extension = Path.GetExtension(file.StoredName);
            return PhysicalFile(path, UploadTypes.ContentTypeFor(extension), file.FileName);
        }

        // GET: Orders/MyOrders
        public async Task<IActionResult> MyOrders()
        {
            var userId = _userManager.GetUserId(User);

            var orders = await _context.Orders
                .Where(o => o.BuyerId == userId)
                .Include(o => o.TicketFiles)
                .Include(o => o.TicketListing)!.ThenInclude(l => l!.Event)!.ThenInclude(e => e!.Venue)
                .Include(o => o.TicketListing)!.ThenInclude(l => l!.Seller)
                .OrderByDescending(o => o.OrderedAt)
                .ToListAsync();

            return View(orders);
        }

        // GET: Orders/Sales
        public async Task<IActionResult> Sales()
        {
            var userId = _userManager.GetUserId(User);

            var sales = await _context.Orders
                .Where(o => o.TicketListing!.SellerId == userId
                            && (o.OrderStatus == OrderStatus.Paid
                                || o.OrderStatus == OrderStatus.Delivered
                                || o.OrderStatus == OrderStatus.Refunded))
                .Include(o => o.Buyer)
                .Include(o => o.TicketFiles)
                .Include(o => o.TicketListing)!.ThenInclude(l => l!.Event)!.ThenInclude(e => e!.Venue)
                .OrderByDescending(o => o.OrderedAt)
                .ToListAsync();

            return View(sales);
        }

        private Task<Order?> LoadOrderAsync(int id, bool includeFiles = false)
        {
            var query = _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.TicketListing)!.ThenInclude(l => l!.Event)!.ThenInclude(e => e!.Venue)
                .Include(o => o.TicketListing)!.ThenInclude(l => l!.Seller)
                .AsQueryable();

            if (includeFiles)
            {
                query = query.Include(o => o.TicketFiles);
            }

            return query.FirstOrDefaultAsync(o => o.Id == id);
        }
    }
}
