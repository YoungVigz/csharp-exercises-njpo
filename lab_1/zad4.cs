using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Text.Json;

interface IOData
{
    void Save<T>(List<T> data, string filePath);
    List<T> Load<T>(string filePath);
}

class JsonIO : IOData
{
    private int shift = int.Parse(ConfigurationManager.AppSettings["Shift"]);

    public void Save<T>(List<T> data, string filePath)
    {
        string jsonData = JsonSerializer.Serialize(data);
        string encryptedData = CaesarCipher.Encrypt(jsonData, shift);
        File.WriteAllText(filePath, encryptedData);
        Console.WriteLine("Dane zapisane w formacie JSON zaszyfrowane");
    }

    public List<T> Load<T>(string filePath)
    {
        string encryptedData = File.ReadAllText(filePath);
        string decryptedData = CaesarCipher.Decrypt(encryptedData, shift);
        return JsonSerializer.Deserialize<List<T>>(decryptedData);
    }
}

static class CaesarCipher
{
    public static string Encrypt(string text, int shift)
    {
        char[] buffer = text.ToCharArray();
        for (int i = 0; i < buffer.Length; i++)
        {
            char letter = buffer[i];
            if (char.IsLetter(letter))
            {
                char offset = char.IsUpper(letter) ? 'A' : 'a';
                letter = (char)((letter + shift - offset) % 26 + offset);
            }
            buffer[i] = letter;
        }
        return new string(buffer);
    }

    public static string Decrypt(string text, int shift)
    {
        return Encrypt(text, 26 - shift);
    }
}


class Employee
{
    public int ID { get; set; }
    public string Name { get; set; }
    public string Position { get; set; }
    public int Age { get; set; }
    public double Salary { get; set; }

    public Employee() { }

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

    public override string ToString()
    {
        return $"ID: {ID}, Name: {Name}, Position: {Position}, Age: {Age}, Salary: {Salary:F2}";
    }
}

class EmployeeCard<T> where T : Employee
{
    private List<T> employees = new List<T>();
    private IOData dataIO;

    public EmployeeCard(IOData saver)
    {
        dataIO = saver;
    }

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


    public void Save(string filePath)
    {
        dataIO.Save(employees, filePath);
    }

    public void Load(string filePath)
    {
        employees = dataIO.Load<T>(filePath);
    }

}

class Program
{
    static void Main(string[] args)
    {

        ConfigurationManager.AppSettings["Shift"] = "3";
        var record = new EmployeeCard<Employee>(new JsonIO());

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

        Console.WriteLine("\nSaving employees to JSON file...");
        record.Save("employees.json");

        Console.WriteLine("\nLoading employees from JSON file...");
        record.Load("employees.json");

        Console.WriteLine("\nWszyscy pracownicy: ");
        record.DisplayAll();
    }
}