using AuthWithAdmin.Server.AuthHelpers;
using AuthWithAdmin.Server.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.Google;
using System.Text;
using AuthWithAdmin.Server.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

//DB
builder.Services.AddScoped<DbRepository>();

//Chat Service
builder.Services.AddScoped<IChatService, ChatService>();
//User management
builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<AuthCheck>();
builder.Services.AddScoped<AuthRepository>();
builder.Services.AddSingleton<ITokenBlacklistService, InMemoryTokenBlacklistService>();
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<PasswordService>();

//Files
builder.Services.AddScoped<FilesManage>();
    
//Mail
builder.Services.AddSingleton<EmailHelper>();


//JWT
var jwtSettings = builder.Configuration.GetSection("JWTSettings");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme; // Only set one challenge scheme!
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // Required for Google sign-in
})
.AddCookie() // Required for external authentication like Google
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.GetValue<string>("validIssuer"),
        ValidAudience = jwtSettings.GetValue<string>("validIssuer"),
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.GetValue<string>("securityKey")))
    };
})
.AddGoogle(options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
});


builder.Services.AddAuthorization(); // Add Authorization services
builder.Services.AddScoped<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".data"] = "applocation/json";
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

app.UseRouting();

//user management
app.UseMiddleware<TokenBlacklistMiddleware>();
app.UseAuthentication();
app.UseAuthorization();


app.MapRazorPages();
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();