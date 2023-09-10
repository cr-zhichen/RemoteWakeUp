using System.Reflection;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using RemoteWakeUp.Filter;
using RemoteWakeUp.Jwt;
using RemoteWakeUp.WS;

// 获取当前运行的程序集的位置目录
var baseDirectory = Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location);

// 使用WebApplicationOptions创建一个新的Web应用构造器，并读取配置文件
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    // 设置内容根路径为当前程序集的位置
    Args = args,
    ContentRootPath = baseDirectory,
});

// 允许所有域名访问
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        corsPolicyBuilder => corsPolicyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

// 向服务集合添加MVC控制器服务
builder.Services.AddControllers();
// 向服务集合添加API终端点的API资源浏览服务
builder.Services.AddEndpointsApiExplorer();
// 向服务集合添加Swagger生成服务
builder.Services.AddSwaggerGen();

//注入服务
builder.Services.AddHttpContextAccessor();

#region 全局异常处理

//全局异常处理
builder.Services.AddControllers(options =>
{
    options.Filters.Add<CustomerExceptionFilter>();
    //添加过滤器
    options.Filters.Add(typeof(ModelValidateActionFilterAttribute));
});

//关闭默认模型验证
builder.Services.Configure<ApiBehaviorOptions>(opt => opt.SuppressModelStateInvalidFilter = true);

#endregion

#region 配置JWT

var section = builder.Configuration.GetSection("TokenOptions"); // 获取TokenOptions配置
var tokenOptions = section.Get<TokenOptions>();

builder.Services.AddTransient<IJwtService, JwtService>(); // 注册Jwt服务到容器
builder.Services.Configure<TokenOptions>(section); // 注入IOptions需要这个
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true, //是否在令牌期间验证签发者
            ValidateAudience = true, //是否验证接收者
            ValidateLifetime = true, //是否验证失效时间
            ValidateIssuerSigningKey = true, //是否验证签名
            ValidAudience = tokenOptions.Audience, //接收者
            ValidIssuer = tokenOptions.Issuer, //签发者，签发的Token的人
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenOptions.SecretKey))
        };
    });

#endregion

#region 配置swagger

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1",
        new() { Title = "RemoteWakeUp", Version = "v1", Description = "" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "使用 Bearer 方案的 JWT 授权标头。",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Scheme = "bearer",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            new List<string>()
        }
    });
});

//使用完整的类型名称作为架构 ID
builder.Services.AddSwaggerGen(c => { c.CustomSchemaIds(x => x.FullName); });

#endregion

builder.Services.AddTransient<WebSocketController>();

// 构建Web应用程序
var app = builder.Build();

// 如果当前是开发环境
if (app.Environment.IsDevelopment())
{
    // 启用Swagger中间件以为JSON终端点生成Swagger文档
    app.UseSwagger();

    // 启用Swagger UI
    app.UseSwaggerUI();
}

// 强制HTTP请求重定向到HTTPS
// app.UseHttpsRedirection();

//JWT认证
app.UseAuthentication();
app.UseAuthorization();

#region WebSocket中间件

// 设置2分钟的心跳检测
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(2)
};

app.UseWebSockets(webSocketOptions);
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws")
    {
        if (context.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            var webSocketController = context.RequestServices.GetService<WebSocketController>();
            await webSocketController?.HandleWebSocketAsync(context, webSocket)!;
        }
        else
        {
            context.Response.StatusCode = 400;
        }
    }
    else
    {
        await next();
    }
});

#endregion


// 将控制器路由映射到MVC中间件
app.MapControllers();

// 运行应用
app.Run();