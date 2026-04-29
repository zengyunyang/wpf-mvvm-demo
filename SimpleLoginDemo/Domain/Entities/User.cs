using CommunityToolkit.Mvvm.ComponentModel;

namespace SimpleLoginDemo.Domain.Entities;

/// <summary>
/// 用户模型。
/// Model 用来表示业务数据本身。
///
/// 这里继承 ObservableObject，
/// 表示该对象的属性变化后可以自动通知界面刷新。
/// </summary>
public partial class User : ObservableObject
{
    /// <summary>
    /// 用户编号。
    /// ObservableProperty 会自动生成公开属性 Id。
    /// </summary>
    [ObservableProperty]
    private int id;

    /// <summary>
    /// 用户名。
    /// </summary>
    [ObservableProperty]
    private string userName = string.Empty;

    /// <summary>
    /// 密码。
    /// 教学示例中直接明文展示，真实项目中不能这么做。
    /// </summary>
    [ObservableProperty]
    private string password = string.Empty;
    
    /// <summary>
    /// 用户角色。
    /// </summary>
    [ObservableProperty]
    private string role = string.Empty;
}
