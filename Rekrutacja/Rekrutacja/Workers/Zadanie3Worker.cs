using Rekrutacja.Workers;
using Soneta.Business;
using Soneta.Kadry;
using Soneta.Kasa.Extensions;
using Soneta.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Rekrutacja.Model.Zadanie2Operacje;

[assembly: Worker(typeof(Zadanie3Worker), typeof(Pracownicy))]
namespace Rekrutacja.Workers
{

    public class Zadanie3Worker
    {
        public class Zadanie3WorkerParametry : ContextBase
        {
            [Caption("A")]
            public string A { get; set; }
            [Caption("B")]
            public string B { get; set; }
            public Zadanie3WorkerParametry(Context context) : base(context)
            {
                A = string.Empty;
                B = string.Empty;
            }
        }
        [Context]
        public Context Cx { get; set; }
        [Context]
        public Zadanie3WorkerParametry Parametry { get; set; }
        [Action("Zadanie3",
           Description = "Parser ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public string WykonajAkcje()
        {
            var sb = new StringBuilder();

            try
            {
                var tmp = TryParseStringToIntByKP(Parametry.A);
                sb.Append("Poprawne parsowanie parametru A: [")
                    .Append(tmp)
                    .Append("]")
                    .AppendLine();
            }
            catch (Exception ex)
            {
                sb.Append("Błedne parsowanie parametru A: [")
                    .Append(Parametry.A)
                    .Append("] przyczyna błedu: [")
                    .Append(ex.Message)
                    .Append("]")
                    .AppendLine();
            }
            try
            {
                var tmp = TryParseStringToIntByKP(Parametry.B);
                sb.Append("Poprawne parsowanie parametru B: [")
                    .Append(tmp)
                    .Append("]")
                    .AppendLine();
            }
            catch (Exception ex)
            {
                sb.Append("Błedne parsowanie parametru B: [")
                    .Append(Parametry.B)
                    .Append("] przyczyna błedu: [")
                    .Append(ex.Message)
                    .Append("]")
                    .AppendLine();
            }

            return sb.ToString();
        }

        private static int TryParseStringToIntByKP(string wartosc)
        {
            if (string.IsNullOrEmpty(wartosc))
                throw new ArgumentNullException("Nie podano wartości dla parametru");

            var valuesDot = new List<char> { ',', '.' };
            
            var valuesCharInt = new Dictionary<char, int>();
            Enumerable.Range(0, 10)
                .ToList()
                .ForEach(x => valuesCharInt.Add(Convert.ToChar(x.ToString()), x));

            if (wartosc.Where(x => x.Equals(",") || x.Equals(".")).Count() > 0)
                throw new ArgumentException("Wartość zawiera wiecej niż jedno z dozwolonych znakow: [ , . ]");

            var nieDopuszczalneZnaki = wartosc
                .Where(x => !valuesDot.Any(y => y.Equals(x)))
                .Where(x => !valuesCharInt.Select(y => y.Key).Any(y => y.Equals(x)))
                .ToList();

            if (nieDopuszczalneZnaki.Count > 0)
                throw new ArgumentException($"W wartości znajdują się znaki nie będące cyframi: [{string.Join(" ,", nieDopuszczalneZnaki)}]");

            var ciagDoKropki = wartosc
                .TakeWhile(x => !valuesDot.Any(y => y.Equals(x)))
                .ToList();

            ciagDoKropki.Reverse();

            int result = 0, number = 1;

            foreach (var item in ciagDoKropki)
            {
                result += valuesCharInt[item] * number;
                number *= 10;
            }

            return result;
        }
    }
}
