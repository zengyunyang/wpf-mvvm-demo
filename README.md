# CommunityToolkit.Mvvm 教学版 WPF 示例

这个项目在 `/Users/wikiglobal/project-study/mvvm-demo` 中重新实现了 `wpf-demo` 里的“登录 + 用户管理”示例，**不修改原项目**，专门面向 WPF 初学者。

## 项目目标

- 用尽量少的概念，搭建一个完整可运行的 MVVM 示例
- 使用 `CommunityToolkit.Mvvm` 简化样板代码
- 保留充足注释，帮助理解 WPF、MVVM、数据绑定和命令
- 让你能把这个项目当成自己的入门模板继续扩展

## 项目结构

```text
mvvm-demo
├── README.md
└── SimpleLoginDemo
    ├── App.xaml
    ├── App.xaml.cs
    ├── Domain
    │   └── Entities
    │       └── User.cs
    ├── Features
    │   ├── Login
    │   │   ├── ViewModels
    │   │   │   └── LoginViewModel.cs
    │   │   └── Views
    │   │       ├── LoginWindow.xaml
    │   │       └── LoginWindow.xaml.cs
    │   └── Management
    │       ├── ViewModels
    │       │   └── ManagementViewModel.cs
    │       └── Views
    │           ├── ManagementWindow.xaml
    │           └── ManagementWindow.xaml.cs
    ├── Infrastructure
    │   └── Stores
    │       └── UserStore.cs
    ├── Shared
    │   └── Controls
    │       ├── BindablePasswordBox.xaml
    │       └── BindablePasswordBox.xaml.cs
    └── SimpleLoginDemo.csproj
```

这样分层后，每个目录的职责会更清楚：

- `Domain`：放业务实体，尽量不掺杂界面细节
- `Features`：按页面/功能组织 View 和 ViewModel，后续新增模块更自然
- `Infrastructure`：放数据来源、仓储、外部依赖的接入代码
- `Shared`：放跨功能复用的控件、样式、转换器等

## 先理解 4 个核心概念

### 1. View

View 就是界面，通常写在 XAML 中。  
比如窗口、按钮、文本框、表格都属于 View。

在这个项目里：

- `Features/Login/Views/LoginWindow.xaml` 是登录界面
- `Features/Management/Views/ManagementWindow.xaml` 是用户管理界面

### 2. ViewModel

ViewModel 是“界面的数据和行为”。  
它不负责画界面，但负责：

- 给界面提供数据
- 处理按钮点击这类操作
- 在数据变化时通知界面刷新

在这个项目里：

- `LoginViewModel` 负责登录页逻辑
- `ManagementViewModel` 负责用户管理页逻辑

### 3. Model

Model 是业务数据本身。

在这个项目里：

- `User` 表示一个用户
- `UserStore` 表示一个简单的用户数据仓库

### 4. Binding（数据绑定）

WPF 最强大的地方之一就是绑定。

例如：

```xml
<TextBox Text="{Binding UserName, UpdateSourceTrigger=PropertyChanged}" />
```

这句话的意思是：

- `TextBox` 显示 `UserName`
- 用户输入时，值会同步回 `UserName`
- `UpdateSourceTrigger=PropertyChanged` 表示每输入一个字符就立刻同步

## DataContext 是什么

`DataContext` 可以理解成：

> 当前这个界面默认要绑定到哪个对象

例如在 `App.xaml.cs` 中：

```csharp
loginWindow.DataContext = loginViewModel;
```

这样 `LoginWindow.xaml` 里的 `{Binding UserName}` 就会去 `LoginViewModel` 中找 `UserName`。

## WPF 入门重难点

### 1. 为什么界面会自动刷新

因为 ViewModel / Model 实现了“属性变更通知”。

如果属性变化后没有通知界面，界面就不知道该刷新。

在这个项目里，我们没有手写 `INotifyPropertyChanged`，而是交给 `CommunityToolkit.Mvvm` 生成。

### 2. 为什么按钮可以直接绑定方法

因为 WPF 的 `Button` 有 `Command` 属性。

例如：

```xml
<Button Content="登录" Command="{Binding LoginCommand}" />
```

它绑定到的是一个命令对象，而不是普通方法。

`CommunityToolkit.Mvvm` 会帮我们把标记了 `[RelayCommand]` 的方法，自动生成对应命令。

### 3. 为什么列表能自动更新

因为这里用的是 `ObservableCollection<T>`。

它和普通 `List<T>` 的区别是：

- `List<T>` 增删元素后，界面通常不知道
- `ObservableCollection<T>` 增删元素后，会通知界面刷新

所以 WPF 中做列表展示时，经常会优先使用它。

### 4. PasswordBox 为什么这么特殊

`TextBox.Text` 可以直接双向绑定，  
但 `PasswordBox.Password` **不能像普通属性那样直接双向绑定**。

所以这个项目保留了一个简单的 `BindablePasswordBox` 包装控件，专门给初学者演示：

- 什么是依赖属性
- 为什么有些 WPF 控件不能直接按普通方式绑定

这也是 WPF 初学者很常遇到的一个坑。

### 5. DataGrid 选中行为什么会联动编辑区

因为这里有双向绑定：

```xml
SelectedItem="{Binding SelectedUser, Mode=TwoWay}"
```

当你点击表格某一行时：

- `SelectedUser` 会更新
- `SelectedUser` 变化后，ViewModel 再把数据同步到上方编辑区

这就是“界面操作 → ViewModel 状态变化 → 其他界面跟着更新”的典型例子。

## CommunityToolkit.Mvvm 为我们做了什么

这是本项目最重要的学习点之一。

如果不用它，我们通常要自己写很多重复代码，例如：

- `INotifyPropertyChanged`
- `SetProperty`
- `RelayCommand`
- 命令的可执行状态刷新

用了 `CommunityToolkit.Mvvm` 后，很多内容都可以交给源生成器自动完成。

### 1. `ObservableObject`

以前我们常常要自己实现：

- `INotifyPropertyChanged`
- `OnPropertyChanged`
- `SetProperty`

现在只要继承：

```csharp
public partial class LoginViewModel : ObservableObject
```

就已经拥有这些基础能力。

### 2. `[ObservableProperty]`

例如：

```csharp
[ObservableProperty]
private string userName = string.Empty;
```

它会自动帮我们生成：

- `UserName` 属性
- 属性变化通知
- 可选的扩展钩子方法

这样写法更短，也更统一。

### 3. `[RelayCommand]`

例如：

```csharp
[RelayCommand(CanExecute = nameof(CanLogin))]
private void Login()
{
}
```

它会自动帮我们生成：

- `LoginCommand` 属性
- `ICommand` 实现
- 与 `CanLogin` 的关联

也就是说，**我们只需要关心业务逻辑，不用再手写命令类**。

### 4. `NotifyCanExecuteChangedFor`

例如：

```csharp
[NotifyCanExecuteChangedFor(nameof(LoginCommand))]
```

当某个属性变化时，Toolkit 会自动帮我们刷新按钮是否可点击。  
这样就不需要自己手动调用 `RaiseCanExecuteChanged()` 了。

## 这个项目里 Toolkit 没有替我们做什么

这点也非常重要。

`CommunityToolkit.Mvvm` 很强，但它**不是 WPF 全家桶**，它主要解决的是 **MVVM 层的样板代码**。

它不会自动帮你解决：

- XAML 布局设计
- `PasswordBox` 直接双向绑定问题
- 复杂导航框架
- 数据库访问
- 主题样式系统

所以你可以把它理解为：

> “帮我们把 ViewModel 写得更轻、更整洁”的工具库

而不是“包办所有 WPF 问题”的框架。

## 这个教学版做了哪些简化

- 不接数据库，数据保存在内存中
- 登录成功后直接打开管理窗口
- 管理页面只演示最基础的增删改
- 为了便于观察，密码在管理页直接用普通文本框展示
- 为了学习 WPF 基础，仍然保留了可绑定密码框示例

## 建议学习顺序

建议按下面顺序阅读代码：

1. `SimpleLoginDemo/Domain/Entities/User.cs`
2. `SimpleLoginDemo/Infrastructure/Stores/UserStore.cs`
3. `SimpleLoginDemo/Features/Login/ViewModels/LoginViewModel.cs`
4. `SimpleLoginDemo/Features/Management/ViewModels/ManagementViewModel.cs`
5. `SimpleLoginDemo/Features/Login/Views/LoginWindow.xaml`
6. `SimpleLoginDemo/Features/Management/Views/ManagementWindow.xaml`
7. `SimpleLoginDemo/App.xaml.cs`
8. `SimpleLoginDemo/Shared/Controls/BindablePasswordBox.xaml.cs`

## 可以重点观察的几个练习点

### 练习 1：自己新增一个“退出登录”按钮

你可以尝试：

- 在管理页添加按钮
- 在 ViewModel 中新增一个 `[RelayCommand]`
- 触发返回登录窗口

### 练习 2：给用户增加“角色”字段

你可以练习：

- 修改 `User` 模型
- 修改 `UserStore` 初始数据
- 修改 DataGrid 列
- 修改增删改逻辑

### 练习 3：把内存数据换成文件保存

你可以继续扩展：

- 程序启动时读取 JSON
- 新增、修改、删除后写回 JSON

这是从“教学示例”走向“稍微真实一些项目”的很好一步。

## 运行方式

在项目目录执行：

```bash
dotnet build /Users/wikiglobal/project-study/mvvm-demo/SimpleLoginDemo/SimpleLoginDemo.csproj
```

如果你本机是 Windows，也可以直接用 Visual Studio 打开 `SimpleLoginDemo.csproj` 运行。

## 默认测试账号

- 用户名：`admin`
- 密码：`123456`

---

如果你接下来愿意，我们还可以继续把这个教学版再往前走一步，例如：

- 增加“退出登录”
- 增加“注册用户”
- 增加 JSON 持久化
- 增加更规范的目录分层
