using System.Collections.Generic;

public class ObservableList<T> : List<T>
{
    // Define event handler delegate
    public delegate void ItemAddedEventHandler(T item);

    // Define event
    public event ItemAddedEventHandler ItemAdded;

    // Override Add method to trigger event when an item is added
    public new void Add(T item)
    {
        base.Add(item);
        OnItemAdded(item);
    }

    // Method to trigger the event
    protected virtual void OnItemAdded(T item)
    {
        ItemAdded?.Invoke(item);
    }
}
