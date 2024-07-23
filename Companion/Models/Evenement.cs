using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Companion.Models
{
    public class Evenement : ObservableObject
    {
        public string Beschrijving { get; set; } = default!;
        public Community Community { get; set; } = default!;

        [ForeignKey("Community")]
        public int CommunityId { get; set; }

        public DateTime DatumEinde { get; set; } = default!;
        public DateTime DatumStart { get; set; } = default!;

        private bool _isUserRegistered;
        public bool IsUserRegistered
        {
            get => _isUserRegistered;
            set
            {
                SetProperty(ref _isUserRegistered, value);
                OnPropertyChanged(nameof(IsJoinButtonVisible));
                OnPropertyChanged(nameof(IsLeaveButtonVisible));
            }
        }

        [Key]
        public int Id { get; set; } = default!;

        private int _aantalDeelnemers;
        public int AantalDeelnemers
        {
            get => _aantalDeelnemers;
            set => SetProperty(ref _aantalDeelnemers, value, nameof(AantalDeelnemers));
        }
        public float Kosten { get; set; } = default!;
        public int MaxDeelnemers { get; set; } = default!;
        public string Naam { get; set; } = default!;
        public List<EvenementenRegistratie> EvenementenRegistratie { get; set; } = default!;

        public ICommand JoinEventCommand { get; set; } = default!;
        public ICommand LeaveEventCommand { get; set; } = default!;

        public bool IsJoinButtonVisible => !IsUserRegistered;
        public bool IsLeaveButtonVisible => IsUserRegistered;
    }
}
