using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLoginDemo.Domain.Entities;
using SimpleLoginDemo.Infrastructure.Stores;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;

namespace SimpleLoginDemo.Features.Management.ViewModels;

public partial class ManagementEditFieldViewModel : ObservableObject
{
    public ManagementEditFieldViewModel(string label, string value = "")
    {
        Label = label;
        this.value = value;
    }

    public string Label { get; }

    [ObservableProperty]
    private string value = string.Empty;
}

/// <summary>
/// 用户管理页面的 ViewModel。
/// 用来演示最基础的增删改查以及表格选中联动编辑区。
/// </summary>
public partial class ManagementViewModel : ObservableObject
{
    private readonly UserStore _userStore;
    private readonly ManagementEditFieldViewModel _editIdField;
    private readonly ManagementEditFieldViewModel _editUserNameField;
    private readonly ManagementEditFieldViewModel _editPasswordField;
    private readonly ManagementEditFieldViewModel _editRoleField;

    public ManagementViewModel(UserStore userStore, User currentUser)
    {
        _userStore = userStore;
        CurrentUserName = currentUser.UserName;
        Users = userStore.Users;
        UsersView = CollectionViewSource.GetDefaultView(Users);
        UsersView.Filter = FilterUser;

        _editIdField = new ManagementEditFieldViewModel("Id");
        _editUserNameField = new ManagementEditFieldViewModel("用户名");
        _editPasswordField = new ManagementEditFieldViewModel("密码");
        _editRoleField = new ManagementEditFieldViewModel("角色");
        EditFields =
        [
            _editIdField,
            _editUserNameField,
            _editPasswordField,
            _editRoleField
        ];

        foreach (var field in EditFields)
        {
            field.PropertyChanged += OnEditFieldPropertyChanged;
        }

        if (Users.Count > 0)
        {
            SelectedUser = Users[0];
        }
    }

    /// <summary>
    /// 搜索关键字。
    /// 这里配合 ICollectionView 做标准过滤，不直接修改底层 Users 集合。
    /// </summary>
    [ObservableProperty]
    private string searchText = string.Empty;

    /// <summary>
    /// 当前登录用户名，仅用于顶部欢迎语。
    /// </summary>
    public string CurrentUserName { get; }

    /// <summary>
    /// 提供给 DataGrid 的数据源。
    /// </summary>
    public ObservableCollection<User> Users { get; }

    /// <summary>
    /// 提供给 DataGrid 的展示视图。
    /// 搜索时只过滤这个视图，不改动底层 Users 数据。
    /// </summary>
    public ICollectionView UsersView { get; }

    /// <summary>
    /// 提供给编辑区的表单项集合。
    /// 让界面通过 ItemsControl 渲染，减少重复 XAML。
    /// </summary>
    public IReadOnlyList<ManagementEditFieldViewModel> EditFields { get; }

    /// <summary>
    /// 当前选中的用户。
    /// 当它变化时，需要刷新“修改/删除”按钮状态。
    /// </summary>
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(UpdateUserCommand))]
    [NotifyCanExecuteChangedFor(nameof(DeleteUserCommand))]
    private User? selectedUser;

    /// <summary>
    /// 编辑区中的 Id 文本。
    /// TextBox 输入本质是字符串，所以先用 string 接收，再在保存时解析为 int。
    /// </summary>
    public string EditIdText
    {
        get => _editIdField.Value;
        set => _editIdField.Value = value;
    }

    /// <summary>
    /// 编辑区中的用户名。
    /// </summary>
    public string EditUserName
    {
        get => _editUserNameField.Value;
        set => _editUserNameField.Value = value;
    }

    /// <summary>
    /// 编辑区中的密码。
    /// </summary>
    public string EditPassword
    {
        get => _editPasswordField.Value;
        set => _editPasswordField.Value = value;
    }

    public string EditRole
    {
        get => _editRoleField.Value;
        set => _editRoleField.Value = value;
    }

    /// <summary>
    /// 页面底部提示消息。
    /// </summary>
    [ObservableProperty]
    private string message = "请选择一条用户记录，或输入新用户信息。";

    /// <summary>
    /// Toolkit 会为 ObservableProperty 生成局部扩展点。
    /// 当 SelectedUser 变化时，这个方法会被自动调用。
    /// </summary>
    partial void OnSelectedUserChanged(User? value)
    {
        if (value is null)
        {
            return;
        }

        EditIdText = value.Id.ToString();
        EditUserName = value.UserName;
        EditPassword = value.Password;
        EditRole = value.Role;
    }

    partial void OnSearchTextChanged(string value)
    {
        ApplySearch();
    }

    private void OnEditFieldPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ManagementEditFieldViewModel.Value))
        {
            return;
        }

        AddUserCommand.NotifyCanExecuteChanged();
        UpdateUserCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// 新增和修改共用的一套输入校验。
    /// </summary>
    private bool CanEditUser()
    {
        return TryGetEditId(out var id) &&
               id > 0 &&
               !string.IsNullOrWhiteSpace(EditUserName) &&
               !string.IsNullOrWhiteSpace(EditPassword) &&
               !string.IsNullOrWhiteSpace(EditRole);
    }

    /// <summary>
    /// 修改用户前，除了输入完整，还必须先选中一条记录。
    /// </summary>
    private bool CanUpdateUser()
    {
        return SelectedUser is not null && CanEditUser();
    }

    /// <summary>
    /// 删除用户前，必须先选中一条记录。
    /// </summary>
    private bool CanDeleteUser()
    {
        return SelectedUser is not null;
    }

    /// <summary>
    /// 新增用户命令。
    /// RelayCommand 会生成 AddUserCommand。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEditUser))]
    private void AddUser()
    {
        if (!TryGetEditId(out var editId))
        {
            Message = "新增失败：Id 必须是正整数。";
            return;
        }

        var trimmedUserName = EditUserName.Trim();

        if (_userStore.ExistsId(editId))
        {
            Message = "新增失败：该 Id 已存在。";
            return;
        }

        if (_userStore.ExistsUserName(trimmedUserName))
        {
            Message = "新增失败：该用户名已存在。";
            return;
        }

        var user = new User
        {
            Id = editId,
            UserName = trimmedUserName,
            Password = EditPassword,
            Role = EditRole
        };

        Users.Add(user);
        UsersView.Refresh();
        SelectedUser = user;
        Message = "新增用户成功。";
    }

    /// <summary>
    /// 修改当前选中用户。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanUpdateUser))]
    private void UpdateUser()
    {
        if (SelectedUser is null)
        {
            return;
        }

        if (!TryGetEditId(out var editId))
        {
            Message = "修改失败：Id 必须是正整数。";
            return;
        }

        var trimmedUserName = EditUserName.Trim();

        if (_userStore.ExistsId(editId, SelectedUser.Id))
        {
            Message = "修改失败：新的 Id 与其他用户重复。";
            return;
        }

        if (_userStore.ExistsUserName(trimmedUserName, SelectedUser.Id))
        {
            Message = "修改失败：新的用户名与其他用户重复。";
            return;
        }

        SelectedUser.Id = editId;
        SelectedUser.UserName = trimmedUserName;
        SelectedUser.Password = EditPassword;
        SelectedUser.Role = EditRole;

        UsersView.Refresh();
        Message = "修改用户成功。";
    }

    /// <summary>
    /// 删除当前选中的用户。
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDeleteUser))]
    private void DeleteUser()
    {
        if (SelectedUser is null)
        {
            return;
        }

        var userToDelete = SelectedUser;
        Users.Remove(userToDelete);
        UsersView.Refresh();
        SelectedUser = Users.FirstOrDefault();
        Message = $"已删除用户：{userToDelete.UserName}";
    }

    /// <summary>
    /// 尝试把编辑区的 Id 文本解析为正整数。
    /// </summary>
    private bool TryGetEditId(out int id)
    {
        return int.TryParse(EditIdText, out id) && id > 0;
    }

    public event EventHandler? LogoutRequested;

    [RelayCommand]
    private void Logout()
    {
        Message = "已注销。";
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// 手动触发一次搜索。
    /// 实际过滤逻辑仍然由 ICollectionView 承担。
    /// </summary>
    [RelayCommand]
    private void Search()
    {
        ApplySearch();
    }

    /// <summary>
    /// 清空搜索关键字并恢复完整列表。
    /// </summary>
    [RelayCommand]
    private void ClearSearch()
    {
        SearchText = string.Empty;
        Message = "已清空搜索条件。";
    }

    private bool FilterUser(object item)
    {
        if (item is not User user)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            return true;
        }

        var keyword = SearchText.Trim();

        return user.UserName.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
               user.Role.Contains(keyword, StringComparison.OrdinalIgnoreCase) ||
               user.Id.ToString().Contains(keyword, StringComparison.OrdinalIgnoreCase);
    }

    private void ApplySearch()
    {
        UsersView.Refresh();

        if (SelectedUser is not null && !UsersView.Cast<User>().Contains(SelectedUser))
        {
            SelectedUser = UsersView.Cast<User>().FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(SearchText))
        {
            Message = "已显示全部用户。";
            return;
        }

        var resultCount = UsersView.Cast<User>().Count();
        Message = $"搜索完成，共匹配到 {resultCount} 条用户记录。";
    }
}
