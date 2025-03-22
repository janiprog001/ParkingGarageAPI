using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.EntityFrameworkCore;
using ParkingGarageAPI.Context;
using ParkingGarageAPI.Entities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ParkingGarageAPI.Services
{
    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceAsync(ParkingHistory parkingHistory);
        Task<string> GeneratePdfInvoiceAsync(Invoice invoice);
        Task<bool> SendInvoiceByEmailAsync(Invoice invoice);
        Task<Invoice> GetInvoiceByIdAsync(int id);
    }
    
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly string _invoicesDirectory;
        
        public InvoiceService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
            
            // Könyvtár létrehozása a számláknak
            _invoicesDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Invoices");
            if (!Directory.Exists(_invoicesDirectory))
            {
                Directory.CreateDirectory(_invoicesDirectory);
            }
        }
        
        public async Task<Invoice> CreateInvoiceAsync(ParkingHistory parkingHistory)
        {
            try
            {
                // Felhasználó keresése
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == parkingHistory.UserId);
                if (user == null)
                {
                    throw new Exception($"Nem található felhasználó a következő azonosítóval: {parkingHistory.UserId}");
                }
                
                // Számla létrehozása
                var invoice = new Invoice
                {
                    InvoiceNumber = Invoice.GenerateInvoiceNumber(),
                    IssueDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(15), // 15 napos fizetési határidő
                    Amount = parkingHistory.Fee,
                    Status = InvoiceStatus.Created,
                    CustomerName = $"{user.FirstName} {user.LastName}",
                    CustomerEmail = user.Email,
                    CustomerAddress = "N/A", // Opcionálisan tárolható lenne a felhasználó címe is
                    ParkingHistoryId = parkingHistory.Id,
                    UserId = user.Id,
                    Description = $"Parkolási díj - {parkingHistory.StartTime.ToString("yyyy.MM.dd HH:mm")} - {parkingHistory.EndTime.ToString("yyyy.MM.dd HH:mm")}",
                    PdfPath = string.Empty // Inicializáljuk üres stringgel
                };
                
                // Mentés az adatbázisba
                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();
                
                // PDF generálása
                string pdfPath = await GeneratePdfInvoiceAsync(invoice);
                
                // PDF elérési út mentése
                invoice.PdfPath = pdfPath;
                await _context.SaveChangesAsync();
                
                return invoice;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a számla létrehozása közben: {ex.Message}");
                throw;
            }
        }
        
        public async Task<string> GeneratePdfInvoiceAsync(Invoice invoice)
        {
            try
            {
                // Parkolási történet lekérése
                var parkingHistory = await _context.ParkingHistories
                    .FirstOrDefaultAsync(ph => ph.Id == invoice.ParkingHistoryId);
                
                if (parkingHistory == null)
                {
                    throw new Exception($"Nem található parkolási előzmény a következő azonosítóval: {invoice.ParkingHistoryId}");
                }
                
                // PDF fájl nevének generálása
                string pdfFileName = $"Invoice_{invoice.InvoiceNumber.Replace("-", "_")}.pdf";
                string pdfPath = Path.Combine(_invoicesDirectory, pdfFileName);
                
                // PDF generálása
                using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);
                    
                    // Magyar karakterek támogatása
                    BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1250, BaseFont.EMBEDDED);
                    
                    // Betűtípusok definiálása
                    Font normalFont = new Font(baseFont, 10, Font.NORMAL);
                    Font boldFont = new Font(baseFont, 12, Font.BOLD);
                    Font headerFont = new Font(baseFont, 18, Font.BOLD);
                    Font italicFont = new Font(baseFont, 10, Font.ITALIC);
                    
                    document.Open();
                    
                    // Fejléc
                    Paragraph header = new Paragraph("Parkolóház Számla", headerFont);
                    header.Alignment = Element.ALIGN_CENTER;
                    document.Add(header);
                    
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));
                    
                    // Számlázási adatok
                    document.Add(new Paragraph($"Számla sorszáma: {invoice.InvoiceNumber}", normalFont));
                    document.Add(new Paragraph($"Kiállítás dátuma: {invoice.IssueDate.ToString("yyyy.MM.dd")}", normalFont));
                    document.Add(new Paragraph($"Fizetési határidő: {invoice.DueDate.ToString("yyyy.MM.dd")}", normalFont));
                    
                    document.Add(new Paragraph(" "));
                    
                    // Vevő adatai
                    document.Add(new Paragraph("Vevő adatai:", boldFont));
                    document.Add(new Paragraph($"Név: {invoice.CustomerName}", normalFont));
                    document.Add(new Paragraph($"Email: {invoice.CustomerEmail}", normalFont));
                    if (!string.IsNullOrEmpty(invoice.CustomerAddress))
                    {
                        document.Add(new Paragraph($"Cím: {invoice.CustomerAddress}", normalFont));
                    }
                    
                    document.Add(new Paragraph(" "));
                    
                    // Parkolási adatok
                    document.Add(new Paragraph("Parkolási adatok:", boldFont));
                    document.Add(new Paragraph($"Parkolás kezdete: {parkingHistory.StartTime.ToString("yyyy.MM.dd HH:mm")}", normalFont));
                    document.Add(new Paragraph($"Parkolás vége: {parkingHistory.EndTime.ToString("yyyy.MM.dd HH:mm")}", normalFont));
                    document.Add(new Paragraph($"Időtartam: {parkingHistory.DurationFormatted}", normalFont));
                    document.Add(new Paragraph($"Parkolóhely: {parkingHistory.FloorNumber}. emelet, {parkingHistory.SpotNumber}. hely", normalFont));
                    document.Add(new Paragraph($"Rendszám: {parkingHistory.LicensePlate}", normalFont));
                    
                    document.Add(new Paragraph(" "));
                    
                    // Fizetési adatok
                    document.Add(new Paragraph("Fizetési adatok:", boldFont));
                    document.Add(new Paragraph($"Fizetendő összeg: {invoice.Amount} Ft", normalFont));
                    document.Add(new Paragraph($"Fizetési mód: Átutalás", normalFont));
                    document.Add(new Paragraph($"Státusz: {GetInvoiceStatusText(invoice.Status)}", normalFont));
                    
                    document.Add(new Paragraph(" "));
                    document.Add(new Paragraph(" "));
                    
                    // Lábléc
                    Paragraph footer = new Paragraph("Köszönjük, hogy igénybe vette parkolóházunkat!", italicFont);
                    footer.Alignment = Element.ALIGN_CENTER;
                    document.Add(footer);
                    
                    document.Close();
                }
                
                return pdfPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a PDF számla generálása közben: {ex.Message}");
                throw;
            }
        }
        
        public async Task<bool> SendInvoiceByEmailAsync(Invoice invoice)
        {
            try
            {
                if (string.IsNullOrEmpty(invoice.PdfPath) || !File.Exists(invoice.PdfPath))
                {
                    // Ha nincs még PDF generálva, generáljunk egyet
                    invoice.PdfPath = await GeneratePdfInvoiceAsync(invoice);
                    await _context.SaveChangesAsync();
                }
                
                // Email tartalom összeállítása
                string subject = $"Parkolási számla - {invoice.InvoiceNumber}";
                string body = $@"
                    <h2>Tisztelt {invoice.CustomerName}!</h2>
                    <p>Köszönjük, hogy igénybe vette parkolóházunkat. Mellékelten küldjük a parkolási díjról szóló számlát.</p>
                    <p><strong>Számla sorszáma:</strong> {invoice.InvoiceNumber}</p>
                    <p><strong>Összeg:</strong> {invoice.Amount} Ft</p>
                    <p><strong>Fizetési határidő:</strong> {invoice.DueDate.ToString("yyyy.MM.dd")}</p>
                    <p>A számla részleteit a csatolt PDF dokumentumban találja.</p>
                    <p>Üdvözlettel,<br/>Parkolóház Szolgáltatás</p>
                ";
                
                // Email küldése
                bool emailSent = await _emailService.SendInvoiceEmailAsync(
                    invoice.CustomerEmail,
                    subject,
                    body,
                    invoice.PdfPath
                );
                
                // Ha sikeres volt a küldés, frissítsük a számla státuszát
                if (emailSent)
                {
                    invoice.EmailSent = true;
                    invoice.EmailSentDate = DateTime.Now;
                    invoice.Status = InvoiceStatus.Sent;
                    
                    await _context.SaveChangesAsync();
                }
                
                return emailSent;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba történt a számla email küldése közben: {ex.Message}");
                return false;
            }
        }
        
        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices.FindAsync(id);
        }
        
        private string GetInvoiceStatusText(InvoiceStatus status)
        {
            switch (status)
            {
                case InvoiceStatus.Created:
                    return "Kiállítva";
                case InvoiceStatus.Sent:
                    return "Elküldve";
                case InvoiceStatus.Paid:
                    return "Kifizetve";
                case InvoiceStatus.Cancelled:
                    return "Sztornózva";
                default:
                    return "Ismeretlen";
            }
        }
    }
} 