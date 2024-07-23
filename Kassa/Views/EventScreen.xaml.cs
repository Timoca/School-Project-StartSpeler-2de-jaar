using Kassa.Models;
using Kassa.ViewModels;
using System.Diagnostics;

namespace Kassa.Views;

public partial class EventScreen : ContentPage
{
    public EventScreen(EventScreenViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        Loaded += async (sender, args) =>
        {
            Debug.WriteLine("Loaded event fired.");
            await vm.InitializeAsync();
        };
    }

    private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.Count > 0)
        {
            ((EventScreenViewModel)BindingContext).SelectedEvent = (Evenement)e.CurrentSelection[0];
        }
    }

    private void OnJoinEventClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var selectedEvent = (Evenement)button.BindingContext;
        ((EventScreenViewModel)BindingContext).JoinEventCommand.Execute(selectedEvent);
    }

    private void OnLeaveEventClicked(object sender, EventArgs e)
    {
        var button = (Button)sender;
        var selectedEvent = (Evenement)button.BindingContext;
        ((EventScreenViewModel)BindingContext).LeaveEventCommand.Execute(selectedEvent);
    }
}