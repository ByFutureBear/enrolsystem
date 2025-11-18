using OnlineEnrolmentSystem.Models;
using OnlineEnrolmentSystem.Security;

namespace OnlineEnrolmentSystem.Data
{
    public static class DbInitializer
    {
        public static void Initialize(StudentDBContext context)
        {
            context.Database.EnsureCreated();

            // --- Students ---
            if (!context.Students.Any())
            {
                var password = PasswordHasher.Hash("abc123");
                var students = new Student[]
                {
                    new Student { Name="TEO Xhi Tang", Email="teo@group1.com", PhoneNumber="0123456789", Address="Kuala Lumpur", PasswordHash=password, CreatedAt=DateTime.Now },
                    new Student { Name="LARISHA KAUR A/P JANGINDER SINGH", Email="larisha@group1.com", PhoneNumber="0198765432", Address="Penang", PasswordHash=password, CreatedAt=DateTime.Now },
                    new Student { Name="KUA Ming Chow", Email="kua@group1.com", PhoneNumber="0112233445", Address="Johor Bahru", PasswordHash=password, CreatedAt=DateTime.Now },
                    new Student { Name="HONG Wei Sheng", Email="hong@group1.com", PhoneNumber="0166778899", Address="Melaka", PasswordHash=password, CreatedAt=DateTime.Now },
                    new Student { Name="CHI Choon Keat", Email="chi@group1.com", PhoneNumber="0175556666", Address="Ipoh", PasswordHash=password, CreatedAt=DateTime.Now },
                };
                context.Students.AddRange(students);
                context.SaveChanges();
            }

            // --- BankDetails ---
            if (!context.BankDetails.Any())
            {
                var bankDetails = new BankDetail[]
                {
                    new BankDetail { BankName="Maybank", BankAccountNumber="1234567890", BankHolderName="TEO XHI TANG", StudentId=1 },
                    new BankDetail { BankName="CIMB", BankAccountNumber="9876543210", BankHolderName="LARISHA KAUR", StudentId=2 },
                    new BankDetail { BankName="Public Bank", BankAccountNumber="555666777", BankHolderName="KUA MING CHOW", StudentId=3 },
                };
                context.BankDetails.AddRange(bankDetails);
                context.SaveChanges();
            }

            // --- Courses ---
            if (!context.Courses.Any())
            {
                var courses = new Course[]
                {
                    new Course { CourseCode="PRG3204E", CourseName="WEB APPLICATION DEVELOPMENT", Credits=3 },
                    new Course { CourseCode="ITM3206E", CourseName="SOFTWARE ENGINEERING", Credits=3, PrerequisiteCourseId=1 },
                    new Course { CourseCode="MMD2205E", CourseName="GRAPHIC ANIMATION", Credits=4 },
                    new Course { CourseCode="IBM2202E", CourseName="IT INFRASTRUCTURE LANDSCAPE", Credits=3 },
                    new Course { CourseCode="CIS3201E", CourseName="COMPUTER COMMUNICATION & NETWORKS", Credits=6, PrerequisiteCourseId=4 }
                };
                context.Courses.AddRange(courses);
                context.SaveChanges();
            }

            // --- Classes ---
            if (!context.Classes.Any())
            {
                var classes = new Class[]
                {
                    new Class { CourseId=1, Section="A", Capacity=30, Schedule="Fri 20:30-21:30" },
                    new Class { CourseId=1, Section="B", Capacity=25, Schedule="Wed 21:30-22:30" },
                    new Class { CourseId=2, Section="A", Capacity=20, Schedule="Tue 20:30-21:30" },
                    new Class { CourseId=2, Section="B", Capacity=20, Schedule="Thu 19:30-20:30" },
                    new Class { CourseId=3, Section="A", Capacity=0, Schedule="Mon 20:30-21:30" },
                    new Class { CourseId=4, Section="A", Capacity=30, Schedule="Fri 20:30-21:30" },
                    new Class { CourseId=5, Section="A", Capacity=15, Schedule="Wed 19:30-20:30" }
                };
                context.Classes.AddRange(classes);
                context.SaveChanges();
            }

            // --- Enrolments ---
            if (!context.Enrolments.Any())
            {
                var enrolments = new Enrolment[]
                {
                    new Enrolment { StudentId=1, ClassId=1, EnrolmentDate=DateTime.Now.AddDays(-20), Status="Enroled" },
                    new Enrolment { StudentId=1, ClassId=3, EnrolmentDate=DateTime.Now.AddDays(-15), Status="Completed" },
                    new Enrolment { StudentId=2, ClassId=4, EnrolmentDate=DateTime.Now.AddDays(-5), Status="Enroled" },
                    new Enrolment { StudentId=3, ClassId=5, EnrolmentDate=DateTime.Now.AddDays(-30), Status="Completed" },
                    new Enrolment { StudentId=4, ClassId=6, EnrolmentDate=DateTime.Now.AddDays(-2), Status="Enroled" }
                };
                context.Enrolments.AddRange(enrolments);
                context.SaveChanges();
            }

            // --- Payments ---
            if (!context.Payments.Any())
            {
                var payments = new Payment[]
                {
                    new Payment { StudentId=1, Amount=1500, PaymentDate=DateTime.Now.AddDays(-10), PaymentMethod="Ewallet", ReceiptNumber="RCPT-7hsk2mf5", Status="Paid", Description="INV-2025-001", EwalletProvider="TNG" }
                };
                context.Payments.AddRange(payments);
                context.SaveChanges();
            }

            // --- Invoice ---
            if (!context.Invoices.Any())
            {
                var invoices = new Invoice[]
                {
                    new Invoice{StudentId = 1, IssueDate = DateTime.Now.AddDays(-30), DueDate = DateTime.Now.AddDays(-10), TotalAmount = 1500m, Status = "Paid", InvoiceNumber = "INV-2025-001", Description = "Semester 1 Tuition Fee", Adjustment = 0m, AdjustmentNotes = string.Empty},
                    new Invoice{StudentId = 1, IssueDate = DateTime.Now.AddDays(-10), DueDate = DateTime.Now.AddDays(25), TotalAmount = 1500m, Status = "Pending", InvoiceNumber = "INV-2025-005", Description = "Semester 2 Tuition Fee", Adjustment = 100m, AdjustmentNotes = "Scholarship discount"},
                    new Invoice{StudentId = 2, IssueDate = DateTime.Now.AddDays(-20), DueDate = DateTime.Now.AddDays(10), TotalAmount = 1200m, Status = "Pending", InvoiceNumber = "INV-2025-002", Description = "Semester 1 Tuition Fee", Adjustment = 0m, AdjustmentNotes = string.Empty},
                    new Invoice{StudentId = 3, IssueDate = DateTime.Now.AddDays(-15), DueDate = DateTime.Now.AddDays(15), TotalAmount = 1800m, Status = "Overdue", InvoiceNumber = "INV-2025-003", Description = "Semester 1 Tuition Fee + Lab Fees", Adjustment = 100m, AdjustmentNotes = "Scholarship discount"},
                    new Invoice{StudentId = 4, IssueDate = DateTime.Now.AddDays(-5), DueDate = DateTime.Now.AddDays(25), TotalAmount = 2000m, Status = "Pending", InvoiceNumber = "INV-2025-004", Description = "Semester 1 Tuition Fee", Adjustment = 0m, AdjustmentNotes = string.Empty}
                };
                context.Invoices.AddRange(invoices);
                context.SaveChanges();
            }

            // --- Evaluations ---
            if (!context.Evaluations.Any())
            {
                var evaluations = new Evaluation[]
                {
                    new Evaluation { StudentId=1, CourseId=2, Rating=5, Comments="Great class!", CreatedAt=DateTime.Now },
                    new Evaluation { StudentId=2, CourseId=2, Rating=4, Comments="Very informative", CreatedAt=DateTime.Now },
                    new Evaluation { StudentId=3, CourseId=3, Rating=3, Comments="Could be better", CreatedAt=DateTime.Now }
                };
                context.Evaluations.AddRange(evaluations);
                context.SaveChanges();
            }

            // --- AddDropHistory ---
            if (!context.AddDropHistories.Any())
            {
                var history = new AddDropHistory[]
                {
                    new AddDropHistory { StudentId=1, ClassId=1, Action="Add", ActionDate=DateTime.Now.AddDays(-20) },
                    new AddDropHistory { StudentId=2, ClassId=2, Action="Add", ActionDate=DateTime.Now.AddDays(-10) },
                    new AddDropHistory { StudentId=2, ClassId=2, Action="Drop", ActionDate=DateTime.Now.AddDays(-8) },
                    new AddDropHistory { StudentId=4, ClassId=6, Action="Add", ActionDate=DateTime.Now.AddDays(-2) }
                };
                context.AddDropHistories.AddRange(history);
                context.SaveChanges();
            }
        }
    }
}
