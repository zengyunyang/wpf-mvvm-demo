using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLoginDemo.Domain.Entities;
using SimpleLoginDemo.Infrastructure.Stores;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace SimpleLoginDemo.Features.ChangePassword.ViewModels
{
    internal partial class ChangePasswordViewModel: ObservableObject
    {
        private readonly UserStore _userStore;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
        private string newPassWord = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
        private string passWord = string.Empty;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ChangePasswordCommand))]
        private string userName = string.Empty;

        public User CurrentUser { get; }

        public ChangePasswordViewModel(UserStore userStore, User user)
        {
            _userStore = userStore;
            CurrentUser = user;
            UserName = CurrentUser.UserName; // 假设当前用户是第一个用户
        }

        public bool CanChangePassword()
        {
            return !string.IsNullOrWhiteSpace(NewPassWord) && !string.IsNullOrWhiteSpace(PassWord) && !string.IsNullOrWhiteSpace(UserName);
        }

        public event EventHandler? ChangePasswordSucceededRequested;
        [RelayCommand(CanExecute = nameof(CanChangePassword))]
        private void ChangePassword()
        {
            // 在这里实现修改密码的逻辑
            // 例如，验证新密码的有效性，然后更新用户的密码
            if (!string.IsNullOrWhiteSpace(NewPassWord))
            {
                // 假设当前用户是第一个用户
                if (CurrentUser != null && CurrentUser.UserName == UserName && CurrentUser.Password == PassWord)
                {
                    CurrentUser.Password = NewPassWord;
                    _userStore.UpdateUser(CurrentUser, CurrentUser.Id, CurrentUser.UserName, CurrentUser.Password, CurrentUser.Role);
                    // 保存更改到存储中
                    ChangePasswordSucceededRequested?.Invoke(this, EventArgs.Empty);
                } else
                {
                    MessageBox.Show("用户名或密码错误！！");
                    // 清空新密码输入框
                    NewPassWord = string.Empty;
                }
            }
        }
    }
}
