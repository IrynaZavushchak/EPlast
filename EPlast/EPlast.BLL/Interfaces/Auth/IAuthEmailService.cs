﻿using EPlast.BLL.DTO.Account;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;

namespace EPlast.BLL.Interfaces
{
    public interface IAuthEmailService
    {
        /// <summary>
        /// Confirming Email after registration
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="code"></param>
        /// <returns>Result of confirming email in system</returns>
        Task<IdentityResult> ConfirmEmailAsync(string userId, string code);

        /// <summary>
        /// Sending email reminder
        /// </summary>
        /// <param name="citiesUrl"></param>
        /// <param name="userDTO"></param>
        /// <returns>Result of sending email</returns>
        Task<bool> SendEmailGreetingAsync(string email);

        /// <summary>
        /// Sending email reminder to join city
        /// </summary>
        /// <param name="email"></param>
        /// <returns>Result of sending email</returns>
        Task<bool> SendEmailJoinToCityReminderAsync(string email);

        /// <summary>
        /// Sending email for registration
        /// </summary>
        /// <param name="confirmationLink"></param>
        /// <param name="userDto"></param>
        /// <returns>Result of sending email</returns>
        Task<bool> SendEmailRegistrAsync(string email);

        /// <summary>
        /// Sending email for password reset
        /// </summary>
        /// <param name="confirmationLink"></param>
        /// <param name="forgotPasswordDto"></param>
        /// <returns>Result of sending email</returns>
        Task SendEmailResetingAsync(string confirmationLink, ForgotPasswordDto forgotPasswordDto);
    }
}
