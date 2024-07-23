using Companion.ViewModels;
using System.Diagnostics;
using Microsoft.Maui.Controls;
using System;
using Companion.Models;

namespace Companion.Views;

public partial class EventPagina : ContentPage
{
    public EventPagina(EventViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Loaded += async (sender, args) =>
        {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }

    private void OnHamburgerMenuTapped(object sender, EventArgs e)
    {
        Shell.Current.FlyoutIsPresented = !Shell.Current.FlyoutIsPresented;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!(BindingContext is EventViewModel viewModel)) return;

        var isAuthenticated = await viewModel.CheckAuthentication();
        if (!isAuthenticated)
        {
            try
            {
                await Task.Delay(10); // Wacht even totdat de Shell is geladen.
                await Shell.Current.GoToAsync(nameof(LoginPagina));
            }
            catch (Exception ex)
            {
                // Log de fout of toon een bericht
                Debug.WriteLine($"Navigatiefout: {ex.Message}");
            }
        }
    }

}