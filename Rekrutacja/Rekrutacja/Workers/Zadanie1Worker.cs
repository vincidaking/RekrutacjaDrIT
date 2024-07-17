using Soneta.Business;
using System;
using System.Linq;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.Types;
using Rekrutacja.Workers.Template;
using Rekrutacja.Model;

[assembly: Worker(typeof(Zadanie1Worker), typeof(Pracownicy))]
namespace Rekrutacja.Workers.Template
{
    public class Zadanie1Worker
    {
        public class Zadanie1WorkerParametry : ContextBase
        {
            [Caption("A")]
            public int A { get; set; }
            [Caption("B")]
            public int B { get; set; }
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("Operacja")]
            public string Operacja { get; set; }

            public Zadanie1WorkerParametry(Context context) : base(context)
            {
                A = 0;
                DataObliczen = Date.Today;
                Operacja = "+";
                B = 0;
            }
        }
        [Context]
        public Context Cx { get; set; }
        [Context]
        public Zadanie1WorkerParametry Parametry { get; set; }
        [Action("Zadanie1",
           Description = "Prosty kalkulator ",
           Priority = 10,
           Mode = ActionMode.ReadOnlySession,
           Icon = ActionIcon.Accept,
           Target = ActionTarget.ToolbarWithText)]
        public async void WykonajAkcje()
        {
            Pracownik[] pracownik = null;
            DebuggerSession.MarkLineAsBreakPoint();

            if (Parametry.DataObliczen < Date.Today)
                throw new ArgumentException($"Data obliczen powinna być równa lub wiekszą od aktualnej daty");

            if (Parametry.Operacja.Length != 1)
                throw new ArgumentException($"Podano nie prawidłowy znak dozwolone: [{string.Join(", ", Zadanie1Operacje.DozwoloneZnaki)}]");

            var operacjaChar = Parametry.Operacja.Single();

            if (!Zadanie1Operacje.DozwoloneZnaki.Any(x => x.Equals(operacjaChar)))
                throw new ArgumentException($"Podano nie prawidłowy znak: [{operacjaChar}] dozwolone: [{string.Join(", ", Zadanie1Operacje.DozwoloneZnaki)}]");

            if (Cx.Contains(typeof(Pracownik[])))
                pracownik = (Pracownik[])Cx[typeof(Pracownik[])];

            if (pracownik is null)
                throw new ArgumentException("Nie zaznaczono żadnego pracownika");

            using (var nowaSesja = this.Cx.Login.CreateSession(false, false, "ModyfikacjaPracownika"))
            {
                using (var trans = nowaSesja.Logout(true))
                {
                    foreach (var item in pracownik)
                    {
                        var pracownikZSesja = nowaSesja.Get(item);

                        pracownikZSesja.Features["DataObliczen"] = Parametry.DataObliczen;
                        pracownikZSesja.Features["Wynik"] = await ArytmetykaAsync(Parametry.A, Parametry.B, operacjaChar);
                    }

                    trans.CommitUI();
                }
                nowaSesja.Save();
            }
        }

        async Task<double> ArytmetykaAsync(double zmiennaA, double zmiennB, char operacja)
        {
            switch (operacja)
            {
                case '+': return await Task.FromResult(zmiennaA + zmiennB);
                case '-': return await Task.FromResult(zmiennaA - zmiennB);
                case '*': return await Task.FromResult(zmiennaA * zmiennB);
                case '/':
                    {
                        if (zmiennaA == default || zmiennB == default)
                            throw new AggregateException("Błąd arytmetyczny, jedno z parametrów jest równe 0");

                        return await Task.FromResult(zmiennaA / zmiennB);
                    }
                default: throw new AggregateException("Błędny znak operacji");
            };
        }
    }
}