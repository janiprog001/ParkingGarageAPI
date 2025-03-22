using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ParkingGarageAPI.Entities
{
    public class Invoice
    {
        public int Id { get; set; }
        
        [Required]
        public string InvoiceNumber { get; set; }
        
        [Required]
        public DateTime IssueDate { get; set; }
        
        [Required]
        public DateTime DueDate { get; set; }
        
        [Required]
        public decimal Amount { get; set; }
        
        [Required]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Created;
        
        // Számlázási adatok
        [Required]
        public string CustomerName { get; set; }
        
        [Required]
        public string CustomerEmail { get; set; }
        
        public string CustomerAddress { get; set; }
        
        // Kapcsolat a ParkingHistory-val
        public int ParkingHistoryId { get; set; }
        
        [JsonIgnore]
        public virtual ParkingHistory? ParkingHistory { get; set; }
        
        // Kapcsolat a felhasználóval
        public int UserId { get; set; }
        
        [JsonIgnore]
        public virtual User? User { get; set; }
        
        // Email küldési információk
        public bool EmailSent { get; set; } = false;
        
        public DateTime? EmailSentDate { get; set; }
        
        // PDF hivatkozás
        public string? PdfPath { get; set; }
        
        // Számla részletek
        public string Description { get; set; }
        
        // Számla azonosító generálása
        public static string GenerateInvoiceNumber()
        {
            string prefix = "INV";
            string dateCode = DateTime.Now.ToString("yyyyMMdd");
            string uniqueCode = Guid.NewGuid().ToString().Substring(0, 8);
            
            return $"{prefix}-{dateCode}-{uniqueCode}";
        }
    }
    
    public enum InvoiceStatus
    {
        Created,
        Sent,
        Paid,
        Cancelled
    }
} 