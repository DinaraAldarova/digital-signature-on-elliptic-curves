using System;
using System.Collections.Generic;
using System.Linq;
//using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Numerics;
using System.Collections;

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
        public BigInteger x;
        public BigInteger y;
        public bool is_O;
        public Point(BigInteger x, BigInteger y)
        {
            this.x = x;
            this.y = y;
            is_O = false;
        }
        public Point(bool is_O)
        {
            x = 0;
            y = 0;
            this.is_O = is_O;
        }
        public Point (Point point)
        {
            x = point.x;
            y = point.y;
            is_O = point.is_O;
        }
        public override string ToString()
        {
            if (is_O)
                return "O";
            else
                return "(" + x.ToString() + "; " + y.ToString() + ")";
        }
    }
    public class Curve
    {
        private BigInteger p, a, b;
        private BigInteger m; //порядок группы
        private BigInteger q; //порядок циклической подгруппы
        private Point P; //генератор циклической подгруппы
        private BigInteger d; //ключ подписи, 0 < d < q
        private Point Q; //ключ проверки подписи, dP = Q
        
        public Curve ()
        {
            p = BigInteger.Parse("57896044618658097711785492504343953926634992332820282019728792003956564821041");
            a = 7;
            b = BigInteger.Parse("43308876546767276905765904595650931995942111794451039583252968842033849580414");
            m = BigInteger.Parse("57896044618658097711785492504343953927082934583725450622380973592137631069619");
            q = BigInteger.Parse("57896044618658097711785492504343953927082934583725450622380973592137631069619");
            BigInteger Px, Py, Qx, Qy;
            Px = 2;
            Py = BigInteger.Parse("4018974056539037503335449422937059775635739389905545080690979365213431566280");
            P = new Point(Px, Py);
            d = BigInteger.Parse("55441196065363246126355624130324183196576709222340016572108097750006097525544");
            Qx = BigInteger.Parse("57520216126176808443631405023338071176630104906313632182896741342206604859403");
            Qy = BigInteger.Parse("17614944419213781543809391949654080031942662045363639260709847859438286763994");
            Q = new Point(Qx, Qy);
        }
        public Curve(BigInteger p, BigInteger a, BigInteger b, BigInteger m, BigInteger q, Point P, BigInteger d, Point Q)
        {
            this.p = p;
            this.a = a;
            this.b = b;
            this.m = m;
            this.q = q;
            this.P = P;
            this.d = d;
            this.Q = Q;
        }
        public string GenerateSignature(string text)
        {
            BigInteger a = hash(text);
            BigInteger e = a.Mod(p);
            if (e == 0)
                e = 1;
            /*DeleteMe*/
            e = BigInteger.Parse("20798893674476452017134061561508270130637142515379653289952617252661468872421");
            bool finish = false;
            BigInteger r = 0, s = 0;
            do
            {
                BigInteger k = 0;
                k = RandomGen.NextBigInteger(1, q - 1);
                /*DeleteMe*/
                k = BigInteger.Parse("53854137677348463731403841147996619241504003434302020712960838528893196233395");
                Point C = Mult(P, k);
                /*DeleteMe*/
                BigInteger Cx = BigInteger.Parse("29700980915817952874371204983938256990422752107994319651632687982059210933395");
                BigInteger Cy = BigInteger.Parse("32842535278684663477094665322517084506804721032454543268132854556539274060910");
                C = new Point(Cx, Cy);
                r = C.x.Mod(p);
                ///*DeleteMe*/
                r = BigInteger.Parse("29700980915817952874371204983938256990422752107994319651632687982059210933395");
                if (r == 0)
                {
                    finish = false;
                    continue;
                }
                s = (r * d + k * e).Mod(q);
                /*DeleteMe*/
                s = BigInteger.Parse("574973400270084654178925310019147038455227042649098563933718999175515839552");
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

            BigInteger r = 0, s = 0;
            Split(signature, out r, out s);
            if (!(r > 0 && s > 0 && r < q && s < q))
                return false;

            BigInteger a = hash(text);
            BigInteger e = a.Mod(p);
            if (e == 0)
                e = 1;

            BigInteger v = e.GetInverse(p);
            BigInteger z1 = (s * v).Mod(p);
            BigInteger z2 = (-r * v).Mod(p);


            Point C = Sum(Mult(P, z1), Mult(Q, z2));
            BigInteger RR = C.x.Mod(p);

            if (RR == r)
                result = true;
            else
                result = false;

            return result;
        }

        private BigInteger hash(string text)
        {
            Exception exception = new Exception("Хеш-код вставлен от балды. Переделай!!!");
            BigInteger res = 0;
            for (int i = 0; i < text.Length; i++)
                res = (res + new BigInteger(text[i])).Mod(256);
            return res;
        }
        private string Concat(BigInteger r, BigInteger s)
        {
            return r.ToString() + "\n" + s.ToString();
        }
        private void Split(string sig, out BigInteger r, out BigInteger s)
        {
            char[] splitchar = { '\n' };
            string[] strs = sig.Split(splitchar);
            r = BigInteger.Parse(strs[0]);
            s = BigInteger.Parse(strs[1]);
        }

        #region Операции работы с кривой
        private Point Sum(Point p1, Point p2)
        {
            if (p1.is_O)
                return p2;
            else if (p2.is_O)
                return p1;

            bool is_O = false;

            BigInteger tg = 0;
            if (p1.x == p2.x && p1.y == p2.y)
            {
                BigInteger value = (2 * p1.y).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((3 * p1.x * p1.x + a) * value.GetInverse(p)).Mod(p);
            }
            else
            {
                BigInteger value = (p2.x - p1.x).Mod(p);
                if (value == 0)
                    is_O = true;
                else
                    tg = ((p2.y - p1.y) * value.GetInverse(p)).Mod(p);
            }

            BigInteger x = 0, y = 0;
            if (is_O)
                return new Point(true);
            else
            {
                x = (tg * tg - p1.x - p2.x).Mod(p);
                y = (tg * (p1.x - x) - p1.y).Mod(p);
                return new Point(x, y);
            }
        }
        private Point Mult(Point point, BigInteger n)
        {
            BitArray number = new BitArray(n.ToByteArray());

            Point power = new Point(point);

            Point result = number[0] ? new Point( power) : new Point(true);

            for (int i = 1; i < number.Count; i++)
            {
                power = Sum(power, power);
                if (number[i])
                {
                    result = Sum(result, power);
                }
            }

            return result;
        }
        #endregion
    }
    public static class Int32Extensions
    {
        private static int[] smallPrimes = new int[] {
                2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97,
                101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199,
                211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293,
                307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397,
                401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499,
                503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599,
                601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691,
                701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797,
                809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887,
                907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997
            };
        public static BigInteger GetInverse(this BigInteger a, BigInteger p)
        {
            // Реализован расширенный алгоритм Евклида
            BigInteger c = a, d = p, u, v;
            BigInteger uc = 1, vc = 0, ud = 0, vd = 1;

            while (!c.Equals(BigInteger.Zero))
            {
                BigInteger q = d / c;
                BigInteger temp;
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
        public static BigInteger PowMod(this BigInteger a, BigInteger pow, BigInteger mod)
        {
            return BigInteger.ModPow(a, pow, mod);
        }
        public static BigInteger Mod(this BigInteger a, BigInteger p)
        {
            BigInteger first = BigInteger.ModPow(a, BigInteger.One, p);
            BigInteger second = ((a % p) + p) % p;
            return first;
        }
    }

    public class RandomGen
    {
        /// <summary>
        /// Возвращает случайное число, которое лежит в заданном диапазоне
        /// </summary>
        public static BigInteger NextBigInteger(BigInteger left, BigInteger right)
        {
            byte[] max = (right - left).ToByteArray();
            int length = max.Length;
            byte[] res = new byte[length];
            Random x = new Random();
            res[length - 1] = Convert.ToByte(x.Next(Convert.ToInt32(max[length - 1])));
            for (int i = 0; i < length-1; i++)
            {
                res[i] = Convert.ToByte(x.Next(byte.MaxValue + 1));
            }
            return new BigInteger(res);
        }
    }
}
