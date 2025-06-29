﻿using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Security.Claims;
using Blazored.LocalStorage;
using System.Threading.Tasks;
using AuthWithAdmin.Shared.AuthSharedModels;
using AuthWithAdmin.Client.ClientHelpers;

namespace AuthWithAdmin.Client
{
    public class AuthStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;
        private readonly AuthenticationState _anonymous;

        public AuthStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
            _anonymous = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        }

        //בדיקה האם יש משתמש מחובר
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");


            if (string.IsNullOrWhiteSpace(token))
            {
                return _anonymous;
            }

            if (IsTokenExpired(token))
            {
                //ניסיון יצירת טוקן חדש - אם עומד לפוג
                var refreshResponse = await _httpClient.GetAsync("api/users/refresh");
                if (refreshResponse.IsSuccessStatusCode)
                {
                    var newToken = await refreshResponse.Content.ReadAsStringAsync();

                    await _localStorage.SetItemAsync("authToken", newToken);
                    token = newToken;
                }
                else
                {
                    return _anonymous; // אם לא הצליח לרענן - פג התוקף
                }
            }
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType")));
        }

        //הודעה שהשתנתה ההתחברות
        public async Task NotifyAuthenticationStateChanged(string token)
        {
            await _localStorage.SetItemAsync("authToken", token);

            var authState = Task.FromResult(new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(JwtParser.ParseClaimsFromJwt(token), "jwtAuthType"))));
            NotifyAuthenticationStateChanged(authState);
        }


     

        //התנתקות
        public async Task NotifyUserLogout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            _httpClient.DefaultRequestHeaders.Authorization = null;

            var authState = Task.FromResult(_anonymous);
            NotifyAuthenticationStateChanged(authState);
        }

        //האם הטוקן פג תוקף
        private bool IsTokenExpired(string token)
        {

            var claims = JwtParser.ParseClaimsFromJwt(token);
            if (claims == null || claims.Count() == 0)
                return true;
            var expiry = claims.FirstOrDefault(c => c.Type == "exp")?.Value;

            if (expiry != null && long.TryParse(expiry, out long exp))
            {
                var expDate = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                // האם עומד לפוג בקרוב
                return expDate < DateTime.UtcNow.AddMinutes(10);
            }

            return true; // If there's no expiration claim, consider the token expired
        }

    }
}
