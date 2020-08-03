using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SimpleFrame {
    internal abstract class ViewModelBase : INotifyPropertyChanged {
        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Updates a property only if it is different, and calls OnPropertyChanged.
        /// </summary>
        /// <returns>If the value was different and toUpdate was assigned to.</returns>
        protected bool UpdateProperty<T>(ref T toUpdate, T value, [CallerMemberName] string? propertyName = null) {
            var changed = !EqualityComparer<T>.Default.Equals(toUpdate, value);

            if (changed) {
                toUpdate = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            return changed;
        }

        protected bool UpdateProperty<T>(Action<T> changer, T original, T value, [CallerMemberName] string? propertyName = null) {
            var changed = !EqualityComparer<T>.Default.Equals(original, value);

            if (changed) {
                changer(value);
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }

            return changed;
        }

    }
}
