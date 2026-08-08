using ECGP;
using ExamClock.Mvvm;
using TimeSync;

namespace ExamClock.Admin.ViewModels
{
    public class RoomViewModel : ViewModelBase
    {
        public RoomInfo Room
        {
            get => _room;
            set => SetProperty(ref _room, value);
        }
        private RoomInfo _room;

        public TimeKeeper Time
        {
            get => _time;
            set => SetProperty(ref _time, value);
        }
        private TimeKeeper _time;

        public bool IsCorrect
        {
            get => _isCorrect;
            set => SetProperty(ref _isCorrect, value);
        }
        private bool _isCorrect;

        public bool IsTimeCorrect
        {
            get => _isTimeCorrect;
            set => SetProperty(ref _isTimeCorrect, value);
        }
        private bool _isTimeCorrect;
    }
}
