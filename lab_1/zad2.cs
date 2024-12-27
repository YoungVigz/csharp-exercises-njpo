using System;
using System.Collections.Generic;

class Employee
{
    public int ID { get; set; }
    private string Name { get; set; }
    private string Position { get; set; }
    private int Age { get; set; }
    private double Salary { get; set; }

    public Employee(int id, string name, string position, int age, double salary)
    {
        ID = id;
        Name = name;
        Position = position;
        Age = age;
        Salary = salary;
    }

    public bool Validate()
    {
        return ID > 0 && !string.IsNullOrWhiteSpace(Name) && !string.IsNullOrWhiteSpace(Position) && Age > 18 && Salary > 0;
    }

    public void Show()
    {
        Console.WriteLine($"ID: {ID}, Name: {Name}, Position: {Position}, Age: {Age}, Salary: {Salary:F2}");
    }

    public bool IsMatch(string keyword)
    {
        return Name.Contains(keyword, StringComparison.OrdinalIgnoreCase) || Position.Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }
}

class EmployeeCard<T> where T : Employee
{
    private List<T> employees = new List<T>();

    public void Add(T employee)
    {
        if (employee.Validate())
        {
            employees.Add(employee);
            Console.WriteLine("Pracownik został dodany");
            return;
        }
   
        Console.WriteLine("Pracownik posiada nieprawdiłowe dane");
    }

    public void Remove(int id)
    {
        var employee = employees.Find(e => e.ID == id);

        if (employee != null)
        {
            employees.Remove(employee);
            Console.WriteLine($"Pracownik o ID: {employee.ID} został usunięty");
            return;
        }
       
        Console.WriteLine($"Pracownik o ID: {id} nie został znaleziony ");
        
    }

    public void DisplayAll()
    {
        foreach (var employee in employees)
        {
            employee.Show();
        }
    }

    public List<T> Search(string keyword)
    {
        return employees.FindAll(e => e.IsMatch(keyword));
    }
}

class Program
{
    static void Main(string[] args)
    {
        var record = new EmployeeCard<Employee>();

        var emp1 = new Employee(1, "John Doe", "Developer", 30, 60000);
        var emp2 = new Employee(2, "Jane Smith", "Manager", 45, 80000);
        var emp3 = new Employee(3, "Joe Rogan", "Manager", 37, 70000);

        record.Add(emp1);
        record.Add(emp2);
        record.Add(emp3);

        Console.WriteLine("\nWszyscy pracownicy: ");
        record.DisplayAll();

        Console.WriteLine("\nWyszukiwanie pracownika na pozycji: \"Manager\"");
        var searchResults = record.Search("Manager");
        foreach (var emp in searchResults)
        {
            emp.Show();
        }

        Console.WriteLine("\nUsuwanie pracownika o ID: 1");
        record.Remove(1);

        Console.WriteLine("\nUsuwanie pracownika o ID: 35");
        record.Remove(35);

        Console.WriteLine("\nWszyscy pracownicy: ");
        record.DisplayAll();
    }
}