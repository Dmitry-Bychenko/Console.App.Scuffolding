using Polly.Extensions.Http;
using Polly;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace ConsoleScuffolding;

/// <summary>
/// Application Core
/// </summary>
public static class Core {
  #region Private Data

  static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy() {
    return HttpPolicyExtensions
      .HandleTransientHttpError()
      .OrResult(msg => msg.StatusCode == HttpStatusCode.NotFound || msg.StatusCode == HttpStatusCode.TooManyRequests)
      .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt) + Random.Shared.NextDouble() * 0.2));
  }

  private static void BuildHttpClient(IServiceCollection services) {
    services
      .AddHttpClient("STANDARD", client => {
        client.DefaultRequestHeaders.Add("User-Agent", "AssemblyName/version");
        client.DefaultRequestHeaders.Add("Accept", "application/json");
      })
      .ConfigurePrimaryHttpMessageHandler(() => {
        HttpClientHandler handler = new HttpClientHandler();

        handler.CookieContainer = new CookieContainer();

        return handler;
      })
      .SetHandlerLifetime(TimeSpan.FromMinutes(5))  //Set lifetime to five minutes
      .AddPolicyHandler(GetRetryPolicy());
  }

  private static readonly Lazy<IHost> LazyHost = new (() => {
    var builder = Host
      .CreateDefaultBuilder(Environment.GetCommandLineArgs())
      .ConfigureHostConfiguration(config => {
        config.SetBasePath(Directory.GetCurrentDirectory());
        config.AddJsonFile($"{Path.GetFileNameWithoutExtension(Assembly.GetEntryAssembly()!.Location)}.json", optional: true);
        config.AddCommandLine(Environment.GetCommandLineArgs());

        StartUp.Configure(config);

        var root = config.Build();

        root.Bind(new Settings());
      })
      .ConfigureServices((context, services) => {
        var root = context.Configuration;

        services.Configure<Settings>(root);

        services.AddLogging();

        BuildHttpClient(services);

        StartUp.Configure(services);
      });

    return builder.Build();
  });

  private static readonly Lazy<Settings> LazySettings = new (() => {
    var options = ApplicationHost
      .Services
      .GetService<IOptions<Settings>>();

    return options is null
      ? new Settings()
      : options.Value;
  });

  #endregion Private Data

  #region Public

  /// <summary>
  /// Host
  /// </summary>
  public static IHost ApplicationHost => LazyHost.Value;

  /// <summary>
  /// Settings
  /// </summary>
  public static Settings ApplicationSettings => LazySettings.Value;

  /// <summary>
  /// Logger
  /// </summary>
  public static ILogger<T> Logger<T>() => ApplicationHost
    .Services
    .GetService<ILogger<T>>() ?? NullLogger<T>.Instance;

  /// <summary>
  ///     Http Client
  /// </summary>
  public static HttpClient Client(string? name = default) {
    var factory = ApplicationHost.Services.GetService<IHttpClientFactory>();

    return factory!.CreateClient(name?.Trim().ToUpper() ?? "STANDARD");
  }

  #endregion Public
}

