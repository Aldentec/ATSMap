using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace ProHauler.UI.Behaviors
{
    /// <summary>
    /// Attached behavior that provides smooth color transitions for TextBlock foreground changes.
    /// </summary>
    public static class SmoothColorTransitionBehavior
    {
        /// <summary>
        /// Attached property to enable smooth color transitions.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(SmoothColorTransitionBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        /// <summary>
        /// Attached property to store the previous color.
        /// </summary>
        private static readonly DependencyProperty PreviousColorProperty =
            DependencyProperty.RegisterAttached(
                "PreviousColor",
                typeof(Color?),
                typeof(SmoothColorTransitionBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// Gets the IsEnabled property value.
        /// </summary>
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// Sets the IsEnabled property value.
        /// </summary>
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TextBlock textBlock)
            {
                if ((bool)e.NewValue)
                {
                    // Store initial color
                    if (textBlock.Foreground is SolidColorBrush initialBrush)
                    {
                        textBlock.SetValue(PreviousColorProperty, initialBrush.Color);
                    }

                    // Subscribe to foreground changes
                    var descriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                        TextBlock.ForegroundProperty,
                        typeof(TextBlock));
                    descriptor?.AddValueChanged(textBlock, OnForegroundChanged);
                }
                else
                {
                    // Unsubscribe from foreground changes
                    var descriptor = System.ComponentModel.DependencyPropertyDescriptor.FromProperty(
                        TextBlock.ForegroundProperty,
                        typeof(TextBlock));
                    descriptor?.RemoveValueChanged(textBlock, OnForegroundChanged);
                }
            }
        }

        private static void OnForegroundChanged(object? sender, EventArgs e)
        {
            if (sender is not TextBlock textBlock) return;
            if (textBlock.Foreground is not SolidColorBrush newBrush) return;

            var previousColor = textBlock.GetValue(PreviousColorProperty) as Color?;
            var newColor = newBrush.Color;

            // If we have a previous color and it's different, animate
            if (previousColor.HasValue && previousColor.Value != newColor)
            {
                // Create a new animatable brush
                var animatedBrush = new SolidColorBrush(previousColor.Value);
                textBlock.Foreground = animatedBrush;

                // Animate to the new color
                var colorAnimation = new ColorAnimation
                {
                    From = previousColor.Value,
                    To = newColor,
                    Duration = TimeSpan.FromMilliseconds(500),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                animatedBrush.BeginAnimation(SolidColorBrush.ColorProperty, colorAnimation);
            }

            // Store the new color as previous for next time
            textBlock.SetValue(PreviousColorProperty, newColor);
        }
    }
}
