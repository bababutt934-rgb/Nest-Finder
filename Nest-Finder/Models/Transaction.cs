namespace NestFinder.Models;

public class Transaction
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int CustomerId { get; set; }
    public string TransactionType { get; set; } = "Buy";
    public decimal Amount { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public string PaymentMethod { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.Now;
    public string Status { get; set; } = "Pending";

    // Display joins
    public string PropertyTitle { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public int TransactionID => Id;
    public DateTime TransactionDate => Date;

    // Format helpers
    public string AmountFormatted => $"${Amount:N0}";
    public string DateFormatted => TransactionDate.ToString("MMM dd, yyyy");
}

public class Favorite
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int PropertyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Display joins
    public string PropertyTitle { get; set; } = string.Empty;
    public string PropertyCity { get; set; } = string.Empty;
    public decimal PropertyPrice { get; set; }
    public string PropertyStatus { get; set; } = string.Empty;
}

public class Review
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int UserId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Display join
    public string ReviewerName { get; set; } = string.Empty;
}

public class PropertyImage
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string ImagePath { get; set; } = string.Empty;
}

public class Viewing
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public int CustomerId { get; set; }
    public DateTime ViewingTime { get; set; }
    public string Status { get; set; } = "Scheduled";

    // Display joins
    public string PropertyTitle { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
}

public class DashboardData
{
    public int TotalProperties { get; set; }
    public int Available { get; set; }
    public int SoldRented { get; set; }
    public List<Transaction> RecentTransactions { get; set; } = new();
    public List<Viewing> UpcomingViewings { get; set; } = new();
}
