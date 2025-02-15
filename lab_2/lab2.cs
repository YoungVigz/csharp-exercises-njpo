using System;
using System.Collections.Generic;
using System.Linq;

// Zadanie 1: Klasa OnpExpression
abstract class OnpExpression
{
    public abstract override string ToString();
}

class Operand : OnpExpression
{
    public string Value { get; }
    public Operand(string value) => Value = value;
    public override string ToString() => Value;
}

class Operator : OnpExpression
{
    public OnpExpression Left { get; }
    public OnpExpression Right { get; }
    public string Symbol { get; }

    public Operator(OnpExpression left, OnpExpression right, string symbol)
    {
        Left = left;
        Right = right;
        Symbol = symbol;
    }

    public override string ToString() => $"{Left} {Right} {Symbol}";
}

// Zadanie 2: Operator < i > dla list
class ComparableList
{
    public List<int> Elements { get; }
    public ComparableList(List<int> elements) => Elements = elements;

    public static bool operator <(ComparableList a, ComparableList b) => a.Elements.Sum() < b.Elements.Sum();
    public static bool operator >(ComparableList a, ComparableList b) => a.Elements.Sum() > b.Elements.Sum();
}

// Zadanie 3: LINQ dla Student i Degree
class Student
{
    public int Index { get; set; }
    public int Age { get; set; }
    public string Gender { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
}

class Degree
{
    public int Index { get; set; }
    public string Subject { get; set; }
    public double Grade { get; set; }
    public int Year { get; set; }
    public int Semester { get; set; }
}

class Program
{
    static void Main()
    {
        // Test Zadania 1
        OnpExpression a = new Operand("a");
        OnpExpression b = new Operand("b");
        OnpExpression c = new Operand("c");
        OnpExpression expr = new Operator(new Operator(a, b, "-"), c, "*");
        Console.WriteLine(expr.ToString()); // "a b - c *"
        
        // Test Zadania 2
        ComparableList listA = new ComparableList(new List<int> { 1, 2, 3, 4 });
        ComparableList listB = new ComparableList(new List<int> { 20, 30 });
        Console.WriteLine(listA < listB); // True
        Console.WriteLine(listA > listB); // False

        // Test Zadania 3
        List<Student> students = new List<Student>
        {
            new Student { Index = 1, Age = 20, Gender = "M", Year = 1, Semester = 2 },
            new Student { Index = 2, Age = 22, Gender = "F", Year = 1, Semester = 2 },
            new Student { Index = 3, Age = 21, Gender = "M", Year = 2, Semester = 4 }
        };

        List<Degree> degrees = new List<Degree>
        {
            new Degree { Index = 1, Subject = "Math", Grade = 4.5, Year = 1, Semester = 2 },
            new Degree { Index = 2, Subject = "Math", Grade = 3.0, Year = 1, Semester = 2 },
            new Degree { Index = 3, Subject = "Physics", Grade = 4.0, Year = 2, Semester = 4 }
        };

        var studentDegrees = from s in students
                             join d in degrees on s.Index equals d.Index
                             select new { s.Index, s.Age, s.Year, s.Semester, d.Subject, d.Grade };

        Console.WriteLine("Połączone dane Student + Degree:");
        foreach (var entry in studentDegrees)
            Console.WriteLine($"{entry.Index} - {entry.Subject}: {entry.Grade}");

        var avgAgesByYear = students.GroupBy(s => s.Year)
                                    .Select(g => new { Year = g.Key, AvgAge = g.Average(s => s.Age) })
                                    .ToList();

        Console.WriteLine("Studenci starsi niż średnia na roku:");
        foreach (var student in students.Where(s => avgAgesByYear.Any(a => a.Year == s.Year && s.Age > a.AvgAge)))
            Console.WriteLine($"Index: {student.Index}, Age: {student.Age}, Year: {student.Year}");

        var avgGradesByYear = degrees.GroupBy(d => d.Year)
                                     .Select(g => new { Year = g.Key, AvgGrade = g.Average(d => d.Grade) })
                                     .ToList();

        Console.WriteLine("Studenci z wyższą średnią ocen:");
        foreach (var student in students)
        {
            var studentAvg = degrees.Where(d => d.Index == student.Index).Average(d => d.Grade);
            if (avgGradesByYear.Any(a => a.Year == student.Year && studentAvg > a.AvgGrade))
                Console.WriteLine($"Index: {student.Index}, Avg Grade: {studentAvg}, Year: {student.Year}");
        }
    }
}
