using System;
using System.ComponentModel;
using System.Globalization;
using System.Resources;
using Szakdoga.Resources;

namespace Szakdoga
{
    public class LocalizationManager : INotifyPropertyChanged
    {
        // Singleton
        public static LocalizationManager Instance { get; } = new LocalizationManager();

        private LocalizationManager()
        {
            _resourceManager = Strings.ResourceManager;
        }

        private CultureInfo _culture = CultureInfo.CurrentUICulture;
        public CultureInfo Culture
        {
            get => _culture;
            set
            {
                if (_culture != value)
                {
                    _culture = value;
                    OnPropertyChanged("");
                }
            }
        }

        private readonly ResourceManager _resourceManager;

        public string this[string key] => _resourceManager.GetString(key, Culture) ?? $"[{key}]";

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
