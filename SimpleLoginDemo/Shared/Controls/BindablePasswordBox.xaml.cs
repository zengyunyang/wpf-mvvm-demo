using System.Windows;
using System.Windows.Controls;

namespace SimpleLoginDemo.Shared.Controls;

/// <summary>
/// 一个可绑定的密码框包装控件。
/// 主要用于教学演示“依赖属性”和“PasswordBox 绑定限制”。
/// </summary>
public partial class BindablePasswordBox : UserControl
{
    /// <summary>
    /// 用来避免“属性赋值触发事件，事件里又反向赋值”的循环更新问题。
    /// </summary>
    private bool _isUpdating;

    public BindablePasswordBox()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 依赖属性是 WPF 属性系统的重要组成部分。
    /// 只有参与 WPF 属性系统，属性才能更自然地接入绑定、样式、动画等机制。
    /// </summary>
    public static readonly DependencyProperty PasswordProperty =
        DependencyProperty.Register(
            nameof(Password),
            typeof(string),
            typeof(BindablePasswordBox),
            new FrameworkPropertyMetadata(
                string.Empty,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnPasswordPropertyChanged));

    /// <summary>
    /// 对外暴露给 ViewModel 绑定的密码属性。
    /// </summary>
    public string Password
    {
        get => (string)GetValue(PasswordProperty);
        set => SetValue(PasswordProperty, value);
    }

    /// <summary>
    /// 当外部绑定值变化时，同步更新内部真正的 PasswordBox。
    /// </summary>
    private static void OnPasswordPropertyChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
    {
        if (dependencyObject is not BindablePasswordBox control || control._isUpdating)
        {
            return;
        }

        control.InnerPasswordBox.Password = e.NewValue?.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 当用户在界面上输入密码时，把值同步回依赖属性，
    /// 这样 ViewModel 才能收到最新密码。
    /// </summary>
    private void InnerPasswordBox_OnPasswordChanged(object sender, RoutedEventArgs e)
    {
        _isUpdating = true;
        Password = InnerPasswordBox.Password;
        _isUpdating = false;
    }
}
