# 🛒 E-Commerce Marketplace

A comprehensive ASP.NET Core e-commerce application with advanced seller dashboard, real-time inventory management, sales analytics, and streamlined checkout system.

## 🚀 Phase 5: Advanced Seller Dashboard & Inventory Management

### 📊 Real-Time Seller Dashboard

- **Live Inventory Valuation**: Real-time calculation of total inventory worth
- **Sales Performance Metrics**: Revenue tracking, order analytics, and profit analysis
- **Interactive Charts**: Monthly sales trends and category breakdowns using Chart.js
- **Smart Alerts**: Low stock and out-of-stock notifications
- **Quick Actions**: One-click stock updates and product management

### 📦 Advanced Inventory Manager

- **Real-Time Stock Tracking**: Live inventory status with color-coded alerts
- **Smart Restocking Recommendations**: AI-powered suggestions based on sales velocity
- **Quick Stock Updates**: Modal-based stock adjustment with reason tracking
- **Product Performance Analytics**: Sales velocity, revenue per product, and inventory turnover
- **Advanced Filtering**: Search by name, category, and stock status

### 📈 Comprehensive Sales Analytics

- **Revenue Analysis**: Monthly trends, growth rates, and performance insights
- **Top Product Rankings**: Best-selling items with detailed metrics
- **Daily Sales Tracking**: 30-day sales trend visualization
- **Performance Insights**: Automated business intelligence and recommendations

### ⚡ Enhanced Checkout System

- **Quick Checkout**: One-click ordering with default shipping information
- **Dual Checkout Options**: Both quick and detailed checkout flows
- **Streamlined Authorization**: Removed role restrictions for better UX
- **Real-Time Cart Updates**: Dynamic pricing and quantity controls

## ✨ Complete Feature Set

### 🔐 Authentication & Authorization

- **Complete Role-Based System**: Customer, Seller, and Admin roles
- **Styled Registration Page**: Interactive role selection with visual cards
- **Enhanced Login Page**: Modern design with FontAwesome icons
- **Role-Based Navigation**: Dynamic navbar based on user roles
- **Automatic Role Assignment**: Users are assigned roles during registration
- **Role-Based Redirects**: Different dashboards for different user types

### 🎨 User Interface

- **Modern Bootstrap 5 Design**: Responsive and professional layout
- **FontAwesome Icons**: Beautiful icons throughout the application
- **Interactive Role Selection**: Hover effects and animations
- **Custom CSS Styling**: Enhanced visual appeal with custom styles
- **Dashboard Layouts**: Separate dashboards for Seller and Admin roles

### 🏗️ Application Structure

- **Extended User Model**: Added FirstName and LastName properties
- **Database Integration**: EF Core with SQL Server
- **Controller Organization**: Separate controllers for different roles
- **View Organization**: Well-structured Razor views with shared layouts

## 🚀 Getting Started

### Prerequisites

- .NET Core 3.1 SDK
- SQL Server or SQL Server Express
- Visual Studio or VS Code

### Installation

1. **Clone the repository**

   ```bash
   git clone <repository-url>
   cd E-commerce
   ```

2. **Update Database Connection String**
   Update `appsettings.json` with your SQL Server connection string:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerceDb;Trusted_Connection=true;MultipleActiveResultSets=true"
     }
   }
   ```

3. **Run Database Migrations**

   ```bash
   dotnet ef database update
   ```

4. **Run the Application**

   ```bash
   dotnet run
   ```

5. **Access the Application**
   - Navigate to `https://localhost:5001` or `http://localhost:5000`

## 👤 User Roles & Features

### 🛍️ Customer Role

- Browse and search products
- Add products to cart
- Place and track orders
- View order history

### 🏪 Seller Role

- Access seller dashboard
- Add and manage products
- Upload product images
- View and manage orders for their products
- Track sales and inventory

### 🔧 Admin Role

- Access admin dashboard
- Manage all users
- Manage all products
- Manage all orders
- View system statistics

## 🗄️ Database Schema

### Users (Identity)

- Extended ApplicationUser with FirstName, LastName
- Role-based authentication (Customer, Seller, Admin)

### Products

- Product information with seller relationship
- Image management system
- Category and inventory tracking

### Orders & OrderItems

- Complete order management system
- Order tracking and status updates

## 🎨 UI Features

### Registration Page

- **Interactive Role Selection**: Visual cards for Customer/Seller selection
- **Form Validation**: Client and server-side validation
- **Modern Design**: Bootstrap 5 with custom styling
- **Responsive Layout**: Works on all device sizes

### Login Page

- **Clean Design**: Professional login interface
- **Role-Based Redirects**: Automatic redirection based on user role
- **Enhanced UX**: Smooth animations and transitions

### Navigation

- **Dynamic Navbar**: Changes based on user authentication and role
- **Quick Access**: Role-specific navigation items
- **User Dropdown**: Easy access to profile and logout

### Dashboards

- **Seller Dashboard**: Quick stats and actions for sellers
- **Admin Dashboard**: System overview and management tools
- **Responsive Cards**: Beautiful card-based layouts

## 🔄 Next Steps (Coming Soon)

1. **Product Management**

   - Complete CRUD operations for products
   - Image upload functionality
   - Category management

2. **Shopping Cart**

   - Add to cart functionality
   - Cart management (update quantities, remove items)
   - Checkout process

3. **Order System**

   - Order placement and processing
   - Order status tracking
   - Email notifications

4. **Advanced Features**
   - Search and filtering
   - Product reviews and ratings
   - Payment integration
   - Inventory management

## 🛠️ Technical Stack

- **Backend**: ASP.NET Core 3.1, Entity Framework Core
- **Frontend**: Razor Pages, Bootstrap 5, FontAwesome
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity
- **Styling**: Custom CSS with Bootstrap 5

## 📁 Project Structure

```
E-commerce/
├── Controllers/          # MVC Controllers
│   ├── AccountController.cs
│   ├── HomeController.cs
│   ├── SellerController.cs
│   ├── AdminController.cs
│   └── ...
├── Models/              # Data Models
│   ├── ApplicationUser.cs
│   ├── AuthViewModels.cs
│   └── ...
├── Views/               # Razor Views
│   ├── Account/
│   ├── Home/
│   ├── Seller/
│   ├── Admin/
│   └── Shared/
├── Data/                # Database Context
├── wwwroot/             # Static Files
└── ...
```

## 🎯 Current Status

✅ **Completed:**

- Authentication system with roles
- User registration with role selection
- Modern login/registration UI
- Role-based navigation
- Basic dashboard layouts
- Database setup with migrations

🚧 **In Progress:**

- Product management system
- Shopping cart functionality
- Order processing system

📋 **Planned:**

- Payment integration
- Advanced search and filtering
- Email notifications
- Admin user management

## 🤝 Contributing

Feel free to contribute to this project by:

1. Forking the repository
2. Creating a feature branch
3. Making your changes
4. Submitting a pull request

## 📜 License

This project is licensed under the MIT License.

---

**Happy Coding! 🚀**
E-commerce website using .NET framework
