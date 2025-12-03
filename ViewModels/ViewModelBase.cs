using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace Avalonia_application.ViewModels
{
    public abstract class ViewModelBase : ObservableObject, IDisposable
    {
        public virtual void Dispose()
        {
            // Очистка ресурсов при необходимости
        }
    }
}