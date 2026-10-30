using System;
using System.Collections.Generic;

namespace Syncfusion.Blazor.Tests.Grids.Actions
{
    public class EmployeeDetailsFH
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string Surname { get; set; }
        public DateTime JoinedDate { get; set; }
        public string Role { get; set; }
        public string Country { get; set; }
        public double Freight { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string City { get; set; }
        public string EmployeeCode { get; set; }
        public decimal Salary { get; set; }
        public bool IsActive { get; set; }
        public string Gender { get; set; }
        public string ManagerName { get; set; }
        public string OfficeLocation { get; set; }
        public string ProjectName { get; set; }
        public string EmploymentType { get; set; }
        public string SkillSet { get; set; }
    }

    public static class EmployeeData
    {
        public static List<EmployeeDetailsFH> GetEmployees()
        {
            // base seed records used to generate the larger set
            var seeds = new List<EmployeeDetailsFH>
            {
                new EmployeeDetailsFH { EmployeeID = 1, FirstName = "Nancy Anton", Surname = "Davolio", JoinedDate = new DateTime(2020,1,15), Role = "Manager", Country = "USA", Freight = 32.38, Email = "nancy.davolio@gmail.com", PhoneNumber = "+1-202-555-0101", Department = "Management", City = "New York", EmployeeCode = "EMP-0001", Salary = 85000, IsActive = true, Gender = "Female", ManagerName = "John Smith", OfficeLocation = "Building-A/Floor-5", ProjectName = "HRMS-V2.0", EmploymentType = "Full-Time", SkillSet = "Leadership,C#,Azure" },
                new EmployeeDetailsFH { EmployeeID = 2, FirstName = "Andrew", Surname = "Fuller", JoinedDate = new DateTime(2019,5,20), Role = "Developer", Country = "UK", Freight = 11.61, Email = "andrew.fuller@gmail.com", PhoneNumber = "+44-7700-900101", Department = "Engineering", City = "London", EmployeeCode = "EMP-0002", Salary = 65000, IsActive = true, Gender = "Male", ManagerName = "Nancy Davolio", OfficeLocation = "Building-B/Floor-3", ProjectName = "Sales-Pro", EmploymentType = "Full-Time", SkillSet = "C#,Blazor,SQL" },
                new EmployeeDetailsFH { EmployeeID = 3, FirstName = "Janet", Surname = "Leverling", JoinedDate = new DateTime(2021,7,10), Role = "Tester", Country = "Germany", Freight = 65.83, Email = "janet.leverling@gmail.com", PhoneNumber = "+49-151-1234567", Department = "QA", City = "Berlin", EmployeeCode = "EMP-0003", Salary = 55000, IsActive = true, Gender = "Female", ManagerName = "Nancy Davolio", OfficeLocation = "Building-C/Floor-2", ProjectName = "QA-Automation", EmploymentType = "Contract", SkillSet = "Selenium,Playwright,API" },
                new EmployeeDetailsFH { EmployeeID = 4, FirstName = "Margaret", Surname = "Peacock", JoinedDate = new DateTime(2018,11,05), Role = "Architect", Country = "France", Freight = 41.34, Email = "margaret.peacock@gmail.com", PhoneNumber = "+33-612-345678", Department = "Architecture", City = "Paris", EmployeeCode = "EMP-0004", Salary = 95000, IsActive = true, Gender = "Female", ManagerName = "John Smith", OfficeLocation = "Building-D/Floor-8", ProjectName = "Cloud-Modernization", EmploymentType = "Full-Time", SkillSet = "Microservices,Azure,DevOps" },
                new EmployeeDetailsFH { EmployeeID = 5, FirstName = "Steven", Surname = "Buchanan", JoinedDate = new DateTime(2022,3,18), Role = "Business Analyst", Country = "India", Freight = 51.30, Email = "steven.buchanan@gmail.com", PhoneNumber = "+91-98765-43210", Department = "Business", City = "Chennai", EmployeeCode = "EMP-0005", Salary = 60000, IsActive = true, Gender = "Male", ManagerName = "Nancy Davolio", OfficeLocation = "Building-E/Floor-4", ProjectName = "ERP-Migration", EmploymentType = "Full-Time", SkillSet = "Agile,JIRA,Analytics" }
            };

            var result = new List<EmployeeDetailsFH>(1000);
            for (int i = 1; i <= 1000; i++)
            {
                var seed = seeds[(i - 1) % seeds.Count];
                result.Add(new EmployeeDetailsFH
                {
                    EmployeeID = i,
                    FirstName = seed.FirstName,
                    Surname = seed.Surname,
                    JoinedDate = seed.JoinedDate,
                    Role = seed.Role,
                    Country = seed.Country,
                    Freight = seed.Freight,
                    Email = $"{seed.FirstName.ToLower().Replace(" ", ".")}.{seed.Surname.ToLower()}{i}@gmail.com",
                    PhoneNumber = seed.PhoneNumber,
                    Department = seed.Department,
                    City = seed.City,
                    EmployeeCode = $"EMP-{i:D4}",
                    Salary = seed.Salary,
                    IsActive = (i % 10) != 0,
                    Gender = seed.Gender,
                    ManagerName = seed.ManagerName,
                    OfficeLocation = seed.OfficeLocation,
                    ProjectName = seed.ProjectName,
                    EmploymentType = seed.EmploymentType,
                    SkillSet = seed.SkillSet
                });
            }

            return result;
        }
    }
}
