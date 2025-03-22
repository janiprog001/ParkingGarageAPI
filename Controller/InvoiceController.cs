using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using ParkingGarageAPI.Services;
using System.Security.Claims;
using System.IO;

namespace ParkingGarageAPI.Controller;

[Route("api/invoices")]
[ApiController]
[Authorize]
public class InvoiceController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IInvoiceService _invoiceService;
    
    public InvoiceController(ApplicationDbContext context, IInvoiceService invoiceService)
    {
        _context = context;
        _invoiceService = invoiceService;
    }
    
    // Felhasználó számláinak lekérdezése
    [HttpGet]
    public IActionResult GetUserInvoices()
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            var invoices = _context.Invoices
                .Where(i => i.CustomerEmail == userEmail)
                .OrderByDescending(i => i.IssueDate)
                .ToList();
                
            return Ok(invoices);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Számla részleteinek lekérdezése
    [HttpGet("{id}")]
    public async Task<IActionResult> GetInvoiceById(int id)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (invoice == null)
                return NotFound("A számla nem található");
                
            // Csak a saját számláit láthatja a felhasználó, kivéve ha admin
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (invoice.CustomerEmail != userEmail && !user.IsAdmin)
                return Forbid("Nincs jogosultságod ehhez a számlához");
                
            return Ok(invoice);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Számla letöltése PDF formátumban
    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadInvoice(int id)
    {
        try
        {
            var userEmail = User.FindFirstValue(ClaimTypes.Name);
            
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == id);
                
            if (invoice == null)
                return NotFound("A számla nem található");
                
            // Csak a saját számláit töltheti le a felhasználó, kivéve ha admin
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == userEmail);
            if (invoice.CustomerEmail != userEmail && !user.IsAdmin)
                return Forbid("Nincs jogosultságod ehhez a számlához");
                
            // Ellenőrizzük, hogy létezik-e a PDF fájl
            if (string.IsNullOrEmpty(invoice.PdfPath) || !System.IO.File.Exists(invoice.PdfPath))
            {
                // Ha nincs PDF, generáljunk egyet
                invoice.PdfPath = await _invoiceService.GeneratePdfInvoiceAsync(invoice);
                await _context.SaveChangesAsync();
            }
            
            // Fájl visszaadása
            var fileBytes = System.IO.File.ReadAllBytes(invoice.PdfPath);
            var fileName = Path.GetFileName(invoice.PdfPath);
            
            return File(fileBytes, "application/pdf", fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Számla újraküldése emailben (csak admin)
    [HttpPost("{id}/resend")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> ResendInvoice(int id)
    {
        try
        {
            var invoice = await _context.Invoices.FindAsync(id);
            
            if (invoice == null)
                return NotFound("A számla nem található");
                
            // Email újraküldése
            bool emailSent = await _invoiceService.SendInvoiceByEmailAsync(invoice);
            
            if (emailSent)
                return Ok("A számla sikeresen újraküldve");
            else
                return StatusCode(500, "Hiba történt a számla küldése közben");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
    
    // Számla státuszának módosítása (csak admin)
    [HttpPut("{id}/status")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<IActionResult> UpdateInvoiceStatus(int id, [FromBody] UpdateInvoiceStatusRequest request)
    {
        try
        {
            if (request == null || !Enum.IsDefined(typeof(InvoiceStatus), request.Status))
                return BadRequest("Érvénytelen státusz");
                
            var invoice = await _context.Invoices.FindAsync(id);
            
            if (invoice == null)
                return NotFound("A számla nem található");
                
            // Státusz módosítása
            invoice.Status = request.Status;
            
            await _context.SaveChangesAsync();
            
            return Ok("A számla státusza sikeresen módosítva");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Belső szerverhiba: {ex.Message}");
        }
    }
}

// Request osztályok
public class UpdateInvoiceStatusRequest
{
    public InvoiceStatus Status { get; set; }
} 