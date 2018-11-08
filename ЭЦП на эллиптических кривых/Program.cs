using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ЭЦП_на_эллиптических_кривых
{
    class Program
    {
        const int p = 41;
        const int a = 1, b = 3; //опорная точка

        static void Main(string[] args)
        {
            List<KeyValuePair<int, int>> points = new List<KeyValuePair<int, int>>();//точки, принадлежащие прямой и удовлетворяющие условию
            int[] sqrs = new int[p]; //sqrs[i] = i^2 mod p
            int[] sqrts = new int[p]; //sqrts[i] = sqrt(i) mod p
            List<int> order = new List<int>(); //порядок points[i] = order[i]

            for (int i = 1; i < p; i++)
            {
                //if (i <= p / 2)
                sqrs[i] = (i * i).Mod(p);
                if (sqrts[(i * i).Mod(p)] == 0)
                    sqrts[(i * i).Mod(p)] = i;
            }
            int x = 0;
            for (; x < p; x++)
            {
                int y = (x * x * x + x + 3).Mod(p);
                if (sqrs.Contains(y))
                {
                    points.Add(new KeyValuePair<int, int>(x, sqrts[y]));
                    if (sqrts[y] != 0)
                        points.Add(new KeyValuePair<int, int>(x, -sqrts[y] + p));
                }
            }
            //foreach (var point in points)
            //{
            //    Console.WriteLine($"{point.Key} {point.Value}");
            //}
            //Console.WriteLine(points.Count);
            for (int i = 0; i < points.Count; i++)
            {
                int por = 1;
                try
                {
                    KeyValuePair<int, int> init = points[i];//new KeyValuePair<int, int>(27, 22);// points[0];
                    KeyValuePair<int, int> result = points[i];//new KeyValuePair<int, int>(27, 22);// points[0];
                    while (true)
                    {
                        por++;
                        result = Sum(init, result);
                        //Console.WriteLine(result);

                    }
                }
                catch (DivideByZeroException ex)
                {
                    Console.WriteLine($"Порядок точки ({points[i].Key};{points[i].Value}) равен {por}");
                    order.Add(por);
                }
            }
            //здесь можно определить свой генератор группы
            int index_gen = order.IndexOf(order.Max());
            KeyValuePair<int, int> gen = points[index_gen];

            List<KeyValuePair<int, int>> group = new List<KeyValuePair<int, int>>();
            try
            {
                KeyValuePair<int, int> init = gen;//new KeyValuePair<int, int>(27, 22);// points[0];
                group.Add(gen);
                KeyValuePair<int, int> result = gen;//new KeyValuePair<int, int>(27, 22);// points[0];
                while (true)
                {
                    result = Sum(init, result);
                    group.Add(result);
                }
            }
            catch (DivideByZeroException ex)
            {
                foreach (var value in group)
                    Console.WriteLine($"({value.Key};{value.Value})");
            }
            bool result_is_O = false;
            do
            {
                Console.WriteLine($"Всего точек в группе {group.Count() + 1} (в том числе О)");
                int n;
                Console.WriteLine($"Введите число n: ");
                do
                {
                    n = Convert.ToInt32(Console.ReadLine());
                    if (n > group.Count || n <= 1)
                        Console.WriteLine($"Пожалуйста, введите другое число n: ");
                }
                while (n > group.Count || n <= 1);

                KeyValuePair<int, int> openKey = group[Mult(group, 0, n)];
                KeyValuePair<int, int> otherOpenKey, closeKey;
                Console.WriteLine($"Открытый ключ: ({openKey.Key};{openKey.Value})");
                do
                {
                    Console.WriteLine($"Введите второй открытый ключ: ");
                    int x1 = Convert.ToInt32(Console.ReadLine());
                    int y1 = Convert.ToInt32(Console.ReadLine());
                    otherOpenKey = new KeyValuePair<int, int>(x1, y1);
                    if (!group.Contains(otherOpenKey))
                        Console.WriteLine($"Такого элемента в группе нет!");
                }
                while (!group.Contains(otherOpenKey));
                int index = Mult(group, group.IndexOf(otherOpenKey), n);
                if (index != group.Count)//если не О
                {
                    closeKey = group[index];
                    Console.WriteLine($"Закрытый ключ: ({closeKey.Key};{closeKey.Value})");
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

        public static KeyValuePair<int, int> Sum(KeyValuePair<int, int> p1, KeyValuePair<int, int> p2)
        {
            //if ((p1.Value + p2.Value).Mod(p) == 0)
            //{
            //    throw new DivideByZeroException();
            //}

            int tg;
            if (p1.Key == p2.Key && p1.Value == p2.Value)
            {
                int value = (2 * p1.Value).Mod(p);
                if (value == 0)
                    throw new DivideByZeroException();
                tg = ((3 * p1.Key * p1.Key + a) * value.GetInverse(p)).Mod(p);
            }
            else
            {
                int value = (p2.Key - p1.Key).Mod(p);
                if (value == 0)
                    throw new DivideByZeroException();
                tg = ((p2.Value - p1.Value) * value.GetInverse(p)).Mod(p);
            }

            int x = (tg * tg - p1.Key - p2.Key).Mod(p);
            int y = (tg * (p1.Key - x) - p1.Value).Mod(p);

            return new KeyValuePair<int, int>(x, y);
        }
        public static int Mult(List<KeyValuePair<int, int>> group, int indexInGroup, int n)
        {
            return ((indexInGroup + 1) * n - 1).Mod(group.Count + 1);
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
        public static int Mod(this int a, int p)
        {
            return ((a % p) + p) % p;
        }
    }
}
