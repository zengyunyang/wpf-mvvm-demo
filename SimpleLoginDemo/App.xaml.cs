using System.Windows;
using SimpleLoginDemo.Domain.Entities;
using SimpleLoginDemo.Features.Login.ViewModels;
using SimpleLoginDemo.Features.Login.Views;
using SimpleLoginDemo.Features.Management.ViewModels;
using SimpleLoginDemo.Features.Management.Views;
using SimpleLoginDemo.Infrastructure.Stores;

namespace SimpleLoginDemo;

/// <summary>
/// 应用程序入口。
/// 这里负责：
/// 1. 创建共享数据
/// 2. 创建首个窗口
/// 3. 组织窗口之间的简单跳转
/// 4. 充当轻量级的应用装配入口
/// </summary>
public partial class App : Application
{
    /// <summary>
    /// 整个程序共用一份用户数据。
    /// 这样登录页和管理页看到的是同一组用户。
    /// </summary>
    private readonly UserStore _userStore = new();

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        ShowLoginWindow();
    }

    /// <summary>
    /// 显示登录窗口。
    /// 这里单独提炼成方法，是为了让初学者更清楚“启动流程”和“窗口装配”的职责。
    /// </summary>
    private void ShowLoginWindow()
    {
        var loginViewModel = new LoginViewModel(_userStore);
        var loginWindow = new LoginWindow
        {
            DataContext = loginViewModel
        };

        loginViewModel.LoginSucceeded += (_, user) =>
        {
            var managementViewModel = new ManagementViewModel(_userStore, user);
            var managementWindow = new ManagementWindow
            {
                DataContext = managementViewModel
            };

            managementViewModel.LogoutRequested += (_, _) =>
            {
                ShowLoginWindow();
                managementWindow.Close();
            };

            managementWindow.Show();
            loginWindow.Close();
        };

        loginWindow.Show();
    }
}
