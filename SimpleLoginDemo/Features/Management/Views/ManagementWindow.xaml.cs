using System.Windows;

namespace SimpleLoginDemo.Features.Management.Views;

/// <summary>
/// 用户管理窗口视图。
/// 与登录窗口一样，这里只负责承载界面，不承担业务逻辑。
/// </summary>
public partial class ManagementWindow : Window
{
    public ManagementWindow()
    {
        InitializeComponent();
    }
}
