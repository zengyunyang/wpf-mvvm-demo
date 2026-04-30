using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json;
using SimpleLoginDemo.Domain.Entities;

namespace SimpleLoginDemo.Infrastructure.Stores;

/// <summary>
/// 使用本地 JSON 文件保存用户数据的简单仓储。
/// 启动时加载文件，增删改后立即写回文件。
/// </summary>
public class UserStore
{
    /// <summary>
    /// JSON 序列化配置。
    /// WriteIndented = true 表示写入文件时自动格式化，方便直接打开 user.json 学习和查看。
    /// </summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// 用户数据文件的完整路径。
    /// 这里使用 AppContext.BaseDirectory，是为了从程序运行目录中定位复制过去的 user.json。
    /// </summary>
    private readonly string _filePath;

    /// <summary>
    /// 创建仓储时立即加载 JSON 文件。
    /// 这样应用启动后，登录页和管理页拿到的就是文件中的最新数据。
    /// </summary>
    public UserStore()
    {
        _filePath = Path.Combine(AppContext.BaseDirectory, "Infrastructure", "Stores", "user.json");
        Users = new ObservableCollection<User>(LoadUsers());
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

    /// <summary>
    /// 新增一个用户，并在内存集合更新后立刻写回 JSON 文件。
    /// </summary>
    public void AddUser(User user)
    {
        Users.Add(user);
        SaveUsers();
    }

    /// <summary>
    /// 修改指定用户的字段，然后把最新结果整体写回 JSON 文件。
    /// 这里直接修改传入的 user，是因为它本身已经是 ObservableCollection 中的对象。
    /// </summary>
    public void UpdateUser(User user, int newId, string newUserName, string newPassword, string newRole)
    {
        user.Id = newId;
        user.UserName = newUserName;
        user.Password = newPassword;
        user.Role = newRole;
        SaveUsers();
    }

    /// <summary>
    /// 从内存集合中删除一个用户，并同步写回 JSON 文件。
    /// </summary>
    public void DeleteUser(User user)
    {
        Users.Remove(user);
        SaveUsers();
    }

    /// <summary>
    /// 读取 JSON 文件中的用户列表。
    ///
    /// 流程分 3 步：
    /// 1. 先确保目录和文件存在，避免首次启动时直接读取失败。
    /// 2. 用 File.ReadAllText 把整个 JSON 文件读成字符串。
    /// 3. 用 JsonSerializer.Deserialize 把 JSON 字符串反序列化成 List&lt;User&gt;。
    ///
    /// 如果反序列化结果为 null，就返回一个空列表，避免调用方出现空引用问题。
    /// </summary>
    private IReadOnlyList<User> LoadUsers()
    {
        EnsureDataFileExists();

        var json = File.ReadAllText(_filePath);
        var users = JsonSerializer.Deserialize<List<User>>(json, JsonOptions);

        return users ?? [];
    }

    /// <summary>
    /// 把当前内存中的 Users 集合完整写回 JSON 文件。
    /// 这里采用“整体覆盖写入”的方式，代码最直观，也很适合教学示例。
    /// </summary>
    private void SaveUsers()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);

        var json = JsonSerializer.Serialize(Users, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    /// <summary>
    /// 确保 user.json 文件存在。
    ///
    /// 如果目录不存在，先创建目录；
    /// 如果文件不存在，说明是第一次运行程序，就写入一份默认种子数据。
    /// 这样后续 LoadUsers 再读取文件时一定有内容可读。
    /// </summary>
    private void EnsureDataFileExists()
    {
        var directory = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(directory);

        if (File.Exists(_filePath))
        {
            return;
        }

        var seedUsers = new List<User>
        {
            new() { Id = 1, UserName = "admin", Password = "123456", Role = "Administrator" },
            new() { Id = 2, UserName = "tom", Password = "111111", Role = "User" },
            new() { Id = 3, UserName = "jerry", Password = "222222", Role = "User" }
        };

        var json = JsonSerializer.Serialize(seedUsers, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
