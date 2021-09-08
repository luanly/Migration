using Microsoft.Extensions.Hosting;
using Sustainsys.Saml2.Configuration;
using SwissAcademic.ApplicationInsights;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace SwissAcademic.Crm.Web
{
    public class ShibbolethBackgroundService
        :
        BackgroundService
    {
        internal static Dictionary<string, string> _metadataUrls;

        internal readonly static TimeSpan Delay = TimeSpan.FromHours(4);

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var isFirstCall = true;
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (ShibbolethIdentityProviderStore.AuthenticationOptions == null ||
                       ShibbolethIdentityProviderStore.AuthenticationOptions.SPOptions == null)
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1));
                        continue;
                    }

                    await RefreshMetadataAsync(null, stoppingToken, forceUseCache: isFirstCall);
                }
                catch (Exception ignored)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
                }
                isFirstCall = false;
                await Task.Delay(Delay, stoppingToken);
            }
        }

        internal static async Task RefreshMetadataAsync(SPOptions spOptions = null, CancellationToken cancellationToken = default, bool forceUseCache = false)
        {
            try
            {
                if (_metadataUrls == null)
                {
                    if (Environment.Build == BuildType.Alpha)
                    {
                        _metadataUrls = ShibbolethConstants.MetadataUrls_Test;
                    }
                    else
                    {
                        _metadataUrls = ShibbolethConstants.MetadataUrls_Production;
                    }
                }
                foreach (var item in _metadataUrls)
                {
                    try
                    {
                        using (var op = Telemetry.StartOperation($"RefreshShibbolethMetadata: {item.Key}"))
                        {
                            var ci = new CultureInfo(item.Value);
                            var region = new RegionInfo(ci.LCID);
                            var fed = new ShibbolethFederation(item.Key, region, spOptions ?? ShibbolethIdentityProviderStore.AuthenticationOptions.SPOptions);
                            await fed.LoadAsync(cancellationToken, forceUseCache);
                            if (cancellationToken.IsCancellationRequested)
                            {
                                return;
                            }

                            ShibbolethIdentityProviderStore.RegisterShibbolethFederation(fed);
                        }
                    }
                    catch (Exception ex)
                    {
                        Telemetry.TrackException(ex, SeverityLevel.Error, ExceptionFlow.Eat, property1: ("url", item.Key));
                    }
                }
            }
            catch (Exception ignored)
            {
                Telemetry.TrackException(ignored, SeverityLevel.Error, ExceptionFlow.Eat);
            }
        }
    }
}
