# Nest-Finder 🏠✨

Nest-Finder is a modern, premium **WPF (.NET 10)** desktop application designed for seamless real estate management. Built with a gorgeous user interface and robust architecture, it serves as a complete solution for property agents (Admins) and clients (Customers) to browse, manage, buy, rent, and review properties.

---

## 🌟 Key Features

### 🔐 Multi-Role Authentication
- **Admin Portal:** Manage properties (Create, Read, Update, Delete), view comprehensive analytics, and monitor transactions.
- **Customer Portal:** Browse and search properties, add items to favorites, schedule viewings, make transactions, and leave ratings/reviews.

### 📊 Interactive Dashboard
- Real-time statistics (total properties, available properties, sales/rentals).
- Quick feed of recent transactions and upcoming property viewings.

### 💼 Portfolio Management
- Property search with fuzzy text matching.
- Filter by type (Residential/Commercial) and status (Available/Sold/Rented).
- Multi-image support for each property.

### 💳 Smooth Transactions & Reviews
- Buy or rent properties directly.
- Mark transactions as Paid with different payment methods.
- Dynamic review and rating system (1-5 stars).

---

## 🛠️ Tech Stack & Design System

- **Framework:** .NET 10.0 (WPF - Windows Presentation Foundation)
- **UI Engine:** 
  - **MahApps.Metro** (Modern window chrome and components)
  - **Material Design in XAML** (High-quality modern inputs, cards, and styling)
  - **FontAwesome.Sharp** (Sleek vector icons)
- **Database:** **SQLite** (Self-contained, cross-platform, zero setup required!)
- **Architecture:** Repository Pattern with ADO.NET (`Microsoft.Data.Sqlite`) for high performance and lightweight execution.

---

## 🚀 Getting Started

Nest-Finder uses a self-initializing SQLite database. This means **you do not need to install SQL Server or run SQL scripts manually.** The application automatically creates, configures, and seeds the database file (`NestFinder.db`) on its first launch!

### Prerequisites
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) installed on a Windows operating system.

### Running the Application

1. **Clone the Repository:**
   ```bash
   git clone https://github.com/your-username/Nest-Finder.git
   cd Nest-Finder/Nest-Finder
   ```

2. **Restore Dependencies:**
   ```bash
   dotnet restore
   ```

3. **Build the Application:**
   ```bash
   dotnet build
   ```

4. **Run the Application:**
   ```bash
   dotnet run
   ```

---

## 🔑 Demo Credentials

Use the following seeded accounts to explore the application:

| Role | Username | Password | Access Details |
| :--- | :--- | :--- | :--- |
| **System Admin** | `admin` | `admin123` | Full dashboard access, property management, transacting rights |
| **Customer** | `customer` | `password123` | Viewings scheduler, shopping list, reviews, favorites |

---

## 📂 Project Structure

```
Nest-Finder/
├── Nest-Finder/
│   ├── Assets/              # Project images and backgrounds
│   ├── Helpers/             # Session managers, themes, and configuration helpers
│   ├── Models/              # OOP Entity Models (User, Property, Transaction)
│   ├── Services/            # SQLite Repositories (Data Access Layer)
│   ├── Styles/              # WPF application brushes and themes
│   ├── Views/               # XAML Views and Windows (User Interface)
│   ├── App.xaml             # Application Entry Point configurations
│   ├── NestFinder.csproj    # MSBuild project file
│   └── NestFinder.sql       # SQLite schema & database seed script
├── .gitignore               # Standard gitignore (ignores bin/, obj/, local DBs)
└── README.md                # Project documentation
```

---

## 📸 Screenshots & UI Showcase

*Add your application screenshots here to wow recruiters on GitHub!*

---

## 📄 License
This project is licensed under the MIT License.
