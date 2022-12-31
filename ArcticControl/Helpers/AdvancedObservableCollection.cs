using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace ArcticControl.Helpers;

/// <summary>
/// This observable collection does also trigger on changed when properties of items in it change.
/// The element type must also implement INotifyPropertyChanged
/// </summary>
public class AdvancedObservableCollection<T>: ObservableCollection<T> where T : INotifyPropertyChanged
{
    public AdvancedObservableCollection()
    {
        CollectionChanged += CollectionChanged_Handler;
    }

    private void CollectionChanged_Handler(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // unsubscribe all old objects
        if (e.OldItems != null)
        {
            foreach (T element in e.OldItems)
            {
                element.PropertyChanged -= Item_Changed;
            }
        }

        // subscribe all new objects
        if (e.NewItems != null)
        {
            foreach (T element in e.NewItems)
            {
                element.PropertyChanged += Item_Changed;
            }
        }
    }

    private void Item_Changed(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is T item)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, item, item, IndexOf(item)));
        }
    }
}
