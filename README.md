# Contract Monthly Claim System (CMCS) - PROG6212 POE

## Project Overview
The Contract Monthly Claim System (CMCS) is a web-based application designed to streamline the submission and approval process of monthly claims for Independent Contractor Lecturers.

## Features Implemented

### Part 1 & 2: Core Functionality
- **User Authentication:** Secure Login and Registration for Lecturers, Programme Coordinators, and Academic Managers.
- **Claim Submission:** Lecturers can submit claims with hours worked and hourly rates.
- **File Upload:** Support for attaching supporting documents (e.g., invoices) to claims.
- **Approval Workflow:**
  - **Coordinator:** Verifies and "Pre-Approves" or "Rejects" claims.
  - **Academic Manager:** Gives final "Approval".
- **Tracking:** Lecturers can track the status of their claims in real-time.

### Part 3: Automation & Enhancements
- **Automated Verification:**
  - Claims with `HoursWorked > 100` are **automatically rejected** by the system.
  - Claims with `HourlyRate < 50` are **automatically rejected**.
- **HR Dashboard:** A dedicated view for HR to generate invoices for approved claims.
- **Profile Management:** Users can update their personal details and password.
- **Unit Testing:** xUnit tests implemented to verify calculation logic and automation rules.

## Technologies Used
- **Framework:** ASP.NET Core MVC (.NET 8.0)
- **Database:** SQL Server with Entity Framework Core
- **Frontend:** Bootstrap 5, HTML, CSS, jQuery
- **Testing:** xUnit

## How to Run
1. Open the solution in **Visual Studio 2022**.
2. Update the connection string in `appsettings.json` if necessary.
3. Run the command `Update-Database` in the Package Manager Console.
4. Build and Run the application.

## Author
Student Name: [Your Name Here]
Student Number: [Your Student Number Here]
