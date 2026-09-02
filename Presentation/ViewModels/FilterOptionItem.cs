using CommunityToolkit.Mvvm.ComponentModel;

namespace VideoGameLibraryAndroid.Presentation.ViewModels
{
    // Un checkbox de FilterPage (una plataforma o un género concreto).
    public partial class FilterOptionItem : ObservableObject
    {
        public string Name { get; }

        [ObservableProperty]
        private bool isSelected;

        public FilterOptionItem(string name, bool isSelected)
        {
            Name = name;
            IsSelected = isSelected;
        }
    }
}
