using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Yahtzee.UI.Controls;

public partial class DiceControl : UserControl
{
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<DiceControl, int>(nameof(Value), defaultValue: 1);

    public static readonly StyledProperty<bool> IsHeldProperty =
        AvaloniaProperty.Register<DiceControl, bool>(nameof(IsHeld), defaultValue: false);

    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    public bool IsHeld
    {
        get => GetValue(IsHeldProperty);
        set => SetValue(IsHeldProperty, value);
    }

    public event EventHandler? ToggleHoldRequested;

    public DiceControl()
    {
        InitializeComponent();
        Loaded += (s, e) =>
        {
            UpdatePipsDisplay(Value);
            UpdateHeldState(IsHeld);
        };
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ValueProperty)
        {
            UpdatePipsDisplay(change.GetNewValue<int>());
        }
        else if (change.Property == IsHeldProperty)
        {
            UpdateHeldState(change.GetNewValue<bool>());
        }
    }

    private void UpdatePipsDisplay(int val)
    {
        if (Pip00 == null) return;

        Pip00.IsVisible = val is 4 or 5 or 6;
        Pip02.IsVisible = val is 2 or 3 or 4 or 5 or 6;
        Pip10.IsVisible = val is 6;
        Pip11.IsVisible = val is 1 or 3 or 5;
        Pip12.IsVisible = val is 6;
        Pip20.IsVisible = val is 2 or 3 or 4 or 5 or 6;
        Pip22.IsVisible = val is 4 or 5 or 6;

        ValueText.Text = val.ToString();
    }

    private void UpdateHeldState(bool isHeld)
    {
        if (HeldBadge == null || DieCard == null) return;

        HeldBadge.IsVisible = isHeld;
        if (isHeld)
        {
            DieCard.Background = Brush.Parse("#FEF3C7");
            DieCard.BorderBrush = Brush.Parse("#F59E0B");
        }
        else
        {
            DieCard.Background = Brush.Parse("#F8FAFC");
            DieCard.BorderBrush = Brush.Parse("#94A3B8");
        }
    }

    private void OnDieClicked(object? sender, PointerPressedEventArgs e)
    {
        ToggleHoldRequested?.Invoke(this, EventArgs.Empty);
    }
}
