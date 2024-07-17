using Soneta.Business;
using System;
using System.Threading.Tasks;
using Soneta.Kadry;
using Soneta.Types;
using static Rekrutacja.Model.Zadanie2Operacje;
using Rekrutacja.Workers;

[assembly: Worker(typeof(Zadanie2Worker), typeof(Pracownicy))]
namespace Rekrutacja.Workers
{
    public class Zadanie2Worker
    {
        public class Zadanie2WorkerParametry : ContextBase
        {
            [Caption("A")]
            public int A { get; set; }
            [Caption("B")]
            public int B { get; set; }
            [Caption("Data obliczeń")]
            public Date DataObliczen { get; set; }
            [Caption("Figura")]
            public Figury Figura { get; set; }

            public Zadanie2WorkerParametry(Context context) : base(context)
            {
                A = 0;
                DataObliczen = Date.Today;
                Figura = Figury.Kwadrat;
                B = 0;
            }
        }
        [Context]
        public Context Cx { get; set; }
        [Context]
        public Zadanie2WorkerParametry Parametry { get; set; }
        [Action("Zadanie2",
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

            if (Parametry.A == default || Parametry.B == default)
                throw new ArgumentException("Każdy z parametrów musi być różny od 0 aby obliczyć pole powierzchni figury");

            if (Parametry.Figura == Figury.Kwadrat && Parametry.A != Parametry.B)
                throw new ArgumentException($"Gdy wybrana jest figura [{Figury.Kwadrat}] wymagane jest aby długość A oraz B były identyczne");

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
                        pracownikZSesja.Features["Wynik"] = await FiguraAsync(Parametry.A, Parametry.B, Parametry.Figura);
                    }

                    trans.CommitUI();
                }
                nowaSesja.Save();
            }
        }

        async Task<double> FiguraAsync(double zmiennaA, double zmiennB, Figury figura)
        {
            // w zadaniu2 jest wymaganzy wynik w postaci int, takiej wartosci nie mozemy ustawic na czesie "Wynik" gdyz oczekuje typu double. 

            switch (figura)
            {
                case Figury.Kwadrat: return await Task.FromResult(Convert.ToInt32(Math.Pow(zmiennaA, 2)));
                case Figury.Prostokat: return await Task.FromResult(Convert.ToInt32(zmiennaA * zmiennB));
                case Figury.Trojkat: return await Task.FromResult(Convert.ToInt32(zmiennaA * zmiennB) / 2);
                case Figury.Kolo: return await Task.FromResult(Convert.ToInt32(Math.PI * Math.Pow(zmiennaA, 2)));
                default: throw new NotImplementedException("Brak wzoru");
            };
        }
    }
}
