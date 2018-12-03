using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЭЦП_на_эллиптических_кривых
{
    class Program
    {
        //Эллиптическая кривая
        const int p = 41;
        const int a = 1, b = 3; //опорная точка

        static void Main(string[] args)
        {
            Group group = new Group(p, a, b);
            //здесь можно определить свой генератор группы
            group.Generate(group.IndexOfMaxOrder());

        }
    }
    public class Point
    {
        public int x;
        public int y;
        public int order;
        public bool is_O;
        public Point(int x, int y, int order)
        {
            this.x = x;
            this.y = y;
            this.order = order;
            is_O = false;
        }
        public Point(int x, int y)
        {
            this.x = x;
            this.y = y;
            order = -1;
            is_O = false;
        }
        public Point(bool is_O)
        {
            x = 0;
            y = 0;
            order = -1;
            this.is_O = is_O;
        }
    }
    public class Group
    {
        public int p, a, b;
        public List<Point> all_points;
        public List<Point> points;
        public int[] sqrs; //sqrs[i] = i^2 mod p
        public int[] sqrts; //sqrts[i] = sqrt(i) mod p
        public Group(int new_p, int new_a, int new_b)
        {
            all_points = new List<Point>();
            points = new List<Point>();
            p = new_p;
            a = new_a;
            b = new_b;
            sqrs = new int[p]; //sqrs[i] = i^2 mod p
            sqrts = new int[p]; //sqrts[i] = sqrt(i) mod p
            for (int i = 1; i < p; i++)
            {
                sqrs[i] = (i * i).Mod(p);
                if (sqrts[(i * i).Mod(p)] == 0)
                    sqrts[(i * i).Mod(p)] = i;
            }
            for (int x = 0; x < p; x++)
            {
                int y = (x * x * x + x + 3).Mod(p);
                if (sqrs.Contains(y))
                {
                    all_points.Add(new Point(x, sqrts[y]));
                    if (sqrts[y] != 0)
                        all_points.Add(new Point(x, -sqrts[y] + p));
                }
            }
            for (int i = 0; i < all_points.Count; i++)
            {
                int por = 1;
                Point init = all_points[i];
                Point result = all_points[i];
                while (!result.is_O)
                {
                    por++;
                    result = Sum(init, result);
                }
                all_points[i].order = por;
            }
        }
        public void Generate(int index)
        {
            Point gen = all_points[index];
            points.Add(new Point(true));
            Point result = gen;
            while (result.is_O != true)
            {
                points.Add(result);
                Console.WriteLine($"({result.x};{result.y})");
                result = Sum(gen, result);
            }

            bool result_is_O = false;
            do
            {
                Console.WriteLine($"Всего точек в группе {points.Count()} (в том числе О)");
                int n;
                Console.WriteLine($"Введите число n: ");
                do
                {
                    n = Convert.ToInt32(Console.ReadLine());
                    if (n >= points.Count - 1 || n <= 1)
                        Console.WriteLine($"Пожалуйста, введите другое число n: ");
                }
                while (n >= points.Count - 1 || n <= 1);

                int index_openKey = Mult(1, n);
                int index_otherOpenKey, index_closeKey;
                Console.WriteLine($"Открытый ключ: ({points[index_openKey].x};{points[index_openKey].y})");
                do
                {
                    Console.WriteLine($"Введите второй открытый ключ: ");
                    int x1 = Convert.ToInt32(Console.ReadLine());
                    int y1 = Convert.ToInt32(Console.ReadLine());
                    index_otherOpenKey = IndexOf(x1, y1);
                    if (index_otherOpenKey == -1)
                        Console.WriteLine($"Такого элемента в группе нет!");
                }
                while (index_otherOpenKey == -1);
                index_closeKey = Mult(index_otherOpenKey, n);
                if (index_closeKey != 0)//если не О
                {
                    Console.WriteLine($"Закрытый ключ: ({points[index_closeKey].x};{points[index_closeKey].y})");
                    result_is_O = false;
                }
                else
                {
                    Console.WriteLine($"Закрытый ключ: O. Пожалуйста, повторите все с начала");
                    result_is_O = true;
                }
            }
            while (result_is_O);
        }
        public int Count()
        {
            return points.Count;
        }
        public Point At(int index)
        {
            if (index >= points.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return points[index];
        }
        public void Add(Point point)
        {
            points.Add(point);
        }
        public int IndexOfMaxOrder()
        {
            int res = 0;
            for (int i = 0; i < all_points.Count; i++)
            {
                if (all_points[i].order > all_points[res].order)
                    res = i;
            }
            return res;
        }
        public int IndexOfRandonMaxOrder()
        {
            int res = IndexOfMaxOrder();
            List<int> indexes = new List<int>();
            indexes.Add(res);
            for (int i = res + 1; i < all_points.Count; i++)
            {
                if (all_points[i].order == all_points[res].order)
                    indexes.Add(i);
            }
            Random x = new Random();
            int index = x.Next().Mod(indexes.Count);
            return index;
        }
        public int IndexOf(int x, int y)
        {
            int i = 1;
            bool end = false;
            for (; i < points.Count && !end; i++)
            {
                if (points[i].x == x && points[i].y == y)
                    end = true;
            }
            if (end)
                return i - 1;
            else
                return -1;
        }
        public int Sum(int index_p1, int index_p2)
        {
            return (index_p1 + index_p2).Mod(points.Count);
        }
        public Point Sum(Point p1, Point p2)
        {
            bool is_O = false;

            int tg = 0;
            if (p1.x == p2.x && p1.y == p2.y)
            {
                int value = (2 * p1.y).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((3 * p1.x * p1.x + a) * value.GetInverse(p)).Mod(p);
            }
            else
            {
                int value = (p2.x - p1.x).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((p2.y - p1.y) * value.GetInverse(p)).Mod(p);
            }

            int x = 0, y = 0;
            if (is_O)
                return new Point(true);
            else
            {
                x = (tg * tg - p1.x - p2.x).Mod(p);
                y = (tg * (p1.x - x) - p1.y).Mod(p);
                return new Point(x, y);
            }
        }
        public int Mult(int indexInGroup, int n)
        {
            return (indexInGroup * n).Mod(points.Count);
        }
    }
    public static class Int32Extensions
    {
        public static int GetInverse(this int a, int b)
        {
            // Реализован расширенный алгоритм Евклида
            int c = a, d = b, u, v;
            int uc = 1, vc = 0, ud = 0, vd = 1;

            while (c != 0)
            {
                int q = d / c;
                int temp;
                temp = c;
                c = d - q * c;
                d = temp;

                temp = uc;
                uc = ud - q * uc;
                ud = temp;

                temp = vc;
                vc = vd - q * vc;
                vd = temp;
            }
            u = ud < 0 ? ud + b : ud;
            v = vd < 0 ? vd + b : vd;

            return (d == 1) ? u : 0;
        }
        public static int PowMod(this int a, int pow, int mod)
        {
            a = a.Mod(mod);
            int res = 1;
            int buf = a;
            for (int i = 1; i <= pow; i *= 2)
            {
                if (i > 1)
                    buf = (buf * buf).Mod(mod);
                if ((pow & i) > 0)
                {
                    res *= buf;
                }
            }
            return res.Mod(mod);
        }
        public static int Mod(this int a, int p)
        {
            return ((a % p) + p) % p;
        }
    }
}