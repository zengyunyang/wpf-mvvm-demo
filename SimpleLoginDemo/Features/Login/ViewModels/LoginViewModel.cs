using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLoginDemo.Domain.Entities;
using SimpleLoginDemo.Infrastructure.Stores;

namespace SimpleLoginDemo.Features.Login.ViewModels;

/// <summary>
/// 登录页面的 ViewModel。
/// 这里重点演示：
/// 1. 用 ObservableProperty 生成属性
/// 2. 用 RelayCommand 生成命令
/// 3. 用 NotifyCanExecuteChangedFor 自动刷新按钮可用状态
/// </summary>
public partial class LoginViewModel : ObservableObject
{
    private readonly UserStore _userStore;

    public LoginViewModel(UserStore userStore)
    {
        _userStore = userStore;
    }

    /// <summary>
    /// 登录成功时通知外部。
    /// ViewModel 不直接创建窗口，这样职责更清晰。
    /// </summary>
    public event EventHandler<User>? LoginSucceeded;

    /// <summary>
    /// 用户名输入框绑定到这个属性。
    /// 当它变化时，Toolkit 会自动通知 LoginCommand 重新判断能否执行。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string userName = string.Empty;

    /// <summary>
    /// 密码输入框绑定到这个属性。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(LoginCommand))]
    private string password = string.Empty;

    /// <summary>
    /// 页面底部的提示信息。
    /// </summary>
    [ObservableProperty]
    private string message = "默认账号：admin，密码：123456";

    /// <summary>
    /// 这是命令的可执行条件。
    /// 只要用户名或密码为空，就不允许点击登录按钮。
    /// </summary>
    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(UserName) &&
               !string.IsNullOrWhiteSpace(Password);
    }

    /// <summary>
    /// RelayCommand 特性会自动生成 LoginCommand。
    /// XAML 中可以直接绑定 {Binding LoginCommand}。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanLogin))]
    private void Login()
    {
        var user = _userStore.ValidateLogin(UserName.Trim(), Password);

        if (user is null)
        {
            Message = "用户名或密码错误，请重新输入。";
            return;
        }

        Message = "登录成功，正在进入管理窗口...";
        LoginSucceeded?.Invoke(this, user);
    }
}
