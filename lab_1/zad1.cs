using System;
using System.Drawing;

abstract class Triangle {
    protected abstract double GetArea();
    protected abstract double GetPerimeter();
    public abstract override string ToString();

}


class Equilateral : Triangle {

    private double a;

    public Equilateral(double a)
    {
        this.a = a;
    }

    protected override double GetArea()
    {
        return (a * a) * Math.Sqrt(3) / 4;
    }

    protected override double GetPerimeter()
    {
        return 3 * a;
    }

    public override string ToString()
    {
        return $"Equilateral Triangle: Side = {a}, Area = {GetArea():F2}, Perimeter = {GetPerimeter():F2}";
    }
}


class Isosceles : Triangle {

    private double a;
    private double b;

    public Isosceles(double a, double b) {
        this.a = a;
        this.b = b;
    
    }

    protected override double GetArea()
    {
        double height = Math.Sqrt(a * a - (b * b) / 4);
        return (b * height) / 2;
    }

    protected override double GetPerimeter()
    {
        return 2 * a + b;
    }

    public override string ToString()
    {
        return $"Isosceles Triangle: Side = {a}, Base = {b}, Area = {GetArea():F2}, Perimeter = {GetPerimeter():F2}";
    }
}


class Right : Triangle
{

    private double a;
    private double b;

    public Right(double a, double b)
    {
        this.a = a;
        this.b = b;
    }

    protected override double GetArea()
    {
        return (a * b) / 2;
    }

    protected override double GetPerimeter()
    {
        double hypotenuse = Math.Sqrt(a * a + b * b);
        return a + b + hypotenuse;
    }
    public override string ToString()
    {
        return $"Right Triangle: SideA = {a}, SideB = {b}, Area = {GetArea():F2}, Perimeter = {GetPerimeter():F2}";
    }
}

class Program
{
    static void Main(string[] args)
    {
        Equilateral eq = new Equilateral(5);
        Console.WriteLine(eq.ToString());

        Isosceles iso = new Isosceles(5, 7);
        Console.WriteLine(iso.ToString());

        Right ri = new Right(5, 7);
        Console.WriteLine(ri.ToString());

    }
}
