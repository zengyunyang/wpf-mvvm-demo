using System.Windows;

namespace SimpleLoginDemo.Features.Login.Views;

/// <summary>
/// 登录窗口视图。
/// 这里几乎不放业务逻辑，只负责承载 XAML。
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }
}
