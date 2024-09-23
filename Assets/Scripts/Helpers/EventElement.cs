using System;

public class EventElement<T>
{
    protected event Action<T> eventdelegate;

    public void Invoke(T kEvent)
    {
        if (eventdelegate != null)
        {
            eventdelegate(kEvent);
        }
    }

    public static EventElement<T> operator +(EventElement<T> kElement, Action<T> kDelegate)
    {
        kElement.eventdelegate += kDelegate;
        return kElement;
    }

    public static EventElement<T> operator -(EventElement<T> kElement, Action<T> kDelegate)
    {
        kElement.eventdelegate -= kDelegate;
        return kElement;
    }
}
