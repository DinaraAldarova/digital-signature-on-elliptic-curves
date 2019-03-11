using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ЭЦП_на_эллиптических_кривых
{
    class Program
    {
        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
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
    public class Curve
    {
        private int p, a, b;
        //private int J;
        private List<Point> group;
        private List<Point> cyclic_group;
        private int m; //порядок группы group
        private int q; //порядок циклической подгруппы cyclic_group
        private int d; //ключ подписи, 0 < d < q
        private int indexcg_Q; //ключ проверки подписи, dP = Q
        private int[] sqrs; //sqrs[i] = i^2 mod p
        private int[] sqrts; //sqrts[i] = sqrt(i) mod p
        private Random rand;
        //int[] prTo1000 = new int[] {257,263,269,271,277,281,283,293,307,311,313,317,331,337,347,349,353,359,
        //367,373,379,383,389,397,401,409,419,421,431,433,439,443,449,457,461,463,467,479,487,491,499,503,509,
        //521,523,541,547,557,563,569,571,577,587,593,599,601,607,613,617,619,631,641,643,647,653,659,661,673,
        //677,683,691,701,709,719,727,733,739,743,751,757,761,769,773,787,797,809,811,821,823,827,829,839,853,
        //857,859,863,877,881,883,887,907,911,919,929,937,941,947,953,967,971,977,983,991,997};

        public Curve(int new_p, int new_a, int new_b)
        {
            rand = new Random();
            group = new List<Point>();
            cyclic_group = new List<Point>();
            p = new_p;
            a = new_a;
            b = new_b;
            //J = (1728 * 4 * a * a * a * (4 * a * a * a + 27 * b * b).GetInverse(p)).Mod(p);
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
                    group.Add(new Point(x, sqrts[y]));
                    if (sqrts[y] != 0)
                        group.Add(new Point(x, -sqrts[y] + p));
                }
            }
            m = group.Count;
            for (int i = 0; i < group.Count; i++)
            {
                int por = 1;
                Point init = group[i];
                Point result = group[i];
                while (!result.is_O)
                {
                    por++;
                    result = Sum(init, result);
                }
                group[i].order = por;
            }
        }
        public void Generate(int indexg_P)
        {
            Point P = group[indexg_P];
            cyclic_group.Add(new Point(true));
            Point result = P;
            while (result.is_O != true)
            {
                cyclic_group.Add(result);
                result = Sum(P, result);
            }
            q = cyclic_group.Count();
            d = rand.Next(1, q - 1);
            indexcg_Q = Mult(1, d);
        }
        public string GenerateSignature(string text)
        {
            int a = hash(text);
            int e = a.Mod(p);
            if (e == 0)
                e = 1;
            bool finish = false;
            int r = 0, s = 0;
            do
            {
                int k = 0;
                k = rand.Next(1, cyclic_group.Count - 1);

                int indexcg_C = Mult(1, k);
                r = cyclic_group[indexcg_C].x.Mod(p);
                if (r == 0)
                {
                    finish = false;
                    continue;
                }
                s = (r * d + k * e).Mod(q);
                if (s == 0)
                {
                    finish = false;
                    continue;
                }
                finish = true;
            }
            while (!finish);

            string signature = Concat(r, s);
            return signature;
        }
        public bool VerifySignature(string text, string signature)
        {
            bool result = false;

            int r = 0, s = 0;
            Split(signature, out r, out s);
            if (!(r > 0 && s > 0 && r < q && s < q))
                return false;

            int a = hash(text);
            int e = a.Mod(p);
            if (e == 0)
                e = 1;

            int v = e.GetInverse(p);
            int z1 = (s * v).Mod(p);
            int z2 = (-r * v).Mod(p);


            int indexcg_C = Sum(Mult(1, z1), Mult(indexcg_Q, z2));
            int RR = group[indexcg_C].x.Mod(p);

            if (RR == r)
                result = true;
            else
                result = false;

            return result;
        }

        private int hash(string text)
        {
            Exception exception = new Exception("Хеш-код вставлен от балды. Переделай!!!");
            int res = 0;
            for (int i = 0; i < text.Length; i++)
                res = (res + (int)text[i]).Mod(256);
            return res;
        }
        private string Concat(int r, int s)
        {
            string res = "";

            res = "0x" + ((UInt32)r).ToString("X8") + "\n";
            res += "0x" + ((UInt32)s).ToString("X8");
            return res;
        }
        private void Split(string sig, out int r, out int s)
        {
            char[] splitchar = { '\n' };
            string[] strs = sig.Split(splitchar);
            r = (Int32)Convert.ToUInt32(strs[0], 16);
            s = (Int32)Convert.ToUInt32(strs[1], 16);
        }

        #region Операции работы с кривой
        private int Count()
        {
            return cyclic_group.Count;
        }
        private Point At(int index)
        {
            if (index >= cyclic_group.Count)
            {
                throw new IndexOutOfRangeException();
            }
            return cyclic_group[index];
        }
        private void Add(Point point)
        {
            cyclic_group.Add(point);
        }
        public int IndexOfMaxOrder()
        {
            int res = 0;
            for (int i = 0; i < group.Count; i++)
            {
                if (group[i].order > group[res].order)
                    res = i;
            }
            return res;
        }
        public int IndexOfRandonMaxOrder()
        {
            int res = IndexOfMaxOrder();
            List<int> indexes = new List<int>();
            indexes.Add(res);
            for (int i = res + 1; i < group.Count; i++)
            {
                if (group[i].order == group[res].order)
                    indexes.Add(i);
            }
            Random x = new Random();
            int index = x.Next().Mod(indexes.Count);
            return index;
        }
        private int IndexOf(int x, int y)
        {
            int i = 1;
            bool end = false;
            for (; i < cyclic_group.Count && !end; i++)
            {
                if (cyclic_group[i].x == x && cyclic_group[i].y == y)
                    end = true;
            }
            if (end)
                return i - 1;
            else
                return -1;
        }
        private int Sum(int index_p1, int index_p2)
        {
            return (index_p1 + index_p2).Mod(cyclic_group.Count);
        }
        private Point Sum(Point p1, Point p2)
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
        private int Mult(int indexInCyclicGroup, int n)
        {
            return (indexInCyclicGroup * n).Mod(cyclic_group.Count);
        }
        #endregion
    }
    public static class Int32Extensions
    {
        public static int GetInverse(this int a, int p)
        {
            // Реализован расширенный алгоритм Евклида
            int c = a, d = p, u, v;
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
            u = ud < 0 ? ud + p : ud;
            v = vd < 0 ? vd + p : vd;

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