using System.Collections.ObjectModel;
using CarnetDepenses.Models;
using CarnetDepenses.Services;
using Microsoft.Maui.Storage;
namespace CarnetDepenses.ViewModels;

public class DepensesViewModel : BaseViewModel
{
    private readonly IDepenseRepository _repo;
    private string _libelle = "", _montantTexte = "";

    public ObservableCollection<Depense> Depenses { get; }
        = new();
    public decimal Total => Depenses.Sum(d => d.Montant);

    public string Libelle
    {
        get => _libelle;
        set { if (SetField(ref _libelle, value)) AjouterCommand.Rafraichir(); }
    }

    public string MontantTexte
    {
        get => _montantTexte;
        set { if (SetField(ref _montantTexte, value)) AjouterCommand.Rafraichir(); }
    }

    public RelayCommandP AjouterCommand { get; }
    public RelayCommandP SupprimerCommand { get; }

    public DepensesViewModel(IDepenseRepository repo, IApiService api)
    {
        _repo = repo;
        _api = api;
        AjouterCommand = new RelayCommandP(
            async _ => await AjouterAsync(),
            _ => !string.IsNullOrWhiteSpace(Libelle)
                && decimal.TryParse(MontantTexte, out var montant)
                && montant > 0);
        SupprimerCommand = new RelayCommandP(
            async p =>
            {
                if (p is Depense depense)
                    await SupprimerAsync(depense);
            });

        ConvertirCommand = new RelayCommand(
            async () => await ConvertirAsync(), () => !IsBusy);
    }

    public async Task ChargerAsync()
    {
        Depenses.Clear();
        foreach (var d in await _repo.ListerAsync())
            Depenses.Add(d);
        OnPropertyChanged(nameof(Total));
    }

    private async Task AjouterAsync()
    {
        if (!decimal.TryParse(MontantTexte, out decimal m))
            return;
        var d = new Depense { Libelle = Libelle, Montant = m };
        await _repo.AjouterAsync(d);
        Depenses.Add(d);
        OnPropertyChanged(nameof(Total));
        Libelle = ""; MontantTexte = "";
    }

    private async Task SupprimerAsync(Depense d)
    {
        await _repo.SupprimerAsync(d);
        Depenses.Remove(d);
        OnPropertyChanged(nameof(Total));
    }

    private readonly IApiService _api;
    private bool _isBusy;
    private string _totalConverti = "", _message = "";

    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetField(ref _isBusy, value))
                ConvertirCommand.Rafraichir();
        }
    }
    public string TotalConverti
    {
        get => _totalConverti;
        set => SetField(ref _totalConverti, value);
    }
    public string Message
    {
        get => _message; set => SetField(ref _message, value);
    }
    public RelayCommand ConvertirCommand { get; }
    private async Task ConvertirAsync()
    {
        IsBusy = true; Message = "";
        try
        {
            var t = await _api.GetTauxAsync();
            Preferences.Set("taux_eur",
                (double)t.Taux["EUR"]);
            TotalConverti = $"{Total * t.Taux["EUR"]:F2} EUR";
        }
        catch (HttpRequestException)
        {
            double c = Preferences.Get("taux_eur", 0.0);
            if (c > 0) Message = "Hors ligne, pas de taux.";
            else
                TotalConverti =
                    $"{Total * (decimal)c:F2} EUR (cache)";
            Message = "Hors ligne : dernier taux connu.";
        }
        finally { IsBusy = false; }
    }
}
