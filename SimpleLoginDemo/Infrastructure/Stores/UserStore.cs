using System.Collections.ObjectModel;
using SimpleLoginDemo.Domain.Entities;

namespace SimpleLoginDemo.Infrastructure.Stores;

/// <summary>
/// 简单的内存数据仓库。
/// 为了聚焦 WPF 和 MVVM 学习，这里不接数据库。
/// </summary>
public class UserStore
{
    public UserStore()
    {
        Users = new ObservableCollection<User>
        {
            new() { Id = 1, UserName = "admin", Password = "123456", Role = "Administrator" },
            new() { Id = 2, UserName = "tom", Password = "111111", Role = "User" },
            new() { Id = 3, UserName = "jerry", Password = "222222", Role = "User" }
        };
    }

    /// <summary>
    /// ObservableCollection 在新增或删除元素时会通知界面刷新。
    /// 因此它非常适合作为列表数据源。
    /// </summary>
    public ObservableCollection<User> Users { get; }

    /// <summary>
    /// 校验用户名和密码是否正确。
    /// </summary>
    public User? ValidateLogin(string userName, string password)
    {
        return Users.FirstOrDefault(user =>
            user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
            user.Password == password);
    }

    /// <summary>
    /// 判断用户名是否已存在。
    /// ignoreId 用于“修改用户”场景，避免把自己也算成重复。
    /// </summary>
    public bool ExistsUserName(string userName, int? ignoreId = null)
    {
        return Users.Any(user =>
            user.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase) &&
            (!ignoreId.HasValue || user.Id != ignoreId.Value));
    }

    /// <summary>
    /// 判断 Id 是否已存在。
    /// </summary>
    public bool ExistsId(int id, int? ignoreId = null)
    {
        return Users.Any(user =>
            user.Id == id &&
            (!ignoreId.HasValue || user.Id != ignoreId.Value));
    }
}
