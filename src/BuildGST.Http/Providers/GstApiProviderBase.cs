using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using BuildGST.Abstractions.Exceptions;
using BuildGST.Abstractions.Interfaces;
using BuildGST.Abstractions.Models;

namespace BuildGST.Http.Providers;

/// <summary>
/// Provides a reusable HTTP-based foundation for GST API providers.
/// </summary>
public abstract class GstApiProviderBase : IGstApiProvider
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GstApiProviderBase"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client used to call the provider endpoint.</param>
    /// <param name="retryOptions">The retry policy configuration.</param>
    protected GstApiProviderBase(HttpClient httpClient, GstApiRetryOptions? retryOptions = null)
    {
        HttpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        RetryOptions = retryOptions ?? new GstApiRetryOptions();
    }

    /// <inheritdoc />
    public abstract string Name { get; }

    /// <summary>
    /// Gets the HTTP client used by the provider.
    /// </summary>
    protected HttpClient HttpClient { get; }

    /// <summary>
    /// Gets the retry options.
    /// </summary>
    protected GstApiRetryOptions RetryOptions { get; }

    /// <inheritdoc />
    public async Task<GstTaxPayer> GetTaxPayerAsync(string gstin, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(gstin))
        {
            throw new ArgumentException("GSTIN is required.", nameof(gstin));
        }

        var attempt = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                using (var request = await CreateRequestAsync(gstin, cancellationToken).ConfigureAwait(false))
                using (var response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false))
                {
                    var payload = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    EnsureSuccessStatus(response, payload);
                    return await ParseTaxPayerAsync(gstin, payload, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (Exception exception) when (ShouldRetry(exception, attempt))
            {
                attempt++;
                await Task.Delay(GetRetryDelay(attempt), cancellationToken).ConfigureAwait(false);
            }
            catch (Exception exception) when (!(exception is GstApiException))
            {
                throw WrapException(exception);
            }
        }
    }

    /// <summary>
    /// Creates the HTTP request for the given GSTIN.
    /// </summary>
    /// <param name="gstin">The GSTIN to query.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The request message.</returns>
    protected abstract Task<HttpRequestMessage> CreateRequestAsync(string gstin, CancellationToken cancellationToken);

    /// <summary>
    /// Parses the provider payload into a taxpayer model.
    /// </summary>
    /// <param name="gstin">The GSTIN that was queried.</param>
    /// <param name="payload">The raw provider payload.</param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    /// <returns>The parsed taxpayer model.</returns>
    protected abstract Task<GstTaxPayer> ParseTaxPayerAsync(string gstin, string payload, CancellationToken cancellationToken);

    /// <summary>
    /// Determines whether the current exception should be retried.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="attempt">The current zero-based attempt number.</param>
    /// <returns><see langword="true"/> when the operation should be retried; otherwise, <see langword="false"/>.</returns>
    protected virtual bool ShouldRetry(Exception exception, int attempt)
    {
        if (attempt >= RetryOptions.MaxRetries)
        {
            return false;
        }

        if (exception is HttpRequestException)
        {
            return true;
        }

        if (exception is GstApiException gstApiException)
        {
            return gstApiException.IsTransient;
        }

        return false;
    }

    /// <summary>
    /// Gets the retry delay for the specified attempt.
    /// </summary>
    /// <param name="attempt">The 1-based retry attempt.</param>
    /// <returns>The retry delay.</returns>
    protected virtual TimeSpan GetRetryDelay(int attempt)
    {
        var delay = RetryOptions.DelayMilliseconds <= 0 ? 250 : RetryOptions.DelayMilliseconds;
        return TimeSpan.FromMilliseconds(delay * attempt);
    }

    /// <summary>
    /// Converts non-success HTTP responses into provider exceptions.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="payload">The raw payload.</param>
    protected virtual void EnsureSuccessStatus(HttpResponseMessage response, string payload)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var statusCode = (int)response.StatusCode;
        var isTransient = response.StatusCode == HttpStatusCode.RequestTimeout || (int)response.StatusCode >= 500;
        var message = string.IsNullOrWhiteSpace(payload)
            ? $"Provider '{Name}' returned HTTP {statusCode}."
            : $"Provider '{Name}' returned HTTP {statusCode}: {payload}";

        throw new GstApiException(Name, message, statusCode, isTransient);
    }

    /// <summary>
    /// Wraps unexpected exceptions into a consistent provider exception.
    /// </summary>
    /// <param name="exception">The original exception.</param>
    /// <returns>A wrapped provider exception.</returns>
    protected virtual GstApiException WrapException(Exception exception)
    {
        if (exception is HttpRequestException httpRequestException)
        {
            return new GstApiException(Name, $"Provider '{Name}' request failed.", null, true, httpRequestException);
        }

        if (exception is TaskCanceledException)
        {
            return new GstApiException(Name, $"Provider '{Name}' request timed out or was canceled.", null, true, exception);
        }

        return new GstApiException(Name, $"Provider '{Name}' failed to process the GST request.", null, false, exception);
    }
}
